#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.CopyItem - preserve attributes, reparse skip, missing source
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class CopyItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CopyItem_Given_>();

    public static async Task PreserveAttributes_Should_CopyTimestamps()
    {
      string source = Path.GetTempFileName();
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
      await File.WriteAllTextAsync(source, "attr");
      DateTime stamp = new(2020, 1, 2, 3, 4, 5, DateTimeKind.Local);
      File.SetLastWriteTime(source, stamp);

      try
      {
        Direct.CopyItem(source, destination, preserveAttributes: true);

        File.GetLastWriteTime(destination).ShouldBe(stamp, TimeSpan.FromSeconds(2));
      }
      finally
      {
        File.Delete(source);
        if (File.Exists(destination))
        {
          File.Delete(destination);
        }
      }
    }

    public static async Task MissingSource_Should_ThrowFileNotFoundException()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Should.Throw<FileNotFoundException>(() => Direct.CopyItem(missing, destination));

      await Task.CompletedTask;
    }

    public static async Task RecursiveDirectory_Should_IncludeDotfiles()
    {
      string sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Directory.CreateDirectory(sourceDir);

      try
      {
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "visible.txt"), "visible");
        await File.WriteAllTextAsync(Path.Combine(sourceDir, ".secret"), "secret");

        Direct.CopyItem(sourceDir, destDir, recursive: true);

        File.Exists(Path.Combine(destDir, "visible.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(destDir, ".secret")).ShouldBeTrue();
        (await File.ReadAllTextAsync(Path.Combine(destDir, ".secret"))).ShouldBe("secret");
      }
      finally
      {
        if (Directory.Exists(sourceDir))
        {
          Directory.Delete(sourceDir, recursive: true);
        }

        if (Directory.Exists(destDir))
        {
          Directory.Delete(destDir, recursive: true);
        }
      }
    }

    public static async Task DirectoryWithOutsideSymlink_Recursive_Should_LeaveOutsideIntact()
    {
      string outsideDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string treeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string outsideFile = Path.Combine(outsideDir, "outside.txt");
      string linkPath = Path.Combine(treeDir, "outside-link");

      Directory.CreateDirectory(outsideDir);
      Directory.CreateDirectory(treeDir);

      try
      {
        await File.WriteAllTextAsync(outsideFile, "outside");
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

        Direct.CopyItem(treeDir, destDir, recursive: true);

        File.Exists(Path.Combine(destDir, "local.txt")).ShouldBeTrue();
        Directory.Exists(Path.Combine(destDir, "outside-link")).ShouldBeFalse();
        File.Exists(outsideFile).ShouldBeTrue();
      }
      finally
      {
        if (Directory.Exists(treeDir))
        {
          Directory.Delete(treeDir, recursive: true);
        }

        if (Directory.Exists(destDir))
        {
          Directory.Delete(destDir, recursive: true);
        }

        if (Directory.Exists(outsideDir))
        {
          Directory.Delete(outsideDir, recursive: true);
        }
      }
    }
  }
}
