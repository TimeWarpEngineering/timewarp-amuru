#!/usr/bin/dotnet run

await RunTests<ErrorHandlingTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class ErrorHandlingTests
{

  public static async Task TestNonExistentCommandWithNoValidation()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("nonexistentcommand12345").WithNoValidation().GetStringAsync(),
      "should have thrown for non-existent command"
    );
  }

  public static async Task TestCommandWithNonZeroExitCodeAndNoValidation()
  {
    string[] lsArgs = ["/nonexistent/path/12345"];

    string lines = await Shell.Builder("ls").WithArguments(lsArgs).WithNoValidation().GetStringAsync();

    AssertTrue(
      string.IsNullOrEmpty(lines),
      "should return empty string for command with non-zero exit code and no validation"
    );
  }

  public static async Task TestExecuteAsyncThrowsOnNonZeroExit()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").ExecuteAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestGetLinesAsyncWithNoValidation()
  {
    string[] lsArgs2 = ["/nonexistent/path/12345"];

    string[] lines = await Shell.Builder("ls").WithArguments(lsArgs2).WithNoValidation().GetLinesAsync();

    AssertTrue(lines.Length == 0, "should return empty array for non-existent path with no validation");
  }

  public static async Task TestSpecialCharactersInArguments()
  {
    string result = await Shell.Builder("echo").WithArguments("Hello \"World\" with 'quotes' and $pecial chars!").GetStringAsync();

    AssertTrue(
      !string.IsNullOrEmpty(result),
      "should not return empty string for command with special characters"
    );
  }

  public static async Task TestEmptyCommandReturnsEmptyString()
  {
    string result = await Shell.Builder("").GetStringAsync();
    AssertTrue(
      string.IsNullOrEmpty(result),
      "should return empty string for empty command"
    );
  }

  public static async Task TestWhitespaceCommandReturnsEmptyString()
  {
    string result = await Shell.Builder("   ").GetStringAsync();
    AssertTrue(
      string.IsNullOrEmpty(result),
      "should return empty string for whitespace command"
    );
  }

  public static async Task TestDefaultGetStringThrowsOnError()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").GetStringAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestDefaultGetLinesThrowsOnError()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").GetLinesAsync(),
      "should have thrown for command with non-zero exit code"
    );
  }

  public static async Task TestExecuteAsyncWithNoValidation()
  {
    string[] lsArgs3 = ["/nonexistent/path/12345"];
    await Shell.Builder("ls").WithArguments(lsArgs3).WithNoValidation().ExecuteAsync();
    AssertTrue(
      true,
      "should not throw for command with no validation"
    );
  }
}