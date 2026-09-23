#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.RemoveItem - force, reparse-point safety, and read-only root clearing
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Direct (throws on failure)
// Action: RemoveItem
// Covers missing+force, file-symlink force (M12), outside symlink tree (skip reparse),
// cyclic directory symlink completion, and read-only root directory (M13)
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class RemoveItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RemoveItem_Given_>();

    public static async Task MissingPathWithoutForce_Should_ThrowFileNotFoundException()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Should.Throw<FileNotFoundException>(() => Direct.RemoveItem(missing));

      await Task.CompletedTask;
    }

    public static async Task MissingPathWithForce_Should_NotThrow()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Direct.RemoveItem(missing, force: true);

      await Task.CompletedTask;
    }

    public static async Task FileSymlinkWithForce_Should_DeleteLinkWithoutMutatingTarget()
    {
      string target = Path.GetTempFileName();
      string link = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".link");
      FileInfo targetInfo = new(target);

      try
      {
        await File.WriteAllTextAsync(target, "target-content");
        targetInfo.Refresh();
        targetInfo.IsReadOnly = true;

        try
        {
          File.CreateSymbolicLink(link, target);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
          throw new InvalidOperationException(
            "Symlink creation is required for this test. Run on Unix or enable Windows Developer Mode.",
            ex);
        }

        Direct.RemoveItem(link, force: true);

        File.Exists(link).ShouldBeFalse();
        File.Exists(target).ShouldBeTrue();
        targetInfo.Refresh();
        targetInfo.IsReadOnly.ShouldBeTrue();
      }
      finally
      {
        if (File.Exists(link))
        {
          File.Delete(link);
        }

        if (File.Exists(target))
        {
          targetInfo.Refresh();
          targetInfo.IsReadOnly = false;
          File.Delete(target);
        }
      }
    }

    public static async Task DirectoryWithOutsideSymlink_ForceRecursive_Should_LeaveOutsideIntact()
    {
      string outsideDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string treeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string outsideFile = Path.Combine(outsideDir, "outside.txt");
      string linkPath = Path.Combine(treeDir, "outside-link");
      FileInfo outsideInfo = new(outsideFile);

      Directory.CreateDirectory(outsideDir);
      Directory.CreateDirectory(treeDir);

      try
      {
        await File.WriteAllTextAsync(outsideFile, "outside");
        outsideInfo.Refresh();
        outsideInfo.IsReadOnly = true;

        try
        {
          Directory.CreateSymbolicLink(linkPath, outsideDir);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
          throw new InvalidOperationException(
            "Symlink creation is required for this test. Run on Unix or enable Windows Developer Mode.",
            ex);
        }

        await File.WriteAllTextAsync(Path.Combine(treeDir, "local.txt"), "local");

        Direct.RemoveItem(treeDir, recursive: true, force: true);

        Directory.Exists(treeDir).ShouldBeFalse();
        File.Exists(outsideFile).ShouldBeTrue();
        outsideInfo.Refresh();
        outsideInfo.IsReadOnly.ShouldBeTrue();
      }
      finally
      {
        if (Directory.Exists(treeDir))
        {
          Directory.Delete(treeDir, recursive: true);
        }

        if (File.Exists(outsideFile))
        {
          outsideInfo.Refresh();
          outsideInfo.IsReadOnly = false;
          File.Delete(outsideFile);
        }

        if (Directory.Exists(outsideDir))
        {
          Directory.Delete(outsideDir, recursive: true);
        }
      }
    }

    [Timeout(10000)]
    public static async Task CyclicDirectorySymlink_ForceRecursive_Should_Complete()
    {
      string treeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string cycleLink = Path.Combine(treeDir, "cycle");

      Directory.CreateDirectory(treeDir);

      try
      {
        try
        {
          Directory.CreateSymbolicLink(cycleLink, treeDir);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
          throw new InvalidOperationException(
            "Symlink creation is required for this test. Run on Unix or enable Windows Developer Mode.",
            ex);
        }

        await File.WriteAllTextAsync(Path.Combine(treeDir, "file.txt"), "content");

        Direct.RemoveItem(treeDir, recursive: true, force: true);

        Directory.Exists(treeDir).ShouldBeFalse();
      }
      finally
      {
        if (Directory.Exists(treeDir))
        {
          Directory.Delete(treeDir, recursive: true);
        }
      }
    }

    public static async Task ReadOnlyRootDirectory_ForceRecursive_Should_Succeed()
    {
      string treeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(treeDir);
      DirectoryInfo directoryInfo = new(treeDir);

      try
      {
        await File.WriteAllTextAsync(Path.Combine(treeDir, "file.txt"), "content");
        directoryInfo.Attributes |= FileAttributes.ReadOnly;

        Direct.RemoveItem(treeDir, recursive: true, force: true);

        Directory.Exists(treeDir).ShouldBeFalse();
      }
      finally
      {
        if (Directory.Exists(treeDir))
        {
          directoryInfo.Refresh();
          directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
          Directory.Delete(treeDir, recursive: true);
        }
      }
    }
  }
}
