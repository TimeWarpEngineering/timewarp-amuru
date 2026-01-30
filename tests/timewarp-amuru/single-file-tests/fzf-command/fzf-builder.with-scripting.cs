#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder scripting options - validates query, filter, expect, listen, null-handling settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithScripting (scripting-related methods: WithQuery, WithFilter, WithExpect, WithListen, etc.)
// Tests verify scripting flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithScripting_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithScripting_Given_>();

    public static async Task QuerySelect1Exit0Filter_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithQuery("initial")
        .WithSelect1()
        .WithExit0()
        .WithFilter("test")
        .FromInput("initial test", "other item", "another option")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --query=initial --select-1 --exit-0 --filter=test",
        $"Expected scripting options, got '{command}'"
      );

      await Task.CompletedTask;
    }

    public static async Task FilterMode_Should_AddFlag()
    {
      // This tests that the command builds correctly even if fzf might not be installed
      string command = Fzf.Builder()
        .WithFilter("test")  // Use filter mode to avoid interactive requirement
        .FromInput("test1", "test2", "test3")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf --filter=test", $"Expected 'fzf --filter=test', got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task ListenAndSync_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithListen(8080)
        .WithSync()
        .FromInput("server1", "server2", "server3")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf --listen=8080 --sync", $"Expected listen and sync options, got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task NullHandling_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithRead0()
        .WithPrint0()
        .FromInput("null1", "null2", "null3")
        .Build()
        .ToCommandString();

      command.ShouldBe("fzf --read0 --print0", $"Expected null handling options, got '{command}'");

      await Task.CompletedTask;
    }

    public static async Task ExpectAndPrintQuery_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithExpect("ctrl-a,ctrl-b")
        .WithPrintQuery()
        .FromInput("expect1", "expect2", "expect3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --expect=ctrl-a,ctrl-b --print-query",
        $"Expected expect and print-query options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
