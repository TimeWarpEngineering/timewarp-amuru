#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.TestPath - exists, file, directory, missing
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
  public class TestPath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<TestPath_Given_>();

    public static async Task ExistingFile_Should_Succeed()
    {
      string path = Path.GetTempFileName();

      try
      {
        Commands.TestPath(path).Success.ShouldBeTrue();
        Commands.TestPath(path, ItemType.File).Success.ShouldBeTrue();
        Commands.TestPath(path, ItemType.Directory).Success.ShouldBeFalse();
      }
      finally
      {
        File.Delete(path);
      }

      await Task.CompletedTask;
    }

    public static async Task ExistingDirectory_Should_Succeed()
    {
      string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(path);

      try
      {
        Commands.TestPath(path).Success.ShouldBeTrue();
        Commands.TestPath(path, ItemType.Directory).Success.ShouldBeTrue();
        Commands.TestPath(path, ItemType.File).Success.ShouldBeFalse();
      }
      finally
      {
        Directory.Delete(path);
      }

      await Task.CompletedTask;
    }

    public static async Task MissingPath_Should_Fail()
    {
      string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

      CommandOutput result = Commands.TestPath(missing);

      result.Success.ShouldBeFalse();
      result.ExitCode.ShouldBe(1);

      await Task.CompletedTask;
    }
  }
}
