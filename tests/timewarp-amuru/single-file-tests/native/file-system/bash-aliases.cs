#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Bash aliases - validates Unix-style command aliases
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: BashAliases (the static methods providing Unix-style command aliases)
// Action: Cat, Ls, Pwd, Cd, Rm, Cp, Mv, Mkdir, Touch, Test, Find, Stat
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
      // Cd mutates process-global CurrentDirectory; restore it for later tests
      string originalDirectory = Directory.GetCurrentDirectory();
      try
      {
        string tempPath = Path.GetTempPath();

        CommandOutput cdResult = Cd(tempPath);
        cdResult.Success.ShouldBeTrue();

        await Task.CompletedTask;
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDirectory);
      }
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

    public static async Task CpMvMkdirTouchTestFindStat_Should_Work()
    {
      string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      string source = Path.Combine(root, "source.txt");
      string copied = Path.Combine(root, "copied.txt");
      string moved = Path.Combine(root, "moved.txt");
      string nested = Path.Combine(root, "sub", "dir");

      try
      {
        Mkdir(root).Success.ShouldBeTrue();
        Touch(source).Success.ShouldBeTrue();
        await File.WriteAllTextAsync(source, "payload");

        Cp(source, copied).Success.ShouldBeTrue();
        File.Exists(copied).ShouldBeTrue();

        Mv(copied, moved).Success.ShouldBeTrue();
        File.Exists(moved).ShouldBeTrue();
        File.Exists(copied).ShouldBeFalse();

        Test(source).Success.ShouldBeTrue();
        Test(Path.Combine(root, "missing")).Success.ShouldBeFalse();
        Test(root, ItemType.Directory).Success.ShouldBeTrue();

        Mkdir(nested).Success.ShouldBeTrue();
        Directory.Exists(nested).ShouldBeTrue();

        CommandOutput findResult = Find(root, new FindCriteria { Name = "*.txt" });
        findResult.Success.ShouldBeTrue();
        findResult.Stdout.ShouldContain("source.txt");

        CommandOutput statResult = Stat(source);
        statResult.Success.ShouldBeTrue();
        statResult.Stdout.ShouldContain("Type: File");

        CpDirect(source, Path.Combine(root, "direct.txt"));
        File.Exists(Path.Combine(root, "direct.txt")).ShouldBeTrue();
        TestDirect(source).ShouldBeTrue();
      }
      finally
      {
        if (Directory.Exists(root))
        {
          Directory.Delete(root, recursive: true);
        }
      }
    }
  }
}
