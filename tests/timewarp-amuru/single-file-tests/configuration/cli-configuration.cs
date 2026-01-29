#!/usr/bin/dotnet --

#region Purpose
// Tests for CliConfiguration - validates command path overrides and configuration management
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// CliConfiguration provides static methods to override command paths for testing/mocking
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CliConfiguration_
{
  [TestTag("Configuration")]
  public class Configuration_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Configuration_Given_>();

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
      List<string> files = [];

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
        CleanupTempFiles([.. tempFiles]);
      }
    }

    public static async Task ValidPath_Should_SetCustomPath()
    {
      await WithTempFiles(1, async files =>
      {
        string tempFile = files[0];

        CliConfiguration.SetCommandPath("fzf", tempFile);

        CliConfiguration.HasCustomPath("fzf").ShouldBeTrue();

        CommandResult command = Fzf.Builder()
          .FromInput("test1", "test2")
          .Build();

        command.ShouldNotBeNull();

        CliConfiguration.ClearCommandPath("fzf");
      });
    }

    public static async Task ClearCommandPath_Should_RemoveCustomPath()
    {
      await WithTempFiles(1, async files =>
      {
        string tempFile = files[0];

        CliConfiguration.SetCommandPath("git", tempFile);
        CliConfiguration.HasCustomPath("git").ShouldBeTrue();

        CliConfiguration.ClearCommandPath("git");

        CliConfiguration.HasCustomPath("git").ShouldBeFalse();
      });
    }

    public static async Task Reset_Should_ClearAllPaths()
    {
      await WithTempFiles(3, async files =>
      {
        CliConfiguration.SetCommandPath("fzf", files[0]);
        CliConfiguration.SetCommandPath("git", files[1]);
        CliConfiguration.SetCommandPath("gh", files[2]);

        CliConfiguration.AllCommandPaths.Count.ShouldBeGreaterThanOrEqualTo(3);

        CliConfiguration.Reset();

        CliConfiguration.AllCommandPaths.Count.ShouldBe(0);
      });
    }

    public static async Task AllCommandPaths_Should_ReturnDictionary()
    {
      CliConfiguration.Reset();

      await WithTempFiles(2, async files =>
      {
        CliConfiguration.SetCommandPath("cmd1", files[0]);
        CliConfiguration.SetCommandPath("cmd2", files[1]);

        IReadOnlyDictionary<string, string> paths = CliConfiguration.AllCommandPaths;

        paths.Count.ShouldBe(2);
        paths.ShouldContainKey("cmd1");
        paths["cmd1"].ShouldBe(files[0]);
        paths.ShouldContainKey("cmd2");
        paths["cmd2"].ShouldBe(files[1]);

        CliConfiguration.Reset();
      });
    }

    public static async Task MockPath_Should_BeUsedByShellBuilder()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), $"timewarp-cli-test-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);
      string mockEcho = Path.Combine(tempDir, "echo");

      try
      {
        await File.WriteAllTextAsync(mockEcho, "#!/bin/bash\necho \"MOCK OUTPUT\"");

        if (!OperatingSystem.IsWindows())
        {
          await Shell.Builder("chmod").WithArguments("+x", mockEcho).RunAsync();
        }

        CliConfiguration.SetCommandPath("echo", mockEcho);

        CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();

        output.Stdout.Trim().ShouldBe("MOCK OUTPUT");
      }
      finally
      {
        CliConfiguration.ClearCommandPath("echo");
        if (Directory.Exists(tempDir))
        {
          Directory.Delete(tempDir, true);
        }
      }
    }

    public static async Task ConcurrentAccess_Should_NotThrow()
    {
      CliConfiguration.Reset();

      await WithTempFiles(10, async files =>
      {
        List<Task> tasks = [];

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

        // If we got here, no exceptions were thrown
        true.ShouldBeTrue();

        CliConfiguration.Reset();
      });
    }

    public static async Task NullArguments_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath(null!, "/path"))
        .Message.ShouldContain("command");

      Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath("cmd", null!))
        .Message.ShouldContain("path");

      await Task.CompletedTask;
    }
  }
}
