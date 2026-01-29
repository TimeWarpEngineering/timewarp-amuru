#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu@1.0.0-beta.8
#:package TimeWarp.Amuru@1.0.0-beta.18

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using Shouldly;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandExtensionsTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandExtensionsTests>();

  public static async Task Should_execute_simple_echo_command()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();

    output.Stdout.Trim().ShouldBe("Hello World");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_command_with_multiple_arguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("arg1", "arg2", "arg3").CaptureAsync();

    output.Stdout.Trim().ShouldBe("arg1 arg2 arg3");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_async_without_throwing()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();

    // Test passes if no exception is thrown
    output.Success.ShouldBeTrue();
    await Task.CompletedTask;
  }

  public static async Task Should_execute_date_command()
  {
    CommandOutput output = await Shell.Builder("date").CaptureAsync();

    output.Stdout.Trim().ShouldNotBeNullOrEmpty();
    await Task.CompletedTask;
  }
}
