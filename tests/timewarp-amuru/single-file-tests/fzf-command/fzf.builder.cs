#!/usr/bin/dotnet --

#region Purpose
// Tests for Fzf.Builder() - validates basic FzfBuilder creation
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Fzf (the static class providing builder factory)
// Action: Builder (the factory method being tested)
// Tests verify builder instance creation
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Fzf_
{
  [TestTag("FzfCommand")]
  public class Builder_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Builder_Given_>();

    public static async Task Default_Should_CreateBuilderSuccessfully()
    {
      FzfBuilder fzfBuilder = Fzf.Builder();

      fzfBuilder.ShouldNotBeNull("Fzf.Builder() should create builder successfully");

      await Task.CompletedTask;
    }
  }
}
