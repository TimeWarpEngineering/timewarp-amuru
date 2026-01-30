#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Watch() - validates the fluent API for file watching during development
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Watch (the method being tested)
// Tests verify command string generation for run, test, build, and watch-specific options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Watch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Watch_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetWatchBuilder watchBuilder = DotNet.Watch();

      watchBuilder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Run_Should_BuildWatchRunCommand()
    {
      string command = DotNet.Watch()
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch run");

      await Task.CompletedTask;
    }

    public static async Task Test_Should_BuildWatchTestCommand()
    {
      string command = DotNet.Watch()
        .Test()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch test");

      await Task.CompletedTask;
    }

    public static async Task WatchBuild_Should_BuildWatchBuildCommand()
    {
      string command = DotNet.Watch()
        .Build()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch build");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectOption()
    {
      string command = DotNet.Watch()
        .WithProject("MyApp.csproj")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --project MyApp.csproj run");

      await Task.CompletedTask;
    }

    public static async Task BasicOptions_Should_IncludeOptions()
    {
      string command = DotNet.Watch()
        .WithQuiet()
        .WithVerbose()
        .WithList()
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --quiet --verbose --list run");

      await Task.CompletedTask;
    }

    public static async Task NoOptions_Should_IncludeNoFlags()
    {
      string command = DotNet.Watch()
        .WithNoRestore()
        .WithNoLaunchProfile()
        .WithNoHotReload()
        .WithNoBuild()
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --no-restore --no-launch-profile --no-hot-reload --no-build run");

      await Task.CompletedTask;
    }

    public static async Task IncludeExcludePatterns_Should_IncludePatterns()
    {
      string command = DotNet.Watch()
        .WithInclude("**/*.cs")
        .WithInclude("**/*.cshtml")
        .WithExclude("**/bin/**")
        .WithExclude("**/obj/**")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --include **/*.cs --include **/*.cshtml --exclude **/bin/** --exclude **/obj/** run");

      await Task.CompletedTask;
    }

    public static async Task BuildConfiguration_Should_IncludeOptions()
    {
      string command = DotNet.Watch()
        .WithConfiguration("Release")
        .WithTargetFramework("net10.0")
        .WithRuntime("linux-x64")
        .WithVerbosity("detailed")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --framework net10.0 --configuration Release --runtime linux-x64 --verbosity detailed run");

      await Task.CompletedTask;
    }

    public static async Task PropertiesAndLaunchProfile_Should_IncludeOptions()
    {
      string command = DotNet.Watch()
        .WithProperty("Configuration=Debug")
        .WithProperty("Platform=x64")
        .WithLaunchProfile("Development")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --property Configuration=Debug --property Platform=x64 --launch-profile Development run");

      await Task.CompletedTask;
    }

    public static async Task AdditionalArguments_Should_IncludeAfterSubcommand()
    {
      string command = DotNet.Watch()
        .WithArguments("--environment", "Development")
        .WithArgument("--port")
        .WithArgument("5000")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch run --environment Development --port 5000");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Watch()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch run");

      await Task.CompletedTask;
    }

    public static async Task ComprehensiveOptions_Should_BuildCompleteCommand()
    {
      string command = DotNet.Watch()
        .WithProject("MyApp.csproj")
        .WithConfiguration("Release")
        .WithTargetFramework("net10.0")
        .WithVerbosity("minimal")
        .WithInclude("**/*.cs")
        .WithExclude("**/bin/**")
        .WithProperty("DefineConstants=RELEASE")
        .WithNoRestore()
        .WithArguments("--environment", "Production")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --project MyApp.csproj --no-restore --include **/*.cs --exclude **/bin/** --property DefineConstants=RELEASE --framework net10.0 --configuration Release --verbosity minimal run --environment Production");

      await Task.CompletedTask;
    }

    public static async Task ListWithProject_Should_IncludeListOption()
    {
      string command = DotNet.Watch()
        .WithList()
        .WithProject("test.csproj")
        .Run()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet watch --project test.csproj --list run");

      await Task.CompletedTask;
    }
  }
}
