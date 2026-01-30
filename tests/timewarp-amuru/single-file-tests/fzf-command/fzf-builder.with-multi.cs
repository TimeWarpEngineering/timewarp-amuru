#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder.WithMulti() - validates multi-select option
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithMulti (the multi-select method)
// Tests verify multi-select flag is added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithMulti_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithMulti_Given_>();

    public static async Task Enabled_Should_AddMultiFlag()
    {
      string command = Fzf.Builder()
        .WithMulti()
        .FromInput("item1", "item2", "item3")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf --multi", $"Expected 'fzf --multi', got '{command}'");

      await Task.CompletedTask;
    }
  }
}
