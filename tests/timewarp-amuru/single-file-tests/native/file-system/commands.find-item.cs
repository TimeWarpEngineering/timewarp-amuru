#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.FindItem - name, size, date, attributes, missing root
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
  public class FindItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FindItem_Given_>();

    public static async Task NameGlob_Should_MatchDescendants()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(Path.Combine(root, "sub"));
      await File.WriteAllTextAsync(Path.Combine(root, "a.txt"), "a");
      await File.WriteAllTextAsync(Path.Combine(root, "sub", "b.txt"), "b");
      await File.WriteAllTextAsync(Path.Combine(root, "c.log"), "c");

      try
      {
        CommandOutput result = Commands.FindItem(root, new FindCriteria { Name = "*.txt" });

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("a.txt");
        result.Stdout.ShouldContain("b.txt");
        result.Stdout.ShouldNotContain("c.log");
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }

    public static async Task SizeFilter_Should_ExcludeSmallFiles()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(root);
      await File.WriteAllBytesAsync(Path.Combine(root, "small.bin"), new byte[10]);
      await File.WriteAllBytesAsync(Path.Combine(root, "large.bin"), new byte[200]);

      try
      {
        CommandOutput result = Commands.FindItem(root, new FindCriteria { MinSize = 100 });

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("large.bin");
        result.Stdout.ShouldNotContain("small.bin");
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }

    public static async Task DateFilter_Should_ExcludeOldFiles()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(root);
      string oldFile = Path.Combine(root, "old.txt");
      string newFile = Path.Combine(root, "new.txt");
      await File.WriteAllTextAsync(oldFile, "old");
      await File.WriteAllTextAsync(newFile, "new");
      File.SetLastWriteTime(oldFile, DateTime.Now.AddDays(-7));

      try
      {
        CommandOutput result = Commands.FindItem(
          root,
          new FindCriteria { ModifiedAfter = DateTimeOffset.Now.AddDays(-1) });

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("new.txt");
        result.Stdout.ShouldNotContain("old.txt");
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }

    public static async Task DirectoryAttribute_Should_MatchDirectories()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string child = Path.Combine(root, "child");
      Directory.CreateDirectory(child);
      await File.WriteAllTextAsync(Path.Combine(root, "file.txt"), "x");

      try
      {
        CommandOutput result = Commands.FindItem(
          root,
          new FindCriteria { Attributes = FileAttributes.Directory });

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("child");
        result.Stdout.ShouldNotContain("file.txt");
      }
      finally
      {
        Directory.Delete(root, recursive: true);
      }
    }

    public static async Task MissingRoot_Should_Fail()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      CommandOutput result = Commands.FindItem(missing);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");

      await Task.CompletedTask;
    }
  }
}
