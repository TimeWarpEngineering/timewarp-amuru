#!/usr/bin/env -S dotnet --

#region Purpose
// Proves FzfBuilder reports a non-matching filter by default and throws under WithZeroExitCodeValidation.
#endregion

#region Design
// Filter mode is non-interactive. A query that matches nothing exits 1 without waiting for a terminal.
// Needs a real fzf binary: the test returns early when fzf is not on PATH (CI runners lack it),
// matching the guard used by fzf-extensions.select-with-fzf.cs.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class Validation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Validation_Given_>();

    [Timeout(30000)]
    public static async Task FilterMiss_Should_ReportFailureUnlessZeroExitValidation()
    {
      if (ResolveFzf() is null)
      {
        // dot-net.validation.cs proves the shared validation contract on machines without fzf.
        return;
      }

      CliConfiguration.ClearCommandPath("fzf");

      CommandOutput output = await Fzf.Builder()
        .WithFilter("no-such-item")
        .FromInput("alpha", "beta")
        .Build()
        .CaptureAsync();

      output.Success.ShouldBeFalse();
      output.ExitCode.ShouldNotBe(0);

      await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(async () =>
        await Fzf.Builder()
          .WithFilter("no-such-item")
          .FromInput("alpha", "beta")
          .WithZeroExitCodeValidation()
          .Build()
          .CaptureAsync());
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
  }
}
