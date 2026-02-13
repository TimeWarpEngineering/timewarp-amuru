#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder search options - validates exact, case, scheme, sorting settings
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: FzfBuilder (the builder class)
// Action: WithSearch (search-related methods: WithExact, WithCaseInsensitive, WithNoSort, etc.)
// Tests verify search flags are added correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class WithSearch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithSearch_Given_>();

    public static async Task ExactCaseScheme_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithExact()
        .WithCaseInsensitive()
        .WithScheme("path")
        .FromInput("path/to/file1", "path/to/file2", "another/path/file3")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --exact -i --scheme=path",
        $"Expected 'fzf --exact -i --scheme=path', got '{command}'"
      );

      await Task.CompletedTask;
    }

    public static async Task SortingAndTracking_Should_AddFlags()
    {
      string command = Fzf.Builder()
        .WithNoSort()
        .WithTac()
        .WithTrack()
        .WithTiebreak("length,begin")
        .FromInput("short", "medium length", "very long text item")
        .Build()
        .ToCommandString();

      command.ShouldBe(
        "fzf --no-sort --tac --track --tiebreak=length,begin",
        $"Expected sorting and tracking options, got '{command}'"
      );

      await Task.CompletedTask;
    }
  }
}
