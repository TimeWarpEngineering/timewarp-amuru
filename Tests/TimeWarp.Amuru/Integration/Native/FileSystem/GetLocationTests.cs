#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;

namespace TimeWarp.Amuru.Tests;

[TestTag("Native")]
public class GetLocationTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<GetLocationTests>();

  public static async Task Should_get_location()
  {
    CommandOutput result = Commands.GetLocation();

    result.Success.ShouldBeTrue();
    result.Stdout.ShouldNotBeNullOrEmpty();
    result.ExitCode.ShouldBe(0);

    await Task.CompletedTask;
  }

  public static async Task Should_set_location()
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
