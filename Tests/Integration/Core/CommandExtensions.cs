#!/usr/bin/dotnet run

await RunTests<BasicCommandTests>();

internal sealed class BasicCommandTests
{
  public static async Task TestSimpleEchoCommand()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();
    
    AssertTrue(
      output.Stdout.Trim() == "Hello World",
      "Echo command should return 'Hello World'"
    );
  }

  public static async Task TestCommandWithMultipleArguments()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("arg1", "arg2", "arg3").CaptureAsync();
    
    AssertTrue(
      output.Stdout.Trim() == "arg1 arg2 arg3",
      $"Multiple arguments should work correctly, got '{output.Stdout.Trim()}'"
    );
  }

  public static async Task TestExecuteAsyncDoesNotThrow()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();
    
    // Test passes if no exception is thrown
    AssertTrue(output.Success, "Command should execute successfully");
  }

  public static async Task TestDateCommand()
  {
    CommandOutput output = await Shell.Builder("date").CaptureAsync();
    
    AssertTrue(
      !string.IsNullOrEmpty(output.Stdout.Trim()),
      "Date command should return non-empty result"
    );
  }
}