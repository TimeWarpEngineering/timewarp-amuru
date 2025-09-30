#!/usr/bin/dotnet --

await RunTests<ConfigurationTests>();

internal sealed class ConfigurationTests
{
  // Helper to create executable temp files
  private static async Task<string> CreateExecutableTempFile()
  {
    string tempFile = Path.GetTempFileName();

    if (!OperatingSystem.IsWindows())
    {
      await Shell.Builder("chmod").WithArguments("+x", tempFile).RunAsync();
    }

    return tempFile;
  }

  // Helper to create multiple executable temp files
  private static async Task<List<string>> CreateExecutableTempFiles(int count)
  {
    var files = new List<string>();

    for (int i = 0; i < count; i++)
    {
      files.Add(await CreateExecutableTempFile());
    }

    return files;
  }

  // Helper to cleanup temp files
  private static void CleanupTempFiles(params string[] files)
  {
    foreach (string file in files)
    {
      if (File.Exists(file))
      {
        File.Delete(file);
      }
    }
  }

  // Helper to run a test with temp files
  private static async Task WithTempFiles(int count, Func<List<string>, Task> testAction)
  {
    List<string> tempFiles = await CreateExecutableTempFiles(count);

    try
    {
      await testAction(tempFiles);
    }
    finally
    {
      CleanupTempFiles(tempFiles.ToArray());
    }
  }
  public static async Task TestSetCommandPath()
  {
    await WithTempFiles(1, async files =>
    {
      string tempFile = files[0];

      // Set a custom path
      CliConfiguration.SetCommandPath("fzf", tempFile);

      CliConfiguration.HasCustomPath("fzf").ShouldBeTrue("Configuration should have custom path for fzf");

      // Create and build a command - it should use the custom path
      CommandResult command = Fzf.Builder()
        .FromInput("test1", "test2")
        .Build();

      command.ShouldNotBeNull("Fzf command with custom path should build correctly");

      // Cleanup config
      CliConfiguration.ClearCommandPath("fzf");
    });
  }
  
  public static async Task TestClearCommandPath()
  {
    await WithTempFiles(1, async files =>
    {
      string tempFile = files[0];

      CliConfiguration.SetCommandPath("git", tempFile);

      CliConfiguration.HasCustomPath("git").ShouldBeTrue("Configuration should have custom path for git");

      // Clear the path
      CliConfiguration.ClearCommandPath("git");

      CliConfiguration.HasCustomPath("git").ShouldBeFalse("Configuration should not have custom path after clearing");
    });
  }
  
  public static async Task TestReset()
  {
    await WithTempFiles(3, async files =>
    {
      // Setup multiple custom paths
      CliConfiguration.SetCommandPath("fzf", files[0]);
      CliConfiguration.SetCommandPath("git", files[1]);
      CliConfiguration.SetCommandPath("gh", files[2]);

      CliConfiguration.AllCommandPaths.Count.ShouldBeGreaterThanOrEqualTo(3, "Configuration should have at least 3 custom paths");

      // Reset all
      CliConfiguration.Reset();

      CliConfiguration.AllCommandPaths.Count.ShouldBe(0, "Configuration should have no custom paths after reset");
    });
  }
  
  public static async Task TestGetAllCommandPaths()
  {
    // Clear any existing configuration
    CliConfiguration.Reset();

    await WithTempFiles(2, async files =>
    {
      // Setup
      CliConfiguration.SetCommandPath("cmd1", files[0]);
      CliConfiguration.SetCommandPath("cmd2", files[1]);

      IReadOnlyDictionary<string, string> paths = CliConfiguration.AllCommandPaths;

      paths.Count.ShouldBe(2, "Should have exactly 2 custom paths");
      paths.ShouldContainKey("cmd1");
      paths["cmd1"].ShouldBe(files[0], "Should have correct path for cmd1");
      paths.ShouldContainKey("cmd2");
      paths["cmd2"].ShouldBe(files[1], "Should have correct path for cmd2");

      // Cleanup config
      CliConfiguration.Reset();
    });
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

      result.Trim().ShouldBe("MOCK OUTPUT");
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

    await WithTempFiles(10, async files =>
    {
      // Run multiple threads setting and clearing paths
      List<Task> tasks = new();

      for (int i = 0; i < 10; i++)
      {
        int index = i;
        string file = files[index];
        tasks.Add(Task.Run(() =>
        {
          for (int j = 0; j < 100; j++)
          {
            CliConfiguration.SetCommandPath($"cmd{index}", file);
            CliConfiguration.HasCustomPath($"cmd{index}");
            CliConfiguration.ClearCommandPath($"cmd{index}");
          }
        }));
      }

      await Task.WhenAll(tasks);

      // Should complete without exceptions - if we got here, no exceptions were thrown
      true.ShouldBeTrue("Thread safety test completed without exceptions");

      // Cleanup config
      CliConfiguration.Reset();
    });
  }
  
  public static async Task TestNullArgumentHandling()
  {
    // Test null command
    Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath(null!, "/path"))
      .Message.ShouldContain("command");

    // Test null path
    Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath("cmd", null!))
      .Message.ShouldContain("path");

    await Task.CompletedTask;
  }
}