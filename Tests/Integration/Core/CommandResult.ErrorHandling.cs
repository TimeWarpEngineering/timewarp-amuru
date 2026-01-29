#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using Shouldly;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandResultErrorHandlingTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandResultErrorHandlingTests>();

  public static async Task Should_handle_non_existent_command_with_no_validation()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("nonexistentcommand12345").WithNoValidation().CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_handle_command_with_non_zero_exit_code_and_no_validation()
  {
    string[] lsArgs = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs).WithNoValidation().CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_throw_on_non_zero_exit()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_get_lines_with_no_validation()
  {
    string[] lsArgs2 = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs2).WithNoValidation().CaptureAsync();
    string[] lines = output.GetStdoutLines(); // Use GetStdoutLines() to only get stdout

    lines.Length.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_handle_special_characters_in_arguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello \"World\" with 'quotes' and $pecial chars!").CaptureAsync();

    output.Stdout.ShouldNotBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_return_empty_string_for_empty_command()
  {
    CommandOutput output = await Shell.Builder("").CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_return_empty_string_for_whitespace_command()
  {
    CommandOutput output = await Shell.Builder("   ").CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_throw_on_error_for_default_get_string()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_throw_on_error_for_default_get_lines()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_execute_with_no_validation()
  {
    string[] lsArgs3 = ["/nonexistent/path/12345"];
    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs3).WithNoValidation().CaptureAsync();

    output.Success.ShouldBeFalse();
    await Task.CompletedTask;
  }
}
