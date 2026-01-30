#!/usr/bin/dotnet --

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
  }
}
