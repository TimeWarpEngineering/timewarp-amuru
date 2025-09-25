#!/usr/bin/dotnet --

await RunTests<ConfigurationTests>();

internal sealed class ConfigurationTests
{
  public static async Task TestSetCommandPath()
  {
    // Setup - create a temporary mock executable
    string tempFile = Path.GetTempFileName();

    try
    {
      // Make it executable on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", tempFile).RunAsync();
      }

      // Set a custom path
      CliConfiguration.SetCommandPath("fzf", tempFile);

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
      CliConfiguration.ClearCommandPath("fzf");
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }

    await Task.CompletedTask;
  }
  
  public static async Task TestClearCommandPath()
  {
    // Setup - create a temporary mock executable
    string tempFile = Path.GetTempFileName();

    try
    {
      // Make it executable on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", tempFile).RunAsync();
      }

      CliConfiguration.SetCommandPath("git", tempFile);

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
    }
    finally
    {
      // Cleanup temp file
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }

    await Task.CompletedTask;
  }
  
  public static async Task TestReset()
  {
    // Create temp files for testing
    string tempFile1 = Path.GetTempFileName();
    string tempFile2 = Path.GetTempFileName();
    string tempFile3 = Path.GetTempFileName();

    try
    {
      // Make them executable on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", tempFile1, tempFile2, tempFile3).RunAsync();
      }

      // Setup multiple custom paths
      CliConfiguration.SetCommandPath("fzf", tempFile1);
      CliConfiguration.SetCommandPath("git", tempFile2);
      CliConfiguration.SetCommandPath("gh", tempFile3);

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
    }
    finally
    {
      // Cleanup temp files
      if (File.Exists(tempFile1)) File.Delete(tempFile1);
      if (File.Exists(tempFile2)) File.Delete(tempFile2);
      if (File.Exists(tempFile3)) File.Delete(tempFile3);
    }

    await Task.CompletedTask;
  }
  
  public static async Task TestGetAllCommandPaths()
  {
    // Clear any existing configuration
    CliConfiguration.Reset();

    // Create temp files
    string tempFile1 = Path.GetTempFileName();
    string tempFile2 = Path.GetTempFileName();

    try
    {
      // Make them executable on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", tempFile1, tempFile2).RunAsync();
      }

      // Setup
      CliConfiguration.SetCommandPath("cmd1", tempFile1);
      CliConfiguration.SetCommandPath("cmd2", tempFile2);

      IReadOnlyDictionary<string, string> paths = CliConfiguration.AllCommandPaths;

      AssertTrue(
        paths.Count == 2,
        "Should have exactly 2 custom paths"
      );

      AssertTrue(
        paths.ContainsKey("cmd1") && paths["cmd1"] == tempFile1,
        "Should have correct path for cmd1"
      );

      AssertTrue(
        paths.ContainsKey("cmd2") && paths["cmd2"] == tempFile2,
        "Should have correct path for cmd2"
      );
    }
    finally
    {
      // Cleanup
      CliConfiguration.Reset();
      if (File.Exists(tempFile1)) File.Delete(tempFile1);
      if (File.Exists(tempFile2)) File.Delete(tempFile2);
    }

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