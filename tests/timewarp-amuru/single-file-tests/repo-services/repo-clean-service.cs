#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for RepoCleanService - validates repository cleaning operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Repo_Services
{
  [TestTag("Repo")]
  public class RepoCleanService_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RepoCleanService_Given_>();

    public static async Task Constructor_WithValidTerminal_ShouldSucceed()
    {
      using TestTerminal terminal = new();
      RepoCleanService service = new(terminal);

      service.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task CleanAsync_WhenNotInGitRepo_ShouldReturnEmptyResult()
    {
      using TestTerminal terminal = new();
      RepoCleanService service = new(terminal);

      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        CleanResult result = await service.CleanAsync();

        result.ObjDirectoriesDeleted.ShouldBe(0);
        result.BinDirectoriesDeleted.ShouldBe(0);
        result.RootBinFilesCleaned.ShouldBe(0);
        terminal.ErrorOutput.ShouldContain("Not in a git repository");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }
    }

    public static async Task CleanAsync_WhenInGitRepo_ShouldDeleteObjDirectories()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string testDir = Path.Combine(repoRoot, "test-obj-dir");
      string objDir = Path.Combine(testDir, "obj");
      Directory.CreateDirectory(objDir);

      try
      {
        using TestTerminal terminal = new();
        RepoCleanService service = new(terminal);

        CleanResult result = await service.CleanAsync();

        result.ObjDirectoriesDeleted.ShouldBeGreaterThanOrEqualTo(1);
        Directory.Exists(objDir).ShouldBeFalse();
      }
      finally
      {
        if (Directory.Exists(testDir))
        {
          Directory.Delete(testDir, recursive: true);
        }
      }
    }

    public static async Task CleanAsync_RootBin_ShouldSkipTrackedFilesReparsePointsAndPreservedNames()
    {
      string tempRoot = Path.Combine(AppContext.BaseDirectory, "temp-fixtures", Guid.NewGuid().ToString("N"));
      string sentinel = Path.Combine(AppContext.BaseDirectory, "temp-fixtures", $"sentinel-{Guid.NewGuid():N}");
      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.CreateDirectory(tempRoot);
        CommandOutput init = await Shell.Builder("git")
          .WithArguments("-C", tempRoot, "init")
          .WithNoValidation()
          .CaptureAsync();
        init.Success.ShouldBeTrue();

        string bin = Path.Combine(tempRoot, "bin");
        Directory.CreateDirectory(bin);
        await File.WriteAllTextAsync(Path.Combine(bin, "dev"), "keep");
        await File.WriteAllTextAsync(Path.Combine(bin, "dev.exe"), "keep");
        await File.WriteAllTextAsync(Path.Combine(bin, "loose.txt"), "delete");
        await File.WriteAllTextAsync(Path.Combine(bin, "tracked.txt"), "keep");

        string looseDir = Path.Combine(bin, "loose-dir");
        Directory.CreateDirectory(looseDir);
        await File.WriteAllTextAsync(Path.Combine(looseDir, "a.txt"), "delete");

        string trackedDir = Path.Combine(bin, "tracked-dir");
        Directory.CreateDirectory(trackedDir);
        await File.WriteAllTextAsync(Path.Combine(trackedDir, "a.txt"), "keep");

        Directory.CreateDirectory(sentinel);
        string sentinelFile = Path.Combine(sentinel, "keep.txt");
        await File.WriteAllTextAsync(sentinelFile, "keep");
        File.CreateSymbolicLink(Path.Combine(bin, "linked-dir"), sentinel);
        File.CreateSymbolicLink(Path.Combine(bin, "linked-file"), sentinelFile);

        CommandOutput add = await Shell.Builder("git")
          .WithArguments("-C", tempRoot, "add", "--", "bin/tracked.txt", "bin/tracked-dir/a.txt")
          .WithNoValidation()
          .CaptureAsync();
        add.Success.ShouldBeTrue();

        Directory.SetCurrentDirectory(tempRoot);
        using TestTerminal terminal = new();
        RepoCleanService service = new(terminal);
        CleanResult result = await service.CleanAsync();

        result.RootBinFilesCleaned.ShouldBe(2);
        File.Exists(Path.Combine(bin, "dev")).ShouldBeTrue();
        File.Exists(Path.Combine(bin, "dev.exe")).ShouldBeTrue();
        File.Exists(Path.Combine(bin, "loose.txt")).ShouldBeFalse();
        File.Exists(Path.Combine(bin, "tracked.txt")).ShouldBeTrue();
        Directory.Exists(looseDir).ShouldBeFalse();
        Directory.Exists(trackedDir).ShouldBeTrue();
        Directory.Exists(Path.Combine(bin, "linked-dir")).ShouldBeTrue();
        File.Exists(Path.Combine(bin, "linked-file")).ShouldBeTrue();
        File.Exists(sentinelFile).ShouldBeTrue();
        terminal.ErrorOutput.ShouldContain("reparse point");
        terminal.ErrorOutput.ShouldContain("git-tracked file");
        terminal.ErrorOutput.ShouldContain("contains git-tracked files");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        if (Directory.Exists(tempRoot))
        {
          Directory.Delete(tempRoot, recursive: true);
        }

        if (Directory.Exists(sentinel))
        {
          Directory.Delete(sentinel, recursive: true);
        }
      }
    }

    public static async Task CleanLocalFeedAsync_ShouldDeleteNupkgsAndSkipTrackedAndReparse()
    {
      string tempRoot = Path.Combine(AppContext.BaseDirectory, "temp-fixtures", Guid.NewGuid().ToString("N"));
      string outside = Path.Combine(AppContext.BaseDirectory, "temp-fixtures", $"outside-{Guid.NewGuid():N}");
      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.CreateDirectory(tempRoot);
        CommandOutput init = await Shell.Builder("git")
          .WithArguments("-C", tempRoot, "init")
          .WithNoValidation()
          .CaptureAsync();
        init.Success.ShouldBeTrue();

        string feed = Path.Combine(tempRoot, "artifacts", "packages");
        string packageDir = Path.Combine(feed, "timewarp.amuru");
        Directory.CreateDirectory(packageDir);
        string looseNupkg = Path.Combine(feed, "TimeWarp.Amuru.1.0.0.nupkg");
        string toolsNupkg = Path.Combine(feed, "TimeWarp.Amuru.Tools.1.0.0-beta.2.nupkg");
        string trackedNupkg = Path.Combine(feed, "TimeWarp.Amuru.Tracked.1.0.0.nupkg");
        await File.WriteAllTextAsync(looseNupkg, "loose");
        await File.WriteAllTextAsync(toolsNupkg, "tools");
        await File.WriteAllTextAsync(trackedNupkg, "tracked");
        await File.WriteAllTextAsync(Path.Combine(packageDir, "placeholder.txt"), "dir");

        Directory.CreateDirectory(outside);
        string outsideFile = Path.Combine(outside, "keep.txt");
        string outsideNupkg = Path.Combine(outside, "TimeWarp.Amuru.Outside.1.0.0.nupkg");
        string outsidePackageDir = Path.Combine(outside, "timewarp.amuru");
        Directory.CreateDirectory(outsidePackageDir);
        await File.WriteAllTextAsync(outsideFile, "keep");
        await File.WriteAllTextAsync(outsideNupkg, "keep");
        await File.WriteAllTextAsync(Path.Combine(outsidePackageDir, "keep.txt"), "keep");
        string linkNupkg = Path.Combine(feed, "TimeWarp.Amuru.Link.1.0.0.nupkg");
        File.CreateSymbolicLink(linkNupkg, outsideFile);
        File.CreateSymbolicLink(Path.Combine(feed, "linked-tree"), outside);

        CommandOutput add = await Shell.Builder("git")
          .WithArguments("-C", tempRoot, "add", "--", "artifacts/packages/TimeWarp.Amuru.Tracked.1.0.0.nupkg")
          .WithNoValidation()
          .CaptureAsync();
        add.Success.ShouldBeTrue();

        Directory.SetCurrentDirectory(tempRoot);
        using TestTerminal terminal = new();
        RepoCleanService service = new(terminal);
        int removed = await service.CleanLocalFeedAsync();

        removed.ShouldBe(3);
        File.Exists(looseNupkg).ShouldBeFalse();
        File.Exists(toolsNupkg).ShouldBeFalse();
        File.Exists(trackedNupkg).ShouldBeTrue();
        Directory.Exists(packageDir).ShouldBeFalse();
        File.Exists(linkNupkg).ShouldBeTrue();
        File.Exists(outsideFile).ShouldBeTrue();
        File.Exists(outsideNupkg).ShouldBeTrue();
        Directory.Exists(outsidePackageDir).ShouldBeTrue();
        Directory.Exists(Path.Combine(feed, "linked-tree")).ShouldBeTrue();
        terminal.ErrorOutput.ShouldContain("git-tracked file");
        terminal.ErrorOutput.ShouldContain("reparse point");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        if (Directory.Exists(tempRoot))
        {
          Directory.Delete(tempRoot, recursive: true);
        }

        if (Directory.Exists(outside))
        {
          Directory.Delete(outside, recursive: true);
        }
      }
    }
  }
}
