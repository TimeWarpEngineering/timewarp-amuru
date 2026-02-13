#!/usr/bin/dotnet --

#region Purpose
// Tests for Bash aliases - validates Unix-style command aliases (Cat, Ls, Pwd, Cd, Rm)
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: BashAliases (the static methods providing Unix-style command aliases)
// Action: Various alias methods (Cat, Ls, Pwd, Cd, Rm, RmDirect)
// Tests verify each alias works correctly
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace BashAliases_
{
  [TestTag("Native")]
  public class Aliases_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Aliases_Given_>();

    public static async Task Cat_Should_ReadFileContent()
    {
      string testFile = Path.GetTempFileName();
      await File.WriteAllTextAsync(testFile, "test content");

      try
      {
        CommandOutput catResult = Cat(testFile);
        catResult.Success.ShouldBeTrue();
        catResult.Stdout.ShouldBe("test content");
      }
      finally
      {
        File.Delete(testFile);
      }
    }

    public static async Task Ls_Should_ListDirectory()
    {
      CommandOutput lsResult = Ls(Path.GetTempPath());
      lsResult.Success.ShouldBeTrue();

      await Task.CompletedTask;
    }

    public static async Task Pwd_Should_ReturnCurrentDirectory()
    {
      CommandOutput pwdResult = Pwd();
      pwdResult.Success.ShouldBeTrue();
      pwdResult.Stdout.ShouldNotBeNullOrEmpty();

      await Task.CompletedTask;
    }

    public static async Task Cd_Should_ChangeDirectory()
    {
      string tempPath = Path.GetTempPath();

      CommandOutput cdResult = Cd(tempPath);
      cdResult.Success.ShouldBeTrue();

      await Task.CompletedTask;
    }

    public static async Task Rm_Should_RemoveFile()
    {
      string testFile = Path.GetTempFileName();
      await File.WriteAllTextAsync(testFile, "test content");

      CommandOutput result = Rm(testFile);

      result.Success.ShouldBeTrue();
      File.Exists(testFile).ShouldBeFalse();
    }

    public static async Task RmDirect_Should_RemoveFile()
    {
      string testFile = Path.GetTempFileName();
      await File.WriteAllTextAsync(testFile, "test content");

      RmDirect(testFile);

      File.Exists(testFile).ShouldBeFalse();

      await Task.CompletedTask;
    }
  }
}
