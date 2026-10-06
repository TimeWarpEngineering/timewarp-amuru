#!/usr/bin/env -S dotnet --

#region Purpose
// Proves DotNetBuilder.WithTimeout is on the base builder and reaches execution.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Leaf builders do not grow a WithTimeout method in this task. The base builder does.
// dotnet --version is a fast success path, so TimedOut stays false.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class WithTimeout_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithTimeout_Given_>();

    [Timeout(30000)]
    public static async Task BaseBuilder_Should_RunWithinTheTimeout()
    {
      CommandOutput output = await DotNet.Builder()
        .WithArguments("--version")
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithTimeoutGracePeriod(TimeSpan.FromSeconds(1))
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
        .Build()
        .CaptureAsync();

      output.Success.ShouldBeTrue();
      output.TimedOut.ShouldBeFalse();
      output.Stdout.ShouldNotBeNullOrEmpty();
    }
  }
}
