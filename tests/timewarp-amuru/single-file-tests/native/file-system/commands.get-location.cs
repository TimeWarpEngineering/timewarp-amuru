#!/usr/bin/dotnet --

#region Purpose
// Tests for Commands.GetLocation - validates current directory retrieval via Commands API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Commands (the static class providing command-style file operations)
// Action: GetLocation (the method to get current working directory)
// Tests verify current directory is returned correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class GetLocation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetLocation_Given_>();

    public static async Task Default_Should_ReturnCurrentDirectory()
    {
      CommandOutput result = Commands.GetLocation();

      result.Success.ShouldBeTrue();
      result.Stdout.ShouldNotBeNullOrEmpty();
      result.ExitCode.ShouldBe(0);

      await Task.CompletedTask;
    }
  }
}
