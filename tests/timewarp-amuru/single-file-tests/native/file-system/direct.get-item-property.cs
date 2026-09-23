#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.GetItemProperty - snapshot fields and missing path
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
  public class GetItemProperty_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetItemProperty_Given_>();

    public static async Task File_Should_ReturnSnapshot()
    {
      string path = Path.GetTempFileName();
      await File.WriteAllTextAsync(path, "hello");

      try
      {
        ItemProperty itemProperty = Direct.GetItemProperty(path);

        itemProperty.Exists.ShouldBeTrue();
        itemProperty.IsFile.ShouldBeTrue();
        itemProperty.IsDirectory.ShouldBeFalse();
        itemProperty.Length.ShouldBe(5);
        itemProperty.Name.ShouldBe(Path.GetFileName(path));
        itemProperty.LinkTarget.ShouldBeNull();
      }
      finally
      {
        File.Delete(path);
      }
    }

    public static async Task MissingPath_Should_ThrowFileNotFoundException()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      Should.Throw<FileNotFoundException>(() => Direct.GetItemProperty(missing));

      await Task.CompletedTask;
    }
  }
}
