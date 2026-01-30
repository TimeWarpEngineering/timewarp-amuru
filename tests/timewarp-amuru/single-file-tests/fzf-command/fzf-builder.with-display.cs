#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder display options - validates ansi, color, tabstop settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithDisplay (display-related methods: WithAnsi, WithColor, WithTabstop, etc.)
// Tests verify display flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithDisplay_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithDisplay_Given_>();

    public static async Task AnsiColorTabstop_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithAnsi()
        .WithColor("dark")
        .WithTabstop(4)
        .WithNoBold()
        .FromInput("colored text", "bold text", "normal text")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --ansi --color=dark --tabstop=4 --no-bold",
        $"Expected display options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
