#!/usr/bin/dotnet --

#region Purpose
// Tests for CliConfiguration.SetCommandPath() - validates setting custom command paths
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SetCommandPath allows overriding command executables for testing/mocking
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CliConfiguration_
{
  [TestTag("Configuration")]
  public class SetCommandPath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SetCommandPath_Given_>();

    public static async Task ValidPath_Should_SetCustomPath()
    {
      string tempFile = await CreateExecutableTempFile();

      try
      {
        CliConfiguration.SetCommandPath("fzf", tempFile);

        CliConfiguration.HasCustomPath("fzf").ShouldBeTrue();

        CommandResult command = Fzf.Builder()
          .FromInput("test1", "test2")
          .Build();

        command.ShouldNotBeNull();
      }
      finally
      {
        CliConfiguration.ClearCommandPath("fzf");
        File.Delete(tempFile);
      }
    }

    public static async Task MockScript_Should_BeUsedByShellBuilder()
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

    public static async Task NullCommand_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath(null!, "/path"))
        .Message.ShouldContain("command");

      await Task.CompletedTask;
    }

    public static async Task NullPath_Should_ThrowArgumentNullException()
    {
      Should.Throw<ArgumentNullException>(() => CliConfiguration.SetCommandPath("cmd", null!))
        .Message.ShouldContain("path");

      await Task.CompletedTask;
    }

    public static async Task ConcurrentAccess_Should_NotThrow()
    {
      CliConfiguration.Reset();
      List<string> tempFiles = [];

      try
      {
        for (int i = 0; i < 10; i++)
        {
          tempFiles.Add(await CreateExecutableTempFile());
        }

        List<Task> tasks = [];

        for (int i = 0; i < 10; i++)
        {
          int index = i;
          string file = tempFiles[index];
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

        true.ShouldBeTrue();
      }
      finally
      {
        CliConfiguration.Reset();
        foreach (string file in tempFiles)
        {
          if (File.Exists(file)) File.Delete(file);
        }
      }
    }
  }
}
