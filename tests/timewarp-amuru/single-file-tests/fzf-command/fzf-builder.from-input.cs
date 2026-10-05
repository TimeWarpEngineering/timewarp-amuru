#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for FzfBuilder.FromInput() - validates various input source methods
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: FromInput (the input source methods: FromInput, FromFiles, FromCommand)
// Tests verify input sources are configured correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class FromInput_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FromInput_Given_>();

    public static async Task Items_Should_ProduceBasicCommand()
    {
      string command = Fzf.Builder()
        .FromInput("apple", "banana", "cherry")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf ", $"Expected 'fzf ', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task Files_Should_ProduceBasicCommand()
    {
      string command = Fzf.Builder()
        .FromFiles("*.cs")
        .WithPreview("head -20 {}")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf \"--preview=head -20 {}\"", $"Expected 'fzf \"--preview=head -20 {{}}\"', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task Command_Should_ProduceBasicCommand()
    {
      string command = Fzf.Builder()
        .FromCommand("echo hello world")
        .WithPrompt("Select output: ")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf \"--prompt=Select output: \"", $"Expected 'fzf \"--prompt=Select output: \"', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task Collection_Should_ProduceBasicCommand()
    {
      var items = new List<string> { "collection1", "collection2", "collection3" };
      string command = Fzf.Builder()
        .FromInput(items)
        .WithPrompt("From collection: ")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf \"--prompt=From collection: \"", $"Expected 'fzf \"--prompt=From collection: \"', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvironment_Should_NotAppearInCommand()
    {
      // Note: Working directory and environment variables don't appear in ToCommandString()
      string command = Fzf.Builder()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("FZF_DEFAULT_OPTS", "--height 40%")
        .FromInput("env1", "env2", "env3")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf ", $"Expected 'fzf ', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task EchoFlagItems_Should_StayOnStdin()
    {
      string mockFzfPath = await CreateStdinCatMock();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        string result = await Fzf.Builder()
          .FromInput("-n", "-e", "-E", "keep")
          .SelectAsync();

        result.ShouldBe("-n\n-e\n-E\nkeep");
      }
      finally
      {
        CliConfiguration.Reset();
        if (File.Exists(mockFzfPath))
        {
          File.Delete(mockFzfPath);
        }
      }
    }

    public static async Task QuotedCommand_Should_KeepSpacesInOneArgument()
    {
      string mockFzfPath = await CreateStdinCatMock();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        string result = await Fzf.Builder()
          .FromCommand("printf %s \"--format=%h %s\"")
          .SelectAsync();

        result.ShouldBe("--format=%h %s");
      }
      finally
      {
        CliConfiguration.Reset();
        if (File.Exists(mockFzfPath))
        {
          File.Delete(mockFzfPath);
        }
      }
    }

    public static async Task Files_Should_ListMatchingNamesOnly()
    {
      string root = Path.Combine(Path.GetTempPath(), "fzf-from-files-" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(Path.Combine(root, "nested"));
      await File.WriteAllTextAsync(Path.Combine(root, "one.cs"), "1");
      await File.WriteAllTextAsync(Path.Combine(root, "nested", "two.cs"), "2");
      await File.WriteAllTextAsync(Path.Combine(root, "skip.txt"), "s");
      string mockFzfPath = await CreateStdinCatMock();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        string result = await Fzf.Builder()
          .WithWorkingDirectory(root)
          .FromFiles("*.cs")
          .SelectAsync();

        result.ShouldContain("./one.cs");
        result.ShouldContain("./nested/two.cs");
        result.ShouldNotContain("skip.txt");
      }
      finally
      {
        CliConfiguration.Reset();
        if (File.Exists(mockFzfPath))
        {
          File.Delete(mockFzfPath);
        }

        if (Directory.Exists(root))
        {
          Directory.Delete(root, recursive: true);
        }
      }
    }

    public static async Task DirectoryGlob_Should_Throw()
    {
      Should.Throw<ArgumentException>(() => Fzf.Builder().FromFiles("src/*.cs").Build());
      await Task.CompletedTask;
    }

    public static async Task CommandOptions_Should_ReachTheFzfStage()
    {
      string root = Directory.CreateTempSubdirectory("fzf-from-command-").FullName;
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath += ".sh";
      const string mockScript = "#!/bin/sh\nprintf '%s\\n' \"$PWD\"\nprintf '%s\\n' \"$FZF_STAGE_MARKER\"\n";
      await File.WriteAllTextAsync(mockPath, mockScript);
      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        await Shell.Builder("chmod").WithArguments("+x", mockPath).RunAsync();
      }

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockPath);

        CommandOutput output = await Fzf.Builder()
          .WithWorkingDirectory(root)
          .WithEnvironmentVariable("FZF_STAGE_MARKER", "stage-ok")
          .FromCommand("printf %s hi")
          .CaptureAsync();

        output.Success.ShouldBeTrue(output.Stderr);
        string[] lines = output.GetLines();
        lines.ShouldContain(root);
        lines.ShouldContain("stage-ok");
      }
      finally
      {
        CliConfiguration.Reset();
        if (File.Exists(mockPath))
        {
          File.Delete(mockPath);
        }

        if (Directory.Exists(root))
        {
          Directory.Delete(root, recursive: true);
        }
      }
    }

    private static async Task<string> CreateStdinCatMock()
    {
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath += ".sh";

      const string mockScript = "#!/bin/sh\ncat\n";
      await File.WriteAllTextAsync(mockPath, mockScript);

      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        await Shell.Builder("chmod")
          .WithArguments("+x", mockPath)
          .RunAsync();
      }

      return mockPath;
    }
  }
}
