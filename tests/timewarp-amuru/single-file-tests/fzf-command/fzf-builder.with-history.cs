#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder history options - validates history file and size settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithHistory (history-related methods: WithHistory, WithHistorySize)
// Tests verify history flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithHistory_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithHistory_Given_>();

    public static async Task FileAndSize_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithHistory("/tmp/fzf_history")
        .WithHistorySize(100)
        .FromInput("history1", "history2", "history3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --history=/tmp/fzf_history --history-size=100",
        $"Expected history options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
