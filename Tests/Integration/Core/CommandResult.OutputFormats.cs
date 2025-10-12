#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<OutputFormatsTests>();

internal sealed class OutputFormatsTests
{
  public static async Task TestGetStringAsyncReturnsRawOutput()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").CaptureAsync();

    output.Stdout.ShouldContain("line1");
    output.Stdout.ShouldContain("line2");
    output.Stdout.ShouldContain("line3");
  }

  public static async Task TestGetLinesAsyncSplitsLinesCorrectly()
  {
    // Use printf to ensure consistent cross-platform newlines
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\nline2\nline3").CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
  }

  public static async Task TestGetLinesAsyncRemovesEmptyLines()
  {
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\n\nline2\n\n").CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(2);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
  }

  public static async Task TestEmptyOutputHandling()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("").CaptureAsync();
    string[] lines = output.GetLines();

    output.Stdout.Length.ShouldBeLessThanOrEqualTo(2);
    lines.Length.ShouldBe(0);
  }

  public static async Task TestRealWorldLsCommand()
  {
    CommandOutput output = await Shell.Builder("ls").WithArguments("-1").CaptureAsync();
    string[] files = output.GetLines();

    files.Length.ShouldBeGreaterThan(0);
  }
}