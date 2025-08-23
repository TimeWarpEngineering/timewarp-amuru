#!/usr/bin/dotnet run

await RunTests<CSharpScriptArgumentsTests>();

internal sealed class CSharpScriptArgumentsTests
{
  // Shared test script path
  static string? testScript;

  static async Task<string> GetOrCreateTestScript()
  {
    if (testScript == null)
    {
      // Create a test script that echoes its arguments
      testScript = Path.GetTempFileName() + ".cs";
      string scriptContent = @"#!/usr/bin/dotnet run
Console.WriteLine($""Args: {string.Join("", "", args)}"");
";
      await File.WriteAllTextAsync(testScript, scriptContent);

      // Make it executable on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", testScript).ExecuteAsync();
      }
    }
    
    return testScript;
  }

  public static async Task TestArgumentsWithDashPassedToScript()
  {
    string script = await GetOrCreateTestScript();
    string result = await Shell.Builder(script).WithArguments("-v", "pattern").GetStringAsync();
    
    AssertTrue(
      result.Contains("Args: -v, pattern", StringComparison.Ordinal),
      $"Arguments with dash should be passed to script - Expected 'Args: -v, pattern', got '{result.Trim()}'"
    );
  }

  public static async Task TestMultipleDashedArguments()
  {
    string script = await GetOrCreateTestScript();
    string result = await Shell.Builder(script).WithArguments("--verbose", "-c", "config.json", "--help").GetStringAsync();
    
    AssertTrue(
      result.Contains("Args: --verbose, -c, config.json, --help", StringComparison.Ordinal),
      $"Multiple dashed arguments should be passed correctly - got '{result.Trim()}'"
    );
  }

  public static async Task TestMixedArguments()
  {
    string script = await GetOrCreateTestScript();
    string result = await Shell.Builder(script).WithArguments("command", "-f", "file.txt", "--", "extra").GetStringAsync();
    
    AssertTrue(
      result.Contains("Args: command, -f, file.txt, --, extra", StringComparison.Ordinal),
      $"Mixed arguments should be passed correctly - got '{result.Trim()}'"
    );
  }

  public static async Task TestNoArguments()
  {
    string script = await GetOrCreateTestScript();
    string result = await Shell.Builder(script).GetStringAsync();
    
    AssertTrue(
      result.Trim() == "Args:",
      $"No arguments should be handled correctly - got '{result.Trim()}'"
    );
  }

  public static async Task TestRegularExecutableUnaffected()
  {
    string result = await Shell.Builder("echo").WithArguments("-n", "test").GetStringAsync();
    
    AssertTrue(
      result.Trim() == "test",
      $"Non-.cs executables should be unaffected - got '{result}'"
    );
  }

  // Clean up method called after tests complete
  public static void CleanUp()
  {
    try
    {
      if (testScript != null && File.Exists(testScript))
      {
        File.Delete(testScript);
      }
    }
    catch
    {
      // Ignore cleanup errors
    }
  }
}