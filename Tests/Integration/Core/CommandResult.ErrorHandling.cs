#!/usr/bin/dotnet run

await RunTests<ErrorHandlingTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class ErrorHandlingTests
{

  public static async Task TestNonExistentCommandWithNoValidation()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("nonexistentcommand12345").WithNoValidation().CaptureAsync(),
      "should have thrown for non-existent command"
    );
  }

  public static async Task TestCommandWithNonZeroExitCodeAndNoValidation()
  {
    string[] lsArgs = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs).WithNoValidation().CaptureAsync();

    AssertTrue(
      string.IsNullOrEmpty(output.Stdout),
      "should return empty stdout for command with non-zero exit code and no validation"
    );
  }

  public static async Task TestExecuteAsyncThrowsOnNonZeroExit()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestGetLinesAsyncWithNoValidation()
  {
    string[] lsArgs2 = ["/nonexistent/path/12345"];

    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs2).WithNoValidation().CaptureAsync();
    string[] lines = output.GetStdoutLines(); // Use GetStdoutLines() to only get stdout

    AssertTrue(lines.Length == 0, "should return empty array for stdout lines when command fails");
  }

  public static async Task TestSpecialCharactersInArguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello \"World\" with 'quotes' and $pecial chars!").CaptureAsync();

    AssertTrue(
      !string.IsNullOrEmpty(output.Stdout),
      "should not return empty string for command with special characters"
    );
  }

  public static async Task TestEmptyCommandReturnsEmptyString()
  {
    CommandOutput output = await Shell.Builder("").CaptureAsync();
    AssertTrue(
      string.IsNullOrEmpty(output.Stdout),
      "should return empty string for empty command"
    );
  }

  public static async Task TestWhitespaceCommandReturnsEmptyString()
  {
    CommandOutput output = await Shell.Builder("   ").CaptureAsync();
    AssertTrue(
      string.IsNullOrEmpty(output.Stdout),
      "should return empty string for whitespace command"
    );
  }

  public static async Task TestDefaultGetStringThrowsOnError()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestDefaultGetLinesThrowsOnError()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestExecuteAsyncWithNoValidation()
  {
    string[] lsArgs3 = ["/nonexistent/path/12345"];
    CommandOutput output = await Shell.Builder("ls").WithArguments(lsArgs3).WithNoValidation().CaptureAsync();
    AssertTrue(
      !output.Success,
      "command should fail but not throw with no validation"
    );
  }
}