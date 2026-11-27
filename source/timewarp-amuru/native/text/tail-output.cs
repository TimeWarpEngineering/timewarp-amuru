#region Purpose
// Byte-preserving result of a Tail run.
#endregion

#region Design
// CommandOutput splits on newlines and drops a trailing newline, so it cannot
// represent tail's stdout. Callers that need the slice unchanged read Stdout.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Stdout and stderr bytes from <see cref="Commands.Tail(IReadOnlyList{string})"/>.
/// </summary>
public sealed class TailOutput
{
  /// <summary>
  /// Initializes a new instance of the <see cref="TailOutput"/> class.
  /// </summary>
  public TailOutput(
    ReadOnlyMemory<byte> stdout,
    ReadOnlyMemory<byte> stderr,
    int exitCode,
    string? savedPath,
    bool helpRequested,
    bool versionRequested)
  {
    Stdout = stdout;
    Stderr = stderr;
    ExitCode = exitCode;
    SavedPath = savedPath;
    HelpRequested = helpRequested;
    VersionRequested = versionRequested;
  }

  /// <summary>Gets the exact stdout bytes.</summary>
  public ReadOnlyMemory<byte> Stdout { get; }

  /// <summary>Gets the exact stderr bytes.</summary>
  public ReadOnlyMemory<byte> Stderr { get; }

  /// <summary>Gets the exit code. Zero is success. One is a usage or file error.</summary>
  public int ExitCode { get; }

  /// <summary>Gets a value indicating that <see cref="ExitCode"/> is zero.</summary>
  public bool Success => ExitCode == 0;

  /// <summary>
  /// Gets the saved stdin file when lines were cut or the save hit its limit.
  /// <see langword="null"/> when nothing was saved.
  /// </summary>
  public string? SavedPath { get; }

  /// <summary>Gets a value indicating that argv asked for help.</summary>
  public bool HelpRequested { get; }

  /// <summary>Gets a value indicating that argv asked for the version.</summary>
  public bool VersionRequested { get; }

  /// <summary>Gets <see cref="Stdout"/> decoded as UTF-8.</summary>
  public string StdoutText => Encoding.UTF8.GetString(Stdout.Span);

  /// <summary>Gets <see cref="Stderr"/> decoded as UTF-8.</summary>
  public string StderrText => Encoding.UTF8.GetString(Stderr.Span);
}
