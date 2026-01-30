#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Restore() - validates the fluent API for restoring NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Restore (the method being tested)
// Tests verify command string generation for various restore options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Restore_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Restore_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Restore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet restore");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
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

    public static async Task SourcesAndProperties_Should_IncludeAllOptions()
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

    public static async Task LockFileAndWorkingDirectory_Should_IncludeLockOptions()
    {
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

    public static async Task ProjectOverload_Should_SetProject()
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

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Restore()
        .WithProject("nonexistent.csproj")
        .WithNoCache()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet restore nonexistent.csproj --no-cache");

      await Task.CompletedTask;
    }
  }
}
