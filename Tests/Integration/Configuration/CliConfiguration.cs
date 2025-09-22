#!/usr/bin/dotnet --

await RunTests<ConfigurationTests>();

internal sealed class ConfigurationTests
{
  public static async Task TestSetCommandPath()
  {
    // Setup
    string? originalFzf = CliConfiguration.HasCustomPath("fzf") ? 
      CliConfiguration.AllCommandPaths["fzf"] : null;
    
    try
    {
      // Set a custom path
      CliConfiguration.SetCommandPath("fzf", "/custom/path/to/fzf");
      
      AssertTrue(
        CliConfiguration.HasCustomPath("fzf"),
        "Configuration should have custom path for fzf"
      );
      
      // Create and build a command - it should use the custom path
      CommandResult command = Fzf.Builder()
        .FromInput("test1", "test2")
        .Build();
      
      AssertTrue(
        command != null,
        "Fzf command with custom path should build correctly"
      );
    }
    finally
    {
      // Cleanup
      if (originalFzf != null)
      {
        CliConfiguration.SetCommandPath("fzf", originalFzf);
      }
      else
      {
        CliConfiguration.ClearCommandPath("fzf");
      }
    }
    
    await Task.CompletedTask;
  }
  
  public static async Task TestClearCommandPath()
  {
    // Setup
    CliConfiguration.SetCommandPath("git", "/mock/git");
    
    AssertTrue(
      CliConfiguration.HasCustomPath("git"),
      "Configuration should have custom path for git"
    );
    
    // Clear the path
    CliConfiguration.ClearCommandPath("git");
    
    AssertFalse(
      CliConfiguration.HasCustomPath("git"),
      "Configuration should not have custom path after clearing"
    );
    
    await Task.CompletedTask;
  }
  
  public static async Task TestReset()
  {
    // Setup multiple custom paths
    CliConfiguration.SetCommandPath("fzf", "/mock/fzf");
    CliConfiguration.SetCommandPath("git", "/mock/git");
    CliConfiguration.SetCommandPath("gh", "/mock/gh");
    
    AssertTrue(
      CliConfiguration.AllCommandPaths.Count >= 3,
      "Configuration should have at least 3 custom paths"
    );
    
    // Reset all
    CliConfiguration.Reset();
    
    AssertTrue(
      CliConfiguration.AllCommandPaths.Count == 0,
      "Configuration should have no custom paths after reset"
    );
    
    await Task.CompletedTask;
  }
  
  public static async Task TestGetAllCommandPaths()
  {
    // Clear any existing configuration
    CliConfiguration.Reset();
    
    // Setup
    CliConfiguration.SetCommandPath("cmd1", "/path/to/cmd1");
    CliConfiguration.SetCommandPath("cmd2", "/path/to/cmd2");
    
    IReadOnlyDictionary<string, string> paths = CliConfiguration.AllCommandPaths;
    
    AssertTrue(
      paths.Count == 2,
      "Should have exactly 2 custom paths"
    );
    
    AssertTrue(
      paths.ContainsKey("cmd1") && paths["cmd1"] == "/path/to/cmd1",
      "Should have correct path for cmd1"
    );
    
    AssertTrue(
      paths.ContainsKey("cmd2") && paths["cmd2"] == "/path/to/cmd2",
      "Should have correct path for cmd2"
    );
    
    // Cleanup
    CliConfiguration.Reset();
    
    await Task.CompletedTask;
  }
  
  public static async Task TestCommandExecutionWithMockPath()
  {
    // Create a temporary mock executable
    string tempDir = Path.Combine(Path.GetTempPath(), $"timewarp-cli-test-{Guid.NewGuid()}");
    Directory.CreateDirectory(tempDir);
    string mockEcho = Path.Combine(tempDir, "echo");
    
    try
    {
      // Create a simple mock echo script
      await File.WriteAllTextAsync(mockEcho, "#!/bin/bash\necho \"MOCK OUTPUT\"");
      
      // Make it executable (Unix-like systems)
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", mockEcho).RunAsync();
      }
      
      // Configure the mock path
      CliConfiguration.SetCommandPath("echo", mockEcho);
      
      // Test that Run uses the mock
      CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();
      string result = output.Stdout;
      
      AssertTrue(
        result.Trim() == "MOCK OUTPUT",
        $"Expected 'MOCK OUTPUT' but got '{result.Trim()}'"
      );
    }
    finally
    {
      // Cleanup
      CliConfiguration.ClearCommandPath("echo");
      if (Directory.Exists(tempDir))
      {
        Directory.Delete(tempDir, true);
      }
    }
  }
  
  public static async Task TestThreadSafety()
  {
    // Clear any existing configuration
    CliConfiguration.Reset();
    
    // Run multiple threads setting and clearing paths
    List<Task> tasks = new();
    
    for (int i = 0; i < 10; i++)
    {
      int index = i;
      tasks.Add(Task.Run(() =>
      {
        for (int j = 0; j < 100; j++)
        {
          CliConfiguration.SetCommandPath($"cmd{index}", $"/path/{index}/{j}");
          CliConfiguration.HasCustomPath($"cmd{index}");
          CliConfiguration.ClearCommandPath($"cmd{index}");
        }
      }));
    }
    
    await Task.WhenAll(tasks);
    
    // Should complete without exceptions
    AssertTrue(
      true,
      "Thread safety test completed without exceptions"
    );
    
    // Cleanup
    CliConfiguration.Reset();
  }
  
  public static async Task TestNullArgumentHandling()
  {
    bool exceptionThrown = false;
    
    try
    {
      CliConfiguration.SetCommandPath(null!, "/path");
    }
    catch (ArgumentNullException)
    {
      exceptionThrown = true;
    }
    
    AssertTrue(
      exceptionThrown,
      "SetCommandPath should throw ArgumentNullException for null command"
    );
    
    exceptionThrown = false;
    try
    {
      CliConfiguration.SetCommandPath("cmd", null!);
    }
    catch (ArgumentNullException)
    {
      exceptionThrown = true;
    }
    
    AssertTrue(
      exceptionThrown,
      "SetCommandPath should throw ArgumentNullException for null path"
    );
    
    await Task.CompletedTask;
  }
}