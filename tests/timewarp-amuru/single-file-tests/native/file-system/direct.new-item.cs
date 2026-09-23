#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.NewItem - create file/directory and existing-item exceptions
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
  public class NewItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NewItem_Given_>();

    public static async Task File_Should_CreateEmptyFile()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");

      try
      {
        FileSystemInfo info = Direct.NewItem(path, ItemType.File);

        info.Exists.ShouldBeTrue();
        File.Exists(path).ShouldBeTrue();
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

    public static async Task DirectoryWhereFileExists_Should_ThrowIOException()
    {
      string path = Path.GetTempFileName();

      try
      {
        Should.Throw<IOException>(() => Direct.Mkdir(path));
      }
      finally
      {
        File.Delete(path);
      }

      await Task.CompletedTask;
    }
  }
}
