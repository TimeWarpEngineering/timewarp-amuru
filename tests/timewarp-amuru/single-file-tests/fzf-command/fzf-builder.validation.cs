#!/usr/bin/env -S dotnet --

#region Purpose
// Proves FzfBuilder reports a non-matching filter by default and throws under WithZeroExitCodeValidation.
#endregion

#region Design
// Filter mode is non-interactive. A query that matches nothing exits 1 without waiting for a terminal.
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
  }
}
