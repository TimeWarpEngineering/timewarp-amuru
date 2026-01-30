#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder layout options - validates height, layout, border, scrollbar settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithLayout (layout-related methods: WithHeightPercent, WithLayout, WithBorder, etc.)
// Tests verify layout flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithLayout_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithLayout_Given_>();

    public static async Task HeightLayoutBorder_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithHeightPercent(50)
        .WithLayout("reverse")
        .WithBorder("rounded")
        .FromInput("option1", "option2", "option3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --height=50% --layout=reverse --border=rounded",
        $"Expected 'fzf --height=50% --layout=reverse --border=rounded', got '{command}'"
      );

      await Task.CompletedTask;
    }

    public static async Task AdvancedOptions_Should_AddAllFlags()
    {
      string command = Fzf.Builder()
        .WithLayout("reverse-list")
        .WithBorderLabel("Selection")
        .WithBorderLabelPos("10")
        .WithMargin("1,2,3,4")
        .WithPadding("1")
        .WithInfo("inline")
        .WithSeparator("---")
        .FromInput("advanced1", "advanced2", "advanced3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --layout=reverse-list --border-label=Selection --border-label-pos=10 --margin=1,2,3,4 --padding=1 --info=inline --separator=---",
        $"Expected advanced layout options, got '{command}'"
      );

      await Task.CompletedTask;
    }

    public static async Task ScrollbarAndEllipsis_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithScrollbar("█░")
        .WithEllipsis("...")
        .WithHeaderLines(2)
        .WithHeaderFirst()
        .FromInput("Header Line 1", "Header Line 2", "Content 1", "Content 2", "Content 3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --scrollbar=█░ --ellipsis=... --header-lines=2 --header-first",
        $"Expected scrollbar and ellipsis options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
