#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

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
      catResult.Success.ShouldBeTrue();
      catResult.Stdout.ShouldBe("test content");
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
    lsResult.Success.ShouldBeTrue();

    await Task.CompletedTask;
  }

  public static async Task TestPwdAlias()
  {
    // Test Pwd alias
    CommandOutput pwdResult = Pwd();
    pwdResult.Success.ShouldBeTrue();
    pwdResult.Stdout.ShouldNotBeNullOrEmpty();

    await Task.CompletedTask;
  }

  public static async Task TestCdAlias()
  {
    string tempPath = Path.GetTempPath();

    // Test Cd alias
    CommandOutput cdResult = Cd(tempPath);
    cdResult.Success.ShouldBeTrue();

    await Task.CompletedTask;
  }

  public static async Task TestRmAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test Rm alias
    CommandOutput result = Rm(testFile);

    result.Success.ShouldBeTrue();
    File.Exists(testFile).ShouldBeFalse();
  }

  public static async Task TestRmDirectAlias()
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