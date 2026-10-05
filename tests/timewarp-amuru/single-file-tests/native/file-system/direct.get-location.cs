#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.GetLocation and its Pwd alias.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Direct
// Action: GetLocation
// Compares against Environment.CurrentDirectory and does not change it.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class GetLocation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetLocation_Given_>();

    public static async Task Default_Should_MatchEnvironmentCurrentDirectory()
    {
      string currentDirectory = Environment.CurrentDirectory;

      Direct.GetLocation().ShouldBe(currentDirectory);
      Direct.Pwd().ShouldBe(currentDirectory);

      await Task.CompletedTask;
    }
  }
}
