#!/usr/bin/env -S dotnet --

#region Purpose
// Tests that SelectWithFzf forwards configured fzf options to the fzf process.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// A stand-in fzf executable prints its argv. ToCommandString cannot see this:
// the old ExtractFzfArguments stub still built a command string of plain "fzf".
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfExtensions_
{
  [TestTag("FzfCommand")]
  public class SelectWithFzf_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SelectWithFzf_Given_>();

    public static async Task ConfiguredOptions_Should_ReachTheFzfProcess()
    {
      string mockFzfPath = await CreateArgumentEchoMock();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        CommandOutput output = await Shell.Builder("printf")
          .WithArguments("%s", "alpha\nbeta\nalpine\n")
          .Build()
          .SelectWithFzf(fzf => fzf.WithMulti().WithHeight(20).WithPrompt("Select output: "))
          .CaptureAsync();

        output.Success.ShouldBeTrue(output.Stderr);
        string[] lines = output.GetLines();
        lines.ShouldContain("--multi");
        lines.ShouldContain("--height=20");
        lines.ShouldContain("--prompt=Select output: ");
      }
      finally
      {
        CliConfiguration.Reset();
        if (File.Exists(mockFzfPath))
        {
          File.Delete(mockFzfPath);
        }
      }
    }

    public static async Task FilterOption_Should_FilterRealFzfMatches()
    {
      if (ResolveFzf() is null)
      {
        // ConfiguredOptions_Should_ReachTheFzfProcess covers option pass-through
        // on machines that do not have the fzf binary.
        return;
      }

      try
      {
        CliConfiguration.ClearCommandPath("fzf");

        CommandOutput output = await Shell.Builder("printf")
          .WithArguments("%s", "alpha\nbeta\nalpine\n")
          .Build()
          .SelectWithFzf(fzf => fzf.WithFilter("alp").WithPrintQuery().WithHeight(20))
          .CaptureAsync();

        output.Success.ShouldBeTrue(output.Stderr);
        string[] lines = output.GetLines();
        lines[0].ShouldBe("alp");
        lines.ShouldContain("alpha");
        lines.ShouldContain("alpine");
        lines.ShouldNotContain("beta");
      }
      finally
      {
        CliConfiguration.Reset();
      }
    }

    private static string? ResolveFzf()
    {
      string pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
      foreach (string directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
      {
        string candidate = Path.Combine(directory, OperatingSystem.IsWindows() ? "fzf.exe" : "fzf");
        if (File.Exists(candidate))
        {
          return candidate;
        }
      }

      return null;
    }

    private static async Task<string> CreateArgumentEchoMock()
    {
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath += ".sh";

      const string mockScript = "#!/bin/sh\nprintf '%s\\n' \"$@\"\n";
      await File.WriteAllTextAsync(mockPath, mockScript);

      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        await Shell.Builder("chmod")
          .WithArguments("+x", mockPath)
          .RunAsync();
      }

      return mockPath;
    }
  }
}
