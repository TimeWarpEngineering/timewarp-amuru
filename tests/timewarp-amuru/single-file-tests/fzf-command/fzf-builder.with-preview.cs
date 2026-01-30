#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder.WithPreview() - validates preview options
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithPreview (the preview-related methods)
// Tests verify preview flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithPreview_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithPreview_Given_>();

    public static async Task Command_Should_AddPreviewFlag()
    {
      string command = Fzf.Builder()
        .WithPreview("echo {}")
        .FromInput("file1.txt", "file2.txt", "file3.txt")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf \"--preview=echo {}\"", $"Expected 'fzf \"--preview=echo {{}}\"', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task LabelOptions_Should_AddAllFlags()
    {
      string command = Fzf.Builder()
        .WithPreview("cat {}")
        .WithPreviewLabel("File Contents")
        .WithPreviewLabelPos(5)
        .WithPreviewWindow("right:60%:border")
        .FromInput("file1.txt", "file2.txt", "file3.txt")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf \"--preview=cat {}\" \"--preview-label=File Contents\" --preview-label-pos=5 --preview-window=right:60%:border",
        $"Expected preview label options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
