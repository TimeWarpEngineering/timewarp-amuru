#region Purpose
// Construction state and file-backed standard streams for CommandResult:
// standard input read from a file, and a byte-level copy of standard output to a file (the tee).
#endregion

#region Design
// - The constructors live here so command-result.cs stays inside its line-cap baseline.
// - StandardInputFile is attached to the CliWrap command as PipeSource.FromFile when the command is built.
//   It counts as configured standard input: PassthroughAsync keeps it, TtyPassthroughAsync refuses it,
//   and a matched mock records the file's text, read when the call matches.
//   A file that does not exist throws FileNotFoundException when the command runs.
// - StandardOutputTeeFile is not attached when the command is built. Each execution mode puts a tee target
//   in front of its own stdout target (WithTeeStandardOutput). The listen paths merge InternalCommand's
//   stdout pipe, so they go through TeeCommand. CliWrap's AggregatePipeTarget hands each chunk to its targets
//   one at a time. The tee is first, so it gets a chunk before a line target that waits for a newline or a
//   slow stream reader.
// - The tee copies raw bytes. It does not decode or split lines, so an unterminated last line is in the file
//   as soon as the child writes it. The FileStream is unbuffered and flushed per chunk, and it is opened with
//   FileShare.ReadWrite | FileShare.Delete so another reader can open or tail it during the run.
//   The file is created (truncated) when the run starts piping stdout.
// - Each chunk is written with CancellationToken.None, so a cancelled or timed-out run keeps every byte read.
// - stderr never goes to the tee.
// - Pipe compositions carry both. The file stays the pipeline stdin (like the string form), and the tee
//   records the last stage's stdout, which is the pipeline output.
// - Mocks: the tee file is created when the setup matches. The setup's Stdout is written after the
//   delay and exception prelude, or after the mock timeout wait. A cancelled mock leaves the file empty.
// - TtyPassthroughAsync refuses a tee: the child inherits the terminal, so its stdout cannot be copied.
#endregion

namespace TimeWarp.Amuru;

public partial class CommandResult
{
  private const int TeeBufferSize = 81920;

  private const string TtyStandardOutputTeeMessage =
    "TtyPassthroughAsync inherits the terminal; a standard output tee file cannot be used with a TTY";

  // Null: stdin is not a file. Otherwise an absolute path already attached to the CliWrap command.
  private string? StandardInputFile { get; }

  // Null: no tee. Otherwise an absolute path that each execution mode copies stdout bytes into.
  private string? StandardOutputTeeFile { get; }

  private bool HasConfiguredStandardInput => ConfiguredStandardInput is not null || StandardInputFile is not null;

  private CommandResult(
    Command? command,
    string? configuredStandardInput,
    bool isPipeline,
    string? mockExecutable,
    string[]? mockArguments,
    TimeSpan? timeout,
    TimeSpan timeoutGracePeriod,
    CommandResultValidation validation,
    string? standardInputFile,
    string? standardOutputTeeFile)
  {
    InternalCommand = command;
    ConfiguredStandardInput = configuredStandardInput;
    IsPipeline = isPipeline;
    MockExecutable = mockExecutable;
    MockArguments = mockArguments;
    Timeout = timeout;
    TimeoutGracePeriod = timeoutGracePeriod;
    Validation = validation;
    StandardInputFile = standardInputFile;
    StandardOutputTeeFile = standardOutputTeeFile;
  }

  internal CommandResult(Command? command)
    : this(
      command,
      configuredStandardInput: null,
      isPipeline: false,
      mockExecutable: null,
      mockArguments: null,
      timeout: null,
      timeoutGracePeriod: CommandOptions.DefaultTimeoutGracePeriod,
      validation: CommandResultValidation.None,
      standardInputFile: null,
      standardOutputTeeFile: null)
  {
  }

  internal CommandResult(
    Command? command,
    string executable,
    string[] arguments,
    string? configuredStandardInput,
    CommandOptions options,
    string? standardInputFile = null,
    string? standardOutputTeeFile = null)
    : this(
      command,
      configuredStandardInput,
      isPipeline: false,
      executable,
      arguments,
      options.Timeout,
      options.TimeoutGracePeriod,
      options.Validation ?? CommandResultValidation.None,
      standardInputFile,
      standardOutputTeeFile)
  {
  }

  /// <summary>
  /// The standard input a matched mock records: the configured text, or the text of the configured file.
  /// </summary>
  private string? StandardInputForMock() =>
    StandardInputFile is null ? ConfiguredStandardInput : File.ReadAllText(StandardInputFile);

  /// <summary>
  /// Returns <paramref name="target"/>, preceded by the tee target when a tee file is configured.
  /// </summary>
  private PipeTarget WithTeeStandardOutput(PipeTarget target) =>
    StandardOutputTeeFile is null
      ? target
      : PipeTarget.Merge(CreateTeeTarget(StandardOutputTeeFile), target);

  /// <summary>
  /// InternalCommand with the tee in front of its own stdout pipe, for the CliWrap listen paths.
  /// </summary>
  private Command TeeCommand =>
    StandardOutputTeeFile is null
      ? InternalCommand!
      : InternalCommand!.WithStandardOutputPipe(WithTeeStandardOutput(InternalCommand.StandardOutputPipe));

  private void ThrowIfTtyCannotUseStandardStreams()
  {
    if (HasConfiguredStandardInput)
    {
      throw new InvalidOperationException(TtyConfiguredStandardInputMessage);
    }

    if (StandardOutputTeeFile is not null)
    {
      throw new InvalidOperationException(TtyStandardOutputTeeMessage);
    }
  }

  private void CreateMockTeeFile()
  {
    if (StandardOutputTeeFile is null)
    {
      return;
    }

    OpenTeeFile(StandardOutputTeeFile).Dispose();
  }

  private async Task WriteMockTeeAsync(Testing.MockSetupData setup)
  {
    if (StandardOutputTeeFile is null || string.IsNullOrEmpty(setup.Stdout))
    {
      return;
    }

    byte[] bytes = Encoding.UTF8.GetBytes(setup.Stdout);
    FileStream file = OpenTeeFile(StandardOutputTeeFile);
    await using ConfiguredAsyncDisposable fileScope = file.ConfigureAwait(false);
    await file.WriteAsync(bytes, CancellationToken.None).ConfigureAwait(false);
  }

  private static FileStream OpenTeeFile(string filePath) =>
    new(
      filePath,
      FileMode.Create,
      FileAccess.Write,
      FileShare.ReadWrite | FileShare.Delete,
      bufferSize: 0,
      FileOptions.Asynchronous);

  private static PipeTarget CreateTeeTarget(string filePath) =>
    PipeTarget.Create(async (origin, cancellationToken) =>
    {
      FileStream file = OpenTeeFile(filePath);
      await using ConfiguredAsyncDisposable fileScope = file.ConfigureAwait(false);
      byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(TeeBufferSize);
      try
      {
        while (true)
        {
          int read = await origin.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
          if (read <= 0)
          {
            break;
          }

          // A chunk already read is written whole, even when the run is being cancelled.
          await file.WriteAsync(buffer.AsMemory(0, read), CancellationToken.None).ConfigureAwait(false);
          await file.FlushAsync(CancellationToken.None).ConfigureAwait(false);
        }
      }
      finally
      {
        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
      }
    });
}
