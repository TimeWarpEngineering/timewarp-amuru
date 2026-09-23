#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.CopyItem - file, directory, overwrite, missing source
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
  public class CopyItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CopyItem_Given_>();

    public static async Task File_Should_BeCopied()
    {
      string source = Path.GetTempFileName();
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
      await File.WriteAllTextAsync(source, "copied");

      try
      {
        CommandOutput result = Commands.CopyItem(source, destination);

        result.Success.ShouldBeTrue();
        File.Exists(destination).ShouldBeTrue();
        (await File.ReadAllTextAsync(destination)).ShouldBe("copied");
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

    public static async Task DirectoryWithRecursive_Should_BeCopied()
    {
      string sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(sourceDir);
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "a.txt"), "a");
      Directory.CreateDirectory(Path.Combine(sourceDir, "sub"));
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "sub", "b.txt"), "b");

      try
      {
        CommandOutput result = Commands.CopyItem(sourceDir, destDir, recursive: true);

        result.Success.ShouldBeTrue();
        File.Exists(Path.Combine(destDir, "a.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(destDir, "sub", "b.txt")).ShouldBeTrue();
      }
      finally
      {
        Directory.Delete(sourceDir, recursive: true);
        if (Directory.Exists(destDir))
        {
          Directory.Delete(destDir, recursive: true);
        }
      }
    }

    public static async Task DirectoryWithoutRecursive_Should_Fail()
    {
      string sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(sourceDir);

      try
      {
        CommandOutput result = Commands.CopyItem(sourceDir, destDir);

        result.Success.ShouldBeFalse();
        result.Stderr.ShouldContain("is a directory");
        Directory.Exists(destDir).ShouldBeFalse();
      }
      finally
      {
        Directory.Delete(sourceDir, recursive: true);
      }
    }

    public static async Task ExistingDestinationWithoutOverwrite_Should_Fail()
    {
      string source = Path.GetTempFileName();
      string destination = Path.GetTempFileName();
      await File.WriteAllTextAsync(source, "new");
      await File.WriteAllTextAsync(destination, "old");

      try
      {
        CommandOutput result = Commands.CopyItem(source, destination);

        result.Success.ShouldBeFalse();
        (await File.ReadAllTextAsync(destination)).ShouldBe("old");
      }
      finally
      {
        File.Delete(source);
        File.Delete(destination);
      }
    }

    public static async Task ExistingDestinationWithOverwrite_Should_Replace()
    {
      string source = Path.GetTempFileName();
      string destination = Path.GetTempFileName();
      await File.WriteAllTextAsync(source, "new");
      await File.WriteAllTextAsync(destination, "old");

      try
      {
        CommandOutput result = Commands.CopyItem(source, destination, overwrite: true);

        result.Success.ShouldBeTrue();
        (await File.ReadAllTextAsync(destination)).ShouldBe("new");
      }
      finally
      {
        File.Delete(source);
        File.Delete(destination);
      }
    }

    public static async Task MissingSource_Should_Fail()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destination = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      CommandOutput result = Commands.CopyItem(missing, destination);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");
      result.ExitCode.ShouldBe(1);

      await Task.CompletedTask;
    }

    public static async Task Glob_Should_CopyMatchingFiles()
    {
      string sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(sourceDir);
      Directory.CreateDirectory(destDir);
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "keep.txt"), "k");
      await File.WriteAllTextAsync(Path.Combine(sourceDir, "skip.log"), "s");

      try
      {
        CommandOutput result = Commands.CopyItem(Path.Combine(sourceDir, "*.txt"), destDir);

        result.Success.ShouldBeTrue();
        File.Exists(Path.Combine(destDir, "keep.txt")).ShouldBeTrue();
        File.Exists(Path.Combine(destDir, "skip.log")).ShouldBeFalse();
      }
      finally
      {
        Directory.Delete(sourceDir, recursive: true);
        Directory.Delete(destDir, recursive: true);
      }
    }
  }
}
