#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder with comprehensive options - validates all major options combined
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: Build (the build method with comprehensive configuration)
// Tests verify all options can be combined correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class BuildComprehensive_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<BuildComprehensive_Given_>();

    public static async Task AllOptions_Should_ProduceCompleteCommand()
    {
      string command = Fzf.Builder()
        .WithMulti(5)
        .WithPreview("echo 'Preview: {}'")
        .WithPreviewWindow("right:50%")
        .WithHeightPercent(60)
        .WithBorder("sharp")
        .WithPrompt("Choose: ")
        .WithMarker("*")
        .WithPointer("=>")
        .WithHeader("Select multiple items")
        .FromInput("item1", "item2", "item3", "item4", "item5", "item6")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --multi=5 \"--preview=echo 'Preview: {}'\" --preview-window=right:50% --height=60% --border=sharp \"--prompt=Choose: \" --marker=* --pointer==> \"--header=Select multiple items\"",
        $"Expected comprehensive fzf command, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
