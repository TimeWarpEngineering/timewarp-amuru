#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Run() - validates the fluent API for running .NET projects
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Run (the method being tested)
// Tests verify command string generation for various configuration options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Run_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Run_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Run()
        .WithProject("test.csproj")
        .WithConfiguration("Debug")
        .WithFramework("net10.0")
        .WithNoRestore()
        .WithArguments("--help")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run --project test.csproj --configuration Debug --framework net10.0 --no-restore -- --help");

      await Task.CompletedTask;
    }

    public static async Task MethodChaining_Should_BuildCompleteCommand()
    {
      string command = DotNet.Run()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithNoRestore()
        .WithNoBuild()
        .WithVerbosity("minimal")
        .WithArguments("arg1", "arg2")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run --project test.csproj --configuration Release --verbosity minimal --no-restore --no-build -- arg1 arg2");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Run()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("TEST_VAR", "test_value")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run --project test.csproj");

      await Task.CompletedTask;
    }

    public static async Task ExtendedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Run()
        .WithProject("test.csproj")
        .WithArchitecture("x64")
        .WithOperatingSystem("linux")
        .WithLaunchProfile("Development")
        .WithForce()
        .WithInteractive()
        .WithTerminalLogger("auto")
        .WithProperty("Configuration", "Debug")
        .WithProperty("Platform", "AnyCPU")
        .WithProcessEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
        .WithNoLaunchProfile()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run --project test.csproj --arch x64 --os linux --launch-profile Development --tl auto --no-launch-profile --force --interactive --property:Configuration=Debug --property:Platform=AnyCPU -e ASPNETCORE_ENVIRONMENT=Development");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Run()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Debug")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet run --project nonexistent.csproj --configuration Debug --no-restore");

      await Task.CompletedTask;
    }
  }
}
