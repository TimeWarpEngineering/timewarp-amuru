#!/usr/bin/dotnet --

await RunTests<DotNetRestoreCommandTests>();

internal sealed class DotNetRestoreCommandTests
{
  public static async Task TestBasicRestoreCommand()
  {
    string command = DotNet.Restore()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore");
    
    await Task.CompletedTask;
  }

  public static async Task TestRestoreFluentConfigurationMethods()
  {
    string command = DotNet.Restore()
      .WithProject("test.csproj")
      .WithRuntime("linux-x64")
      .WithVerbosity("minimal")
      .WithPackagesDirectory("./packages")
      .WithNoCache()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore test.csproj --runtime linux-x64 --verbosity minimal --packages ./packages --no-cache");
    
    await Task.CompletedTask;
  }

  public static async Task TestRestoreMethodChainingWithSourcesAndProperties()
  {
    string command = DotNet.Restore()
      .WithProject("test.csproj")
      .WithSource("https://api.nuget.org/v3/index.json")
      .WithSource("https://nuget.pkg.github.com/MyOrg/index.json")
      .WithNoDependencies()
      .WithInteractive()
      .WithTerminalLogger("auto")
      .WithProperty("RestoreNoCache", "true")
      .WithProperty("RestoreIgnoreFailedSources", "true")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore test.csproj --tl auto --source https://api.nuget.org/v3/index.json --source https://nuget.pkg.github.com/MyOrg/index.json --no-dependencies --interactive --property:RestoreNoCache=true --property:RestoreIgnoreFailedSources=true");
    
    await Task.CompletedTask;
  }

  public static async Task TestRestoreLockFileAndWorkingDirectoryOptions()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Restore()
      .WithProject("test.csproj")
      .WithLockFilePath("./packages.lock.json")
      .WithLockedMode()
      .WithForce()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("NUGET_PACKAGES", "./temp-packages")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore test.csproj --lock-file-path ./packages.lock.json --locked-mode --force");
    
    await Task.CompletedTask;
  }

  public static async Task TestRestoreOverloadWithProjectParameter()
  {
    string command = DotNet.Restore("test.csproj")
      .WithRuntime("win-x64")
      .WithVerbosity("quiet")
      .WithNoCache()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore test.csproj --runtime win-x64 --verbosity quiet --no-cache");
    
    await Task.CompletedTask;
  }

  public static async Task TestRestoreCommandExecutionGracefulHandling()
  {
    // Test that command string is built correctly even for non-existent project
    string command = DotNet.Restore()
      .WithProject("nonexistent.csproj")
      .WithNoCache()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet restore nonexistent.csproj --no-cache");
    
    await Task.CompletedTask;
  }
}