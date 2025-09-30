#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Native.Aliases.Bash;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

await TestRunner.RunTests<AliasTests>();

internal sealed class AliasTests
{
  public static async Task TestCatAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    try
    {
      // Test Cat alias
      CommandOutput catResult = Cat(testFile);
      AssertTrue(
        catResult.Success && catResult.Stdout == "test content",
        "Cat() alias should work like GetContent()"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestLsAlias()
  {
    // Test Ls alias
    CommandOutput lsResult = Ls(Path.GetTempPath());
    AssertTrue(
      lsResult.Success,
      "Ls() alias should work like GetChildItem()"
    );

    await Task.CompletedTask;
  }

  public static async Task TestPwdAlias()
  {
    // Test Pwd alias
    CommandOutput pwdResult = Pwd();
    AssertTrue(
      pwdResult.Success && !string.IsNullOrEmpty(pwdResult.Stdout),
      "Pwd() alias should return current directory"
    );

    await Task.CompletedTask;
  }

  public static async Task TestCdAlias()
  {
    string tempPath = Path.GetTempPath();

    // Test Cd alias
    CommandOutput cdResult = Cd(tempPath);
    AssertTrue(
      cdResult.Success,
      "Cd() alias should work like SetLocation()"
    );

    await Task.CompletedTask;
  }

  public static async Task TestRmAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test Rm alias
    CommandOutput result = Rm(testFile);

    AssertTrue(
      result.Success,
      "Rm() alias should work like RemoveItem()"
    );

    AssertFalse(
      File.Exists(testFile),
      "File should be deleted by Rm alias"
    );
  }

  public static async Task TestRmDirectAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test RmDirect alias
    RmDirect(testFile);

    AssertFalse(
      File.Exists(testFile),
      "File should be deleted by RmDirect alias"
    );

    await Task.CompletedTask;
  }
}