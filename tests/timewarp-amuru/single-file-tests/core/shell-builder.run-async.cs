#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.RunAsync() - validates streaming execution with exit codes
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// RunAsync streams output to console and returns exit code
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class RunAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RunAsync_Given_>();

    public static async Task FailingCommandWithNoValidation_Should_ReturnExitCode()
    {
      int exitCode = await Shell.Builder("false")
        .WithNoValidation()
        .RunAsync();

      exitCode.ShouldBe(1);
    }
  }
}
