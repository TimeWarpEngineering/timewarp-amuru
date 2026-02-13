#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Clean() - validates the fluent API for cleaning .NET projects
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Clean (the method being tested)
// Tests verify command string generation for various configuration options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Clean_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Clean_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Clean()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Clean()
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Clean()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithFramework("net10.0")
        .WithOutput("bin/Debug")
        .WithVerbosity("minimal")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean test.csproj --configuration Debug --framework net10.0 --output bin/Debug --verbosity minimal");

      await Task.CompletedTask;
    }

    public static async Task RuntimeAndProperties_Should_IncludeAllOptions()
    {
      string command = DotNet.Clean()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithRuntime("linux-x64")
        .WithNoLogo()
        .WithProperty("Platform", "AnyCPU")
        .WithProperty("CleanTargets", "All")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean test.csproj --configuration Release --runtime linux-x64 --nologo --property:Platform=AnyCPU --property:CleanTargets=All");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Clean()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("CLEAN_ENV", "test")
        .WithVerbosity("quiet")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean test.csproj --verbosity quiet");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.Clean("test.csproj")
        .WithConfiguration("Debug")
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean test.csproj --configuration Debug --nologo");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Clean()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Debug")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet clean nonexistent.csproj --configuration Debug");

      await Task.CompletedTask;
    }
  }
}
