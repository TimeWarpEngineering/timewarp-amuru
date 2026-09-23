#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.MoveItem - rename, move into directory, missing source
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
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
        CommandOutput result = Commands.MoveItem(source, destination);

        result.Success.ShouldBeTrue();
        File.Exists(source).ShouldBeFalse();
        File.Exists(destination).ShouldBeTrue();
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

    public static async Task FileIntoDirectory_Should_KeepName()
    {
      string source = Path.GetTempFileName();
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(destDir);
      await File.WriteAllTextAsync(source, "into-dir");
      string expected = Path.Combine(destDir, Path.GetFileName(source));

      try
      {
        CommandOutput result = Commands.MoveItem(source, destDir);

        result.Success.ShouldBeTrue();
        File.Exists(expected).ShouldBeTrue();
        File.Exists(source).ShouldBeFalse();
      }
      finally
      {
        if (Directory.Exists(destDir))
        {
          Directory.Delete(destDir, recursive: true);
        }

        if (File.Exists(source))
        {
          File.Delete(source);
        }
      }
    }

    public static async Task Directory_Should_BeMoved()
    {
      string sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(sourceDir);
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "f.txt"), "x");

      try
      {
        CommandOutput result = Commands.MoveItem(sourceDir, destDir);

        result.Success.ShouldBeTrue();
        Directory.Exists(sourceDir).ShouldBeFalse();
        File.Exists(Path.Combine(destDir, "f.txt")).ShouldBeTrue();
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

    public static async Task MissingSource_Should_Fail()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      CommandOutput result = Commands.MoveItem(missing, destination);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");

      await Task.CompletedTask;
    }
  }
}
