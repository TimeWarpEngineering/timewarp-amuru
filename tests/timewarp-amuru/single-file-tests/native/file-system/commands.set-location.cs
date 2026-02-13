#!/usr/bin/dotnet --

#region Purpose
// Tests for Commands.SetLocation - validates directory change via Commands API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Commands (the static class providing command-style file operations)
// Action: SetLocation (the method to change current working directory)
// Tests verify directory change works correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class SetLocation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SetLocation_Given_>();

    public static async Task ValidPath_Should_ChangeDirectory()
    {
      string tempPath = Path.GetTempPath();
      CommandOutput result = Commands.SetLocation(tempPath);

      result.Success.ShouldBeTrue();

      // Verify location was changed
      CommandOutput pwdResult = Commands.GetLocation();
      pwdResult.Stdout.TrimEnd(Path.DirectorySeparatorChar).ShouldBe(tempPath.TrimEnd(Path.DirectorySeparatorChar));

      await Task.CompletedTask;
    }
  }
}
