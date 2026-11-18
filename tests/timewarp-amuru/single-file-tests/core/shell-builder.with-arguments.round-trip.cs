#!/usr/bin/env -S dotnet --
#:package CsCheck

#region Purpose
// Property test: every argument list passed to ShellBuilder.WithArguments reaches the child
// process unchanged. Amuru has no quoting layer of its own; CliWrap's ArgumentsBuilder does the
// per-platform escaping. This test pins that contract, including across a CliWrap swap (task 119).
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The child is core/argv-echo/argv-echo.cs, built once into a temp folder and run as a real
// process. It prints the argument count, then the Base64 of each argument's UTF-8 bytes.
// Generators mix printable ASCII, shell and quoting metacharacters, BMP unicode, and astral
// code points. They never produce NUL or a lone surrogate: neither can cross an OS argv.
// Seed: CsCheck reads CsCheck_Seed (CI pins it in the workflow). A failure message carries the
// shrunk case's seed, and the catch below prints it with the pinned seed for replay.
// CsCheck's Faster is not used here; timing checks are flaky on CI runners.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  using CsCheck;

  [TestTag("Core")]
  public class WithArgumentsRoundTrip_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithArgumentsRoundTrip_Given_>();

    // Characters that a quoting layer has to treat specially on Windows or a POSIX shell.
    private const string EdgeCharacters = " \t\n\r\"'\\`$&|;<>(){}[]*?!#~%^=,@";

    private static readonly Gen<string> ArgumentPiece = Gen.Frequency(
      (4, Gen.Char[' ', '~'].Select(character => character.ToString())),
      (4, Gen.Char[EdgeCharacters].Select(character => character.ToString())),
      (1, Gen.Char[' ', '퟿'].Select(character => character.ToString())),
      (1, Gen.Int[0x10000, 0x10FFFF].Select(char.ConvertFromUtf32))
    );

    private static readonly Gen<string> Argument = ArgumentPiece.Array[0, 12].Select(string.Concat);

    private static readonly Gen<string[]> ArgumentList = Argument.Array[0, 6];

    public static async Task KnownEdgeCases_Should_ReachChildUnchanged()
    {
      string echo = await ArgvEchoPathAsync();
      string[] arguments =
      [
        "",
        " ",
        "a b",
        "\"",
        "\"\"",
        "\\",
        "\\\\",
        "trailing\\",
        "trailing\\\\",
        "a\\\"b",
        "\\\"quoted\\\"",
        "'single'",
        "$HOME",
        "`whoami`",
        "$(id)",
        "; rm -rf / &",
        "a|b>c<d",
        "*?[]{}",
        "tab\there",
        "line\nbreak",
        "日本語",
        "emoji 😀",
        "-",
        "--",
        "",
      ];

      string[] received = await EchoAsync(echo, arguments);

      received.ShouldBe(arguments);
    }

    public static async Task EmptyList_Should_ReachChildAsNoArguments()
    {
      string echo = await ArgvEchoPathAsync();

      string[] received = await EchoAsync(echo, []);

      received.ShouldBeEmpty();
    }

    public static async Task TrailingEmptyArguments_Should_ReachChild()
    {
      string echo = await ArgvEchoPathAsync();
      string[] arguments = ["a", "", ""];

      string[] received = await EchoAsync(echo, arguments);

      received.ShouldBe(arguments);
    }

    public static async Task RandomArguments_Should_ReachChildUnchanged()
    {
      string echo = await ArgvEchoPathAsync();
      try
      {
        await ArgumentList.SampleAsync(
          async arguments =>
          {
            string[] received = await EchoAsync(echo, arguments);
            received.ShouldBe(arguments);
          },
          writeLine: line => TimeWarpTerminal.Default.WriteLine(line),
          threads: 4,
          print: Print
        );
      }
      catch (CsCheckException exception)
      {
        string pinned = Environment.GetEnvironmentVariable("CsCheck_Seed") ?? "(unset)";
        await TimeWarpTerminal.Default.WriteLineAsync(
          $"CsCheck failure. Pinned CsCheck_Seed={pinned}. Replay the shrunk case with the seed in: {exception.Message}"
        );
        throw;
      }
    }

    private static async Task<string[]> EchoAsync(string echo, string[] arguments)
    {
      CommandOutput output = await Shell.Builder(echo)
        .WithArguments(arguments)
        .WithNoValidation()
        .CaptureAsync();
      if (output.ExitCode != 0)
      {
        throw new InvalidOperationException($"argv-echo exited {output.ExitCode}: {output.Stderr}");
      }

      string[] lines = output.Stdout.Split('\n');
      int count = int.Parse(lines[0], System.Globalization.CultureInfo.InvariantCulture);
      lines.Length.ShouldBe(count + 2, $"argv-echo report: {output.Stdout}");
      lines[count + 1].ShouldBe("end");
      string[] received = new string[count];
      for (int index = 0; index < count; index++)
      {
        received[index] = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(lines[index + 1]));
      }

      return received;
    }

    // Escaped form so a failure report shows control characters and the list boundaries.
    private static string Print(string[] arguments) =>
      "[" + string.Join(", ", arguments.Select(Quote)) + "]";

    private static string Quote(string argument)
    {
      System.Text.StringBuilder quoted = new("\"");
      foreach (char character in argument)
      {
        quoted.Append(character switch
        {
          '"' => "\\\"",
          '\\' => "\\\\",
          '\t' => "\\t",
          '\n' => "\\n",
          '\r' => "\\r",
          _ => character.ToString()
        });
      }

      return quoted.Append('"').ToString();
    }

    private static async Task<string> ArgvEchoPathAsync()
    {
      string? root = Git.FindRoot();
      root.ShouldNotBeNull();
      string source = Path.Combine(root, "tests", "timewarp-amuru", "single-file-tests", "core", "argv-echo", "argv-echo.cs");
      string outDir = Path.Combine(Path.GetTempPath(), "timewarp-amuru-128-argv-echo");
      string built = Path.Combine(outDir, OperatingSystem.IsWindows() ? "argv-echo.exe" : "argv-echo");
      if (File.Exists(built) && File.GetLastWriteTimeUtc(built) >= File.GetLastWriteTimeUtc(source))
      {
        return built;
      }

      Directory.CreateDirectory(outDir);
      CommandOutput output = await DotNet.Build(source)
        .WithOutput(outDir)
        .WithConfiguration("Release")
        .WithNoLogo()
        .WithVerbosity("q")
        .WithNoValidation()
        .CaptureAsync();
      if (output.ExitCode != 0)
      {
        throw new InvalidOperationException(output.Combined);
      }

      return built;
    }
  }
}
