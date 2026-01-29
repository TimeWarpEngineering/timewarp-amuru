#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;

namespace TimeWarp.Amuru.Tests;

[TestTag("Native")]
public class AliasTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<AliasTests>();

  public static async Task Should_support_cat_alias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    try
    {
      // Test Cat alias
      CommandOutput catResult = Cat(testFile);
      catResult.Success.ShouldBeTrue();
      catResult.Stdout.ShouldBe("test content");
    }
    finally
    {
      File.Delete(testFile);
    }

    await Task.CompletedTask;
  }

  public static async Task Should_support_ls_alias()
  {
    // Test Ls alias
    CommandOutput lsResult = Ls(Path.GetTempPath());
    lsResult.Success.ShouldBeTrue();

    await Task.CompletedTask;
  }

  public static async Task Should_support_pwd_alias()
  {
    // Test Pwd alias
    CommandOutput pwdResult = Pwd();
    pwdResult.Success.ShouldBeTrue();
    pwdResult.Stdout.ShouldNotBeNullOrEmpty();

    await Task.CompletedTask;
  }

  public static async Task Should_support_cd_alias()
  {
    string tempPath = Path.GetTempPath();

    // Test Cd alias
    CommandOutput cdResult = Cd(tempPath);
    cdResult.Success.ShouldBeTrue();

    await Task.CompletedTask;
  }

  public static async Task Should_support_rm_alias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test Rm alias
    CommandOutput result = Rm(testFile);

    result.Success.ShouldBeTrue();
    File.Exists(testFile).ShouldBeFalse();

    await Task.CompletedTask;
  }

  public static async Task Should_support_rm_direct_alias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test RmDirect alias
    RmDirect(testFile);

    File.Exists(testFile).ShouldBeFalse();

    await Task.CompletedTask;
  }
}
