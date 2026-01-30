#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder field processing options - validates nth, with-nth, delimiter settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithField (field-related methods: WithNth, WithWithNth, WithDelimiter)
// Tests verify field processing flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithField_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithField_Given_>();

    public static async Task NthWithNthDelimiter_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithNth("1,2")
        .WithWithNth("2..")
        .WithDelimiter(":")
        .FromInput("field1:field2:field3", "data1:data2:data3", "info1:info2:info3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --nth=1,2 --with-nth=2.. --delimiter=:",
        $"Expected field processing options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
