#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.NewItem - mkdir with parents, touch, existing-item behavior
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
  public class NewItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NewItem_Given_>();

    public static async Task DirectoryWithParents_Should_CreateTree()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string nested = Path.Combine(root, "a", "b");

      try
      {
        CommandOutput result = Commands.NewItem(nested, ItemType.Directory);

        result.Success.ShouldBeTrue();
        Directory.Exists(nested).ShouldBeTrue();
      }
      finally
      {
        if (Directory.Exists(root))
        {
          Directory.Delete(root, recursive: true);
        }
      }

      await Task.CompletedTask;
    }

    public static async Task File_Should_CreateEmptyFile()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");

      try
      {
        CommandOutput result = Commands.NewItem(path, ItemType.File);

        result.Success.ShouldBeTrue();
        File.Exists(path).ShouldBeTrue();
        new FileInfo(path).Length.ShouldBe(0);
      }
      finally
      {
        if (File.Exists(path))
        {
          File.Delete(path);
        }
      }

      await Task.CompletedTask;
    }

    public static async Task ExistingDirectory_Should_Succeed()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(path);

      try
      {
        CommandOutput result = Commands.Mkdir(path);

        result.Success.ShouldBeTrue();
        Directory.Exists(path).ShouldBeTrue();
      }
      finally
      {
        Directory.Delete(path);
      }

      await Task.CompletedTask;
    }

    public static async Task ExistingFile_Touch_Should_Succeed()
    {
      string path = Path.GetTempFileName();
      await File.WriteAllTextAsync(path, "keep");
      DateTime original = File.GetLastWriteTime(path);
      await Task.Delay(20);

      try
      {
        CommandOutput result = Commands.Touch(path);

        result.Success.ShouldBeTrue();
        (await File.ReadAllTextAsync(path)).ShouldBe("keep");
        File.GetLastWriteTime(path).ShouldBeGreaterThanOrEqualTo(original);
      }
      finally
      {
        File.Delete(path);
      }
    }

    public static async Task DirectoryWhereFileExists_Should_Fail()
    {
      string path = Path.GetTempFileName();

      try
      {
        CommandOutput result = Commands.Mkdir(path);

        result.Success.ShouldBeFalse();
        result.Stderr.ShouldContain("file exists");
      }
      finally
      {
        File.Delete(path);
      }

      await Task.CompletedTask;
    }

    public static async Task FileWhereDirectoryExists_Should_Fail()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(path);

      try
      {
        CommandOutput result = Commands.Touch(path);

        result.Success.ShouldBeFalse();
        result.Stderr.ShouldContain("is a directory");
      }
      finally
      {
        Directory.Delete(path);
      }

      await Task.CompletedTask;
    }
  }
}
