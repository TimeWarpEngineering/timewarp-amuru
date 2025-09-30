#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Native.Aliases.Bash;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

await TestRunner.RunTests<GetLocationTests>();

internal sealed class GetLocationTests
{
  public static async Task TestGetLocation()
  {
    CommandOutput result = Commands.GetLocation();

    AssertTrue(
      result.Success,
      "GetLocation should succeed"
    );

    AssertTrue(
      !string.IsNullOrEmpty(result.Stdout),
      "GetLocation should return a non-empty path"
    );

    AssertTrue(
      result.ExitCode == 0,
      "Exit code should be 0 for success"
    );

    await Task.CompletedTask;
  }

  public static async Task TestSetLocation()
  {
    string tempPath = Path.GetTempPath();
    CommandOutput result = Commands.SetLocation(tempPath);

    AssertTrue(
      result.Success,
      "SetLocation should succeed for valid directory"
    );

    // Verify location was changed
    CommandOutput pwdResult = Commands.GetLocation();
    AssertTrue(
      pwdResult.Stdout.TrimEnd(Path.DirectorySeparatorChar) == tempPath.TrimEnd(Path.DirectorySeparatorChar),
      $"Location should be changed to temp path. Expected: '{tempPath}', Got: '{pwdResult.Stdout}'"
    );

    await Task.CompletedTask;
  }
}