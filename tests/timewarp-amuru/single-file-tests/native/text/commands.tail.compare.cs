#!/usr/bin/env -S dotnet --

#region Purpose
// For each accepted tail form, stdout and the exit code match the system tail.
#endregion

#region Design
// Byte comparison goes through CliWrap pipes. CommandOutput drops a trailing newline.
// Skipped: live -f on a file (it blocks), --pid, -s, -F, --help, and --version.
// Those are either rejected on purpose or handled outside this library.
// Bare +0 is skipped on this host: uutils 0.10.0 prints nothing for +0 and the whole
// file for -n +0. Tail follows -n +0, which is the whole file.
// A missing tail binary skips the file on Windows and fails on Linux and macOS.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextTailCompare_
{
using System.Globalization;
using System.Text;
using CliWrap;
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class Compare_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<Compare_Given_>();

  public static async Task LineForms_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    byte[] lines = SampleLines(12);
    await Compare(["-n", "3"], lines);
    await Compare(["-n3"], lines);
    await Compare(["--lines", "3"], lines);
    await Compare(["--lines=3"], lines);
    await Compare(["--lines=-3"], lines);
    await Compare(["-n", "-3"], lines);
    await Compare(["-n", "+3"], lines);
    await Compare(["--lines=+3"], lines);
    await Compare(["+3"], lines);
    await Compare(["-n", "+0"], lines);
    await Compare(["-n", "+1"], lines);
    await Compare(["-n", "0"], lines);
    await Compare(["-40"], lines);
    await Compare([], lines);
  }

  public static async Task ByteForms_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    byte[] lines = SampleLines(6);
    await Compare(["-c", "4"], lines);
    await Compare(["-c4"], lines);
    await Compare(["--bytes", "4"], lines);
    await Compare(["--bytes=4"], lines);
    await Compare(["-c", "+4"], lines);
    await Compare(["--bytes=+2"], lines);
    await Compare(["-c", "+0"], lines);
    await Compare(["-c", "1K"], lines);
    await Compare(["-c", "1KB"], lines);
    await Compare(["-c", "2b"], lines);
    await Compare(["-c", "0"], lines);
    await Compare(["-c", "999Y"], lines);
  }

  public static async Task HeaderForms_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      string second = await WriteAsync(directory, "b.txt", "d\n");
      string empty = await WriteAsync(directory, "empty.txt", "");
      await Compare(["-n", "1", first, second], null);
      await Compare(["-q", "-n", "1", first, second], null);
      await Compare(["-qn", "1", first, second], null);
      await Compare(["-v", "-n", "1", first], null);
      await Compare(["-v", "-q", "-n", "1", first, second], null);
      await Compare(["-q", "-v", "-n", "1", first], null);
      await Compare(["-n", "1", empty, first], null);
      await Compare([first, "-n", "1"], null);
      await Compare(["-n", "1", "--", first], null);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task ZeroTerminated_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    await Compare(["-z", "-n", "1"], "a\0b\0c"u8.ToArray());
    await Compare(["--zero-terminated", "-n", "2"], "a\0b\0c\0"u8.ToArray());
  }

  public static async Task PartialAndBinary_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    await Compare(["-n", "1"], "a\nb"u8.ToArray());
    await Compare(["-n", "1"], "a\n\n"u8.ToArray());
    await Compare(["-c", "2"], new byte[] { 0x00, 0xFF, 0xFE, 0x61 });
  }

  public static async Task FileErrors_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    string directory = NewDirectory();
    try
    {
      string notes = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      string missing = Path.Combine(directory, "missing.txt");
      await Compare(["-n", "1", missing], null, compareStderr: true);
      await Compare(["-n", "1", missing, notes], null, compareStderr: true);
      await Compare(["-n", "1", directory], null, compareStderr: true);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task FollowOnAPipe_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    await Compare(["-f", "-n", "1"], SampleLines(4));
    await Compare(["-fn", "2"], SampleLines(4));
  }

  public static async Task RepeatedOption_Should_MatchSystemTail()
  {
    if (!await SystemTailExists())
    {
      return;
    }

    await Compare(["-n", "1", "-n", "2"], SampleLines(5));
    await Compare(["-n", "4", "-c", "3"], SampleLines(5));
  }

  private static int TailState;
  private static readonly SemaphoreSlim TailGate = new(1, 1);

  private static async Task<bool> SystemTailExists()
  {
    if (Volatile.Read(ref TailState) != 0)
    {
      return TailState > 0;
    }

    await TailGate.WaitAsync();
    try
    {
      if (TailState != 0)
      {
        return TailState > 0;
      }

      bool found = await ProbeTail();
      TailState = found ? 1 : -1;
      return found;
    }
    finally
    {
      TailGate.Release();
    }
  }

  private static async Task<bool> ProbeTail()
  {
    try
    {
      using MemoryStream versionOut = new();
      using MemoryStream versionErr = new();
      CommandResult result = await Cli.Wrap("tail")
        .WithArguments(["--version"])
        .WithStandardOutputPipe(PipeTarget.ToStream(versionOut))
        .WithStandardErrorPipe(PipeTarget.ToStream(versionErr))
        .WithValidation(CommandResultValidation.None)
        .ExecuteAsync();
      if (result.ExitCode == 0)
      {
        return true;
      }

      if (OperatingSystem.IsWindows())
      {
        return false;
      }

      throw new InvalidOperationException(
        "tail --version exited " + result.ExitCode
        + " stdout=" + Encoding.UTF8.GetString(versionOut.ToArray())
        + " stderr=" + Encoding.UTF8.GetString(versionErr.ToArray()));
    }
    catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or FileNotFoundException)
    {
      if (OperatingSystem.IsWindows())
      {
        return false;
      }

      throw new InvalidOperationException("tail is not on PATH. Comparison on Linux and macOS needs it.", exception);
    }
  }

  private static async Task Compare(string[] arguments, byte[]? stdin, bool compareStderr = false)
  {
    using MemoryStream expectedOut = new();
    using MemoryStream expectedErr = new();
    Command command = Cli.Wrap("tail")
      .WithArguments(arguments)
      .WithStandardOutputPipe(PipeTarget.ToStream(expectedOut))
      .WithStandardErrorPipe(PipeTarget.ToStream(expectedErr))
      .WithValidation(CommandResultValidation.None);
    if (stdin is not null)
    {
      command = command.WithStandardInputPipe(PipeSource.FromBytes(stdin));
    }

    CommandResult system = await command.ExecuteAsync();
    using MemoryStream input = new(stdin ?? []);
    Text.TailOutput ours = Text.Commands.Tail(arguments, input, CancellationToken.None);
    string label = arguments.Length == 0 ? "(no arguments)" : string.Join(' ', arguments);
    ours.ExitCode.ShouldBe(system.ExitCode, label);
    ours.Stdout.ToArray().ShouldBe(expectedOut.ToArray(), label);
    if (compareStderr)
    {
      ours.Stderr.ToArray().ShouldBe(expectedErr.ToArray(), label);
    }
  }

  private static byte[] SampleLines(int count)
  {
    using MemoryStream stream = new();
    for (int line = 1; line <= count; line++)
    {
      stream.Write(Encoding.ASCII.GetBytes(line.ToString(CultureInfo.InvariantCulture)));
      stream.WriteByte((byte)'\n');
    }

    return stream.ToArray();
  }

  private static string NewDirectory()
  {
    string path = Path.Combine(Path.GetTempPath(), "amuru-text-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
  }

  private static async Task<string> WriteAsync(string directory, string name, string content)
  {
    string path = Path.Combine(directory, name);
    await File.WriteAllTextAsync(path, content);
    return path;
  }
}
}
