#!/usr/bin/dotnet --

#region Purpose
// Tests for CommandMock - validates mocking command execution for testing
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// CommandMock allows mocking command responses without executing real commands
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandMock_
{
  [TestTag("Core")]
  public class Enable_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Enable_Given_>();

    public static async Task BasicSetup_Should_ReturnMockedOutput()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "status")
          .Returns("On branch main\nnothing to commit", "");

        CommandOutput output = await Shell.Builder("git")
          .WithArguments("status")
          .CaptureAsync();

        output.Stdout.ShouldContain("On branch main");
        CommandMock.VerifyCalled("git", "status");
      }
    }

    public static async Task ErrorSetup_Should_ReturnErrorResponse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "push")
          .ReturnsError("remote: Permission denied", 128);

        CommandOutput output = await Shell.Builder("git")
          .WithArguments("push")
          .WithNoValidation()
          .CaptureAsync();

        output.Stderr.ShouldContain("Permission denied");
        output.ExitCode.ShouldBe(128);
      }
    }

    public static async Task MultipleScopes_Should_BeIsolated()
    {
      // First mock scope
      using (CommandMock.Enable())
      {
        CommandMock.Setup("echo", "test1")
          .Returns("mocked1");

        CommandOutput result1 = await Shell.Builder("echo")
          .WithArguments("test1")
          .CaptureAsync();

        result1.Stdout.Trim().ShouldBe("mocked1");
      }

      // Second mock scope - should be isolated
      using (CommandMock.Enable())
      {
        CommandMock.Setup("echo", "test2")
          .Returns("mocked2");

        CommandOutput result2 = await Shell.Builder("echo")
          .WithArguments("test2")
          .CaptureAsync();

        result2.Stdout.Trim().ShouldBe("mocked2");

        // test1 should not be mocked in this scope
        CommandMock.CallCount("echo", "test1").ShouldBe(0);
      }
    }
  }
}
