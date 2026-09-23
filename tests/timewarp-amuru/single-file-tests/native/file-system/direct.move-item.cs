#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.MoveItem - rename, overwrite, missing source
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
  public class MoveItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<MoveItem_Given_>();

    public static async Task FileRename_Should_MovePath()
    {
      string source = Path.GetTempFileName();
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
      await File.WriteAllTextAsync(source, "moved");

      try
      {
        Direct.MoveItem(source, destination);

        File.Exists(source).ShouldBeFalse();
        (await File.ReadAllTextAsync(destination)).ShouldBe("moved");
      }
      finally
      {
        if (File.Exists(source))
        {
          File.Delete(source);
        }

        if (File.Exists(destination))
        {
          File.Delete(destination);
        }
      }
    }

    public static async Task ExistingFileWithoutOverwrite_Should_Throw()
    {
      string source = Path.GetTempFileName();
      string destination = Path.GetTempFileName();
      await File.WriteAllTextAsync(source, "new");
      await File.WriteAllTextAsync(destination, "old");

      try
      {
        Should.Throw<IOException>(() => Direct.MoveItem(source, destination));
        (await File.ReadAllTextAsync(destination)).ShouldBe("old");
        File.Exists(source).ShouldBeTrue();
      }
      finally
      {
        File.Delete(source);
        File.Delete(destination);
      }
    }

    public static async Task ExistingFileWithOverwrite_Should_Replace()
    {
      string source = Path.GetTempFileName();
      string destination = Path.GetTempFileName();
      await File.WriteAllTextAsync(source, "new");
      await File.WriteAllTextAsync(destination, "old");

      try
      {
        Direct.MoveItem(source, destination, overwrite: true);

        File.Exists(source).ShouldBeFalse();
        (await File.ReadAllTextAsync(destination)).ShouldBe("new");
      }
      finally
      {
        if (File.Exists(source))
        {
          File.Delete(source);
        }

        File.Delete(destination);
      }
    }

    public static async Task MissingSource_Should_ThrowFileNotFoundException()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Should.Throw<FileNotFoundException>(() => Direct.MoveItem(missing, destination));

      await Task.CompletedTask;
    }

    public static async Task ExistingDestinationDirectoryWithOverwrite_Should_ThrowAndPreserve()
    {
      string sourceName = Guid.NewGuid().ToString("N");
      string sourceDir = Path.Combine(Path.GetTempPath(), sourceName);
      string containerDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string finalDestination = Path.Combine(containerDir, sourceName);
      string markerPath = Path.Combine(finalDestination, "marker.txt");

      Directory.CreateDirectory(sourceDir);
      Directory.CreateDirectory(finalDestination);
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "source.txt"), "source");
      await File.WriteAllTextAsync(markerPath, "keep-me");

      try
      {
        // Destination container exists, so final path is container/sourceName — already a directory.
        Should.Throw<IOException>(() => Direct.MoveItem(sourceDir, containerDir, overwrite: true));

        Directory.Exists(finalDestination).ShouldBeTrue();
        File.Exists(markerPath).ShouldBeTrue();
        (await File.ReadAllTextAsync(markerPath)).ShouldBe("keep-me");
      }
      finally
      {
        if (Directory.Exists(sourceDir))
        {
          Directory.Delete(sourceDir, recursive: true);
        }

        if (Directory.Exists(containerDir))
        {
          Directory.Delete(containerDir, recursive: true);
        }
      }
    }
  }
}
