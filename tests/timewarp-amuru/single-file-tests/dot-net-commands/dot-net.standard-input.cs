#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for WithStandardInput on DotNet.Run and DotNet.Builder. Task 154.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The text is not a command-line token. A mock records what Shell.Run received.
// An empty string is immediate EOF. Null is rejected. Omitting the call leaves stdin unset.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNetRunBuilder_
{
  [TestTag("DotNetCommands")]
  public class WithStandardInput_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithStandardInput_Given_>();

    public static async Task Text_Should_ReachTheChildAndStayOffTheCommandString()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "run", "--file", "app.cs", "--", "serve")
          .Returns("ok");

        DotNetRunBuilder builder = DotNet.Run()
          .WithFile("app.cs")
          .WithArguments("serve")
          .WithStandardInput("{\"name\":\"a\"}");

        builder.Build().ToCommandString().ShouldBe("dotnet run --file app.cs -- serve");

        CommandOutput output = await builder.Build().CaptureAsync();

        output.Stdout.Trim().ShouldBe("ok");
        CommandMock.GetConfiguredStandardInput("dotnet", "run", "--file", "app.cs", "--", "serve")
          .ShouldBe("{\"name\":\"a\"}");
      }
    }

    public static async Task Empty_Should_SendImmediateEof()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "run").Returns(string.Empty);

        await DotNet.Run().WithStandardInput("").Build().CaptureAsync();

        CommandMock.GetConfiguredStandardInput("dotnet", "run").ShouldBe(string.Empty);
      }
    }

    public static async Task Omitted_Should_LeaveStandardInputUnset()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "run").Returns(string.Empty);

        await DotNet.Run().Build().CaptureAsync();

        CommandMock.GetConfiguredStandardInput("dotnet", "run").ShouldBeNull();
      }
    }

    public static async Task Null_Should_Throw()
    {
      Should.Throw<ArgumentNullException>(() => DotNet.Run().WithStandardInput(null!));

      await Task.CompletedTask;
    }
  }
}

namespace DotNetBuilder_
{
  [TestTag("DotNetCommands")]
  public class WithStandardInput_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithStandardInput_Given_>();

    public static async Task Text_Should_ReachTheChildAndStayOffTheCommandString()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("dotnet", "--version").Returns("9.0.0");

        DotNetBuilder builder = DotNet.Builder()
          .WithArguments("--version")
          .WithStandardInput("payload");

        builder.Build().ToCommandString().ShouldBe("dotnet --version");

        await builder.Build().CaptureAsync();

        CommandMock.GetConfiguredStandardInput("dotnet", "--version").ShouldBe("payload");
      }
    }

    public static async Task Null_Should_Throw()
    {
      Should.Throw<ArgumentNullException>(() => DotNet.Builder().WithStandardInput(null!));

      await Task.CompletedTask;
    }
  }
}
