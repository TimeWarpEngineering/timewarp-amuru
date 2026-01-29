#!/usr/bin/dotnet --

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using Shouldly;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandResultOutputFormatsTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandResultOutputFormatsTests>();

  public static async Task Should_get_string_async_returns_raw_output()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").CaptureAsync();

    output.Stdout.ShouldContain("line1");
    output.Stdout.ShouldContain("line2");
    output.Stdout.ShouldContain("line3");
    await Task.CompletedTask;
  }

  public static async Task Should_get_lines_async_split_lines_correctly()
  {
    // Use printf to ensure consistent cross-platform newlines
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\nline2\nline3").CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
    await Task.CompletedTask;
  }

  public static async Task Should_get_lines_async_remove_empty_lines()
  {
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\n\nline2\n\n").CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(2);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    await Task.CompletedTask;
  }

  public static async Task Should_handle_empty_output()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("").CaptureAsync();
    string[] lines = output.GetLines();

    output.Stdout.Length.ShouldBeLessThanOrEqualTo(2);
    lines.Length.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_execute_real_world_ls_command()
  {
    CommandOutput output = await Shell.Builder("ls").WithArguments("-1").CaptureAsync();
    string[] files = output.GetLines();

    files.Length.ShouldBeGreaterThan(0);
    await Task.CompletedTask;
  }
}
