#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

await TestRunner.RunTests<GetLocationTests>();

internal sealed class GetLocationTests
{
  public static async Task TestGetLocation()
  {
    CommandOutput result = Commands.GetLocation();

    result.Success.ShouldBeTrue();
    result.Stdout.ShouldNotBeNullOrEmpty();
    result.ExitCode.ShouldBe(0);

    await Task.CompletedTask;
  }

  public static async Task TestSetLocation()
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