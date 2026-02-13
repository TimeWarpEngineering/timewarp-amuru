#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.PassthroughAsync() - validates interactive passthrough execution
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// PassthroughAsync connects stdin/stdout/stderr to console for interactive commands
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class PassthroughAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<PassthroughAsync_Given_>();

    public static async Task EchoCommand_Should_ReturnZeroExitCode()
    {
      ExecutionResult result = await Shell.Builder("echo")
        .WithArguments("Hello from interactive mode")
        .PassthroughAsync();

      result.ExitCode.ShouldBe(0);
      result.StandardOutput.ShouldBeNullOrEmpty();
    }

    public static async Task EmptyCommand_Should_ReturnZeroExitCode()
    {
      CommandResult nullCommand = Shell.Builder("").Build();

      ExecutionResult execResult = await nullCommand.PassthroughAsync();

      execResult.ExitCode.ShouldBe(0);
    }
  }
}
