#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder interface options - validates cycle, mouse, prompt, keybind settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithInterface (interface-related methods: WithCycle, WithNoMouse, WithBind, etc.)
// Tests verify interface flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithInterface_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithInterface_Given_>();

    public static async Task CycleMouseScrollPrompt_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithCycle()
        .WithNoMouse()
        .WithScrollOff(3)
        .WithPrompt("Select: ")
        .FromInput("choice1", "choice2", "choice3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --cycle --no-mouse --scroll-off=3 \"--prompt=Select: \"",
        $"Expected interface options, got '{command}'"
      );

      await Task.CompletedTask;
    }

    public static async Task BindAndJumpLabels_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithBind("ctrl-a:select-all")
        .WithJumpLabels("abcdefghij")
        .WithFilepathWord()
        .FromInput("path/to/file1", "path/to/file2", "different/path/file3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --bind=ctrl-a:select-all --jump-labels=abcdefghij --filepath-word",
        $"Expected bind and jump labels options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
