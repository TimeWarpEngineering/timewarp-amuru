#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Direct.TestPath - exists, type checks, missing, whitespace
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
  public class TestPath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<TestPath_Given_>();

    public static async Task FileAndDirectory_Should_MatchItemType()
    {
      string file = Path.GetTempFileName();
      string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(directory);

      try
      {
        Direct.TestPath(file).ShouldBeTrue();
        Direct.TestPath(file, ItemType.File).ShouldBeTrue();
        Direct.TestPath(file, ItemType.Directory).ShouldBeFalse();
        Direct.TestPath(directory).ShouldBeTrue();
        Direct.TestPath(directory, ItemType.Directory).ShouldBeTrue();
        Direct.TestPath(directory, ItemType.File).ShouldBeFalse();
      }
      finally
      {
        File.Delete(file);
        Directory.Delete(directory);
      }

      await Task.CompletedTask;
    }

    public static async Task MissingOrBlank_Should_BeFalse()
    {
      Direct.TestPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).ShouldBeFalse();
      Direct.TestPath("").ShouldBeFalse();
      Direct.TestPath("   ").ShouldBeFalse();
      Direct.TestPath("", ItemType.File).ShouldBeFalse();
      Direct.TestPath("   ", ItemType.Directory).ShouldBeFalse();

      await Task.CompletedTask;
    }

    public static async Task UnknownItemType_Should_ThrowArgumentOutOfRangeException()
    {
      Should.Throw<ArgumentOutOfRangeException>(() => Direct.TestPath(".", (ItemType)999));

      await Task.CompletedTask;
    }
  }
}
