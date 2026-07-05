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

    public static async Task ArgumentWithSpaces_Should_Match()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "commit", "-m", "my commit message")
          .Returns("[main abc1234] my commit message");

        CommandOutput output = await Shell.Builder("git")
          .WithArguments("commit", "-m", "my commit message")
          .CaptureAsync();

        output.Stdout.ShouldContain("abc1234");
        CommandMock.VerifyCalled("git", "commit", "-m", "my commit message");
      }
    }

    public static async Task ArgumentWithPipeChar_Should_NotCollideWithSplitArguments()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("echo", "a|b").Returns("piped-arg");
        CommandMock.Setup("echo", "a", "b").Returns("two-args");

        CommandOutput output = await Shell.Builder("echo")
          .WithArguments("a|b")
          .CaptureAsync();

        output.Stdout.Trim().ShouldBe("piped-arg");
      }
    }

    public static async Task StrictMode_Should_ThrowOnUnmockedCommand()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "status").Returns("clean");

        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
          await Shell.Builder("git").WithArguments("push").CaptureAsync()
        );

        exception.Message.ShouldContain("strict mode");
      }
    }

    public static async Task LooseMode_Should_AllowUnmockedCommand()
    {
      using (CommandMock.Enable(MockBehavior.Loose))
      {
        CommandMock.Setup("git", "status").Returns("clean");

        CommandOutput output = await Shell.Builder("echo")
          .WithArguments("real execution")
          .CaptureAsync();

        output.Stdout.ShouldContain("real execution");
      }
    }

    public static async Task StreamStdout_Should_ReturnMockedLines()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "log", "--oneline")
          .Returns("abc1234 first\ndef5678 second");

        List<string> lines = [];
        await foreach (string line in Shell.Builder("git").WithArguments("log", "--oneline").Build().StreamStdoutAsync())
        {
          lines.Add(line);
        }

        lines.Count.ShouldBe(2);
        lines[0].ShouldContain("abc1234");
      }
    }

    public static async Task DelaysOnly_Should_RegisterSetup()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("sleep", "1").Delays(TimeSpan.FromMilliseconds(10));

        CommandOutput output = await Shell.Builder("sleep")
          .WithArguments("1")
          .CaptureAsync();

        output.Success.ShouldBeTrue();
        CommandMock.VerifyCalled("sleep", "1");
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
