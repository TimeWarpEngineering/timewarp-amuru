#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Build() - validates the fluent API for building .NET projects
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Build (the method being tested)
// Tests verify command string generation for various configuration options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Build_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Build_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Build()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Build()
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithFramework("net10.0")
        .WithNoRestore()
        .WithOutput("bin/Debug")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --framework net10.0 --output bin/Debug --no-restore");

      await Task.CompletedTask;
    }

    public static async Task AdvancedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithArchitecture("x64")
        .WithOperatingSystem("linux")
        .WithNoRestore()
        .WithNoDependencies()
        .WithNoIncremental()
        .WithVerbosity("minimal")
        .WithProperty("Platform", "AnyCPU")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Release --arch x64 --os linux --verbosity minimal --no-restore --no-dependencies --no-incremental --property:Platform=AnyCPU");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("BUILD_ENV", "test")
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --nologo");

      await Task.CompletedTask;
    }

    public static async Task MSBuildProperties_Should_IncludeAllProperties()
    {
      string command = DotNet.Build()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithProperty("RestoreNoCache", "true")
        .WithProperty("DisableImplicitNuGetFallbackFolder", "true")
        .WithProperty("RestoreIgnoreFailedSources", "true")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --property:RestoreNoCache=true --property:DisableImplicitNuGetFallbackFolder=true --property:RestoreIgnoreFailedSources=true");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.Build("test.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet build test.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string commandString = DotNet.Build()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      commandString.ShouldBe("dotnet build nonexistent.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }
  }
}
