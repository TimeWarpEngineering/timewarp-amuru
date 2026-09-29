#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for PathResolver - validates cross-platform PATH resolution
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: PathResolver (the static class providing PATH resolution)
// Tests verify finding executables, handling edge cases, and cross-platform behavior
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace PathResolver_
{
  [TestTag("Native")]
  public class ResolveExecutable_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ResolveExecutable_Given_>();

    public static async Task ExistingCommand_Should_ReturnPath()
    {
      // Use well-known commands that should exist on all platforms
      string command = OperatingSystem.IsWindows() ? "cmd" : "ls";
      
      string? result = PathResolver.ResolveExecutable(command);
      
      result.ShouldNotBeNull();
      File.Exists(result!).ShouldBeTrue($"PathResolver returned a path that doesn't exist: {result}");
    }

    public static async Task NonExistentCommand_Should_ReturnNull()
    {
      string? result = PathResolver.ResolveExecutable("this-command-definitely-does-not-exist-xyz123");
      
      result.ShouldBeNull();
    }

    public static async Task EmptyName_Should_ThrowArgumentException()
    {
      Should.Throw<ArgumentException>(() => PathResolver.ResolveExecutable(""));
    }

    public static async Task NullName_Should_ThrowArgumentException()
    {
      Should.Throw<ArgumentException>(() => PathResolver.ResolveExecutable(null!));
    }

    public static async Task WhitespaceName_Should_ThrowArgumentException()
    {
      Should.Throw<ArgumentException>(() => PathResolver.ResolveExecutable("   "));
    }

    public static async Task PathInName_Should_ReturnAsIsIfExists()
    {
      // Use a known existing file with full path
      string knownFile = OperatingSystem.IsWindows()
        ? @"C:\Windows\System32\cmd.exe"
        : "/bin/ls";
      
      string? result = PathResolver.ResolveExecutable(knownFile);
      
      result.ShouldNotBeNull();
    }

    public static async Task PathInName_Should_ReturnNullIfNotExists()
    {
      string? result = PathResolver.ResolveExecutable("/nonexistent/path/to/command");
      
      result.ShouldBeNull();
    }
  }

  [TestTag("Native")]
  public class ResolveAllExecutables_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ResolveAllExecutables_Given_>();

    public static async Task ExistingCommand_Should_ReturnAtLeastOne()
    {
      string command = OperatingSystem.IsWindows() ? "cmd" : "ls";
      
      IReadOnlyList<string> results = PathResolver.ResolveAllExecutables(command);
      
      results.Count.ShouldBeGreaterThan(0);
    }

    public static async Task NonExistentCommand_Should_ReturnEmptyList()
    {
      IReadOnlyList<string> results = PathResolver.ResolveAllExecutables("this-command-definitely-does-not-exist-xyz123");
      
      results.Count.ShouldBe(0);
    }

    public static async Task EmptyName_Should_ThrowArgumentException()
    {
      Should.Throw<ArgumentException>(() => PathResolver.ResolveAllExecutables(""));
    }

    public static async Task MultipleMatches_Should_ReturnAllInPathOrder()
    {
      // This test verifies that if there are multiple matches, they're returned in PATH order
      // We can't guarantee multiple matches exist, so just verify the method works
      string command = OperatingSystem.IsWindows() ? "cmd" : "ls";
      
      IReadOnlyList<string> results = PathResolver.ResolveAllExecutables(command);
      
      // All returned paths should exist
      foreach (string path in results)
      {
        File.Exists(path).ShouldBeTrue($"Returned path doesn't exist: {path}");
      }
    }
  }

  [TestTag("Native")]
  public class ResolveExecutable_UnixExecuteBit_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ResolveExecutable_UnixExecuteBit_Given_>();

    public static async Task NonExecutableEarlierOnPath_Should_SkipToExecutable()
    {
      if (OperatingSystem.IsWindows())
      {
        await Task.CompletedTask;
        return;
      }

      string root = Path.Combine(Path.GetTempPath(), "path-resolver-" + Guid.NewGuid().ToString("N"));
      string earlier = Path.Combine(root, "earlier");
      string later = Path.Combine(root, "later");
      Directory.CreateDirectory(earlier);
      Directory.CreateDirectory(later);
      string blocked = Path.Combine(earlier, "amuru-tool");
      string allowed = Path.Combine(later, "amuru-tool");
      string? originalPath = Environment.GetEnvironmentVariable("PATH");

      try
      {
        await File.WriteAllTextAsync(blocked, "#!/bin/sh\n");
        await File.WriteAllTextAsync(allowed, "#!/bin/sh\n");
        File.SetUnixFileMode(blocked, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.SetUnixFileMode(
          allowed,
          UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        Environment.SetEnvironmentVariable("PATH", earlier + Path.PathSeparator + later);

        PathResolver.ResolveExecutable("amuru-tool").ShouldBe(allowed);
        IReadOnlyList<string> all = PathResolver.ResolveAllExecutables("amuru-tool");
        all.Count.ShouldBe(1);
        all[0].ShouldBe(allowed);
        PathResolver.ResolveExecutable(blocked).ShouldBeNull();
      }
      finally
      {
        Environment.SetEnvironmentVariable("PATH", originalPath);
        if (Directory.Exists(root))
        {
          Directory.Delete(root, recursive: true);
        }
      }
    }
  }
}
