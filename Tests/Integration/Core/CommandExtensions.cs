#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<BasicCommandTests>();

internal sealed class BasicCommandTests
{
  public static async Task TestSimpleEchoCommand()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();

    output.Stdout.Trim().ShouldBe("Hello World");
  }

  public static async Task TestCommandWithMultipleArguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("arg1", "arg2", "arg3").CaptureAsync();

    output.Stdout.Trim().ShouldBe("arg1 arg2 arg3");
  }

  public static async Task TestExecuteAsyncDoesNotThrow()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();

    // Test passes if no exception is thrown
    output.Success.ShouldBeTrue();
  }

  public static async Task TestDateCommand()
  {
    CommandOutput output = await Shell.Builder("date").CaptureAsync();

    output.Stdout.Trim().ShouldNotBeNullOrEmpty();
  }
}