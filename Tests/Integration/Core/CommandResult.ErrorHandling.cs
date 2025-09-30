#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<ErrorHandlingTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class ErrorHandlingTests
{

  public static async Task TestNonExistentCommandWithNoValidation()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("nonexistentcommand12345").WithNoValidation().CaptureAsync()
    );
  }

  public static async Task TestCommandWithNonZeroExitCodeAndNoValidation()
  {
    string[] lsArgs = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs).WithNoValidation().CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
  }

  public static async Task TestExecuteAsyncThrowsOnNonZeroExit()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
  }

  public static async Task TestGetLinesAsyncWithNoValidation()
  {
    string[] lsArgs2 = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs2).WithNoValidation().CaptureAsync();
    string[] lines = output.GetStdoutLines(); // Use GetStdoutLines() to only get stdout

    lines.Length.ShouldBe(0);
  }

  public static async Task TestSpecialCharactersInArguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello \"World\" with 'quotes' and $pecial chars!").CaptureAsync();

    output.Stdout.ShouldNotBeNullOrEmpty();
  }

  public static async Task TestEmptyCommandReturnsEmptyString()
  {
    CommandOutput output = await Shell.Builder("").CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
  }

  public static async Task TestWhitespaceCommandReturnsEmptyString()
  {
    CommandOutput output = await Shell.Builder("   ").CaptureAsync();

    output.Stdout.ShouldBeNullOrEmpty();
  }

  public static async Task TestDefaultGetStringThrowsOnError()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
  }

  public static async Task TestDefaultGetLinesThrowsOnError()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
    );
  }

  public static async Task TestExecuteAsyncWithNoValidation()
  {
    string[] lsArgs3 = ["/nonexistent/path/12345"];
    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs3).WithNoValidation().CaptureAsync();

    output.Success.ShouldBeFalse();
  }
}