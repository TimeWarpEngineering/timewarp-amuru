#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Publish() - validates the fluent API for publishing .NET applications
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Publish (the method being tested)
// Tests verify command string generation for various publishing and deployment options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Publish_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Publish_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.Publish()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Publish()
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Publish()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithFramework("net10.0")
        .WithRuntime("win-x64")
        .WithOutput("./publish")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish test.csproj --configuration Release --framework net10.0 --runtime win-x64 --output ./publish --no-restore");

      await Task.CompletedTask;
    }

    public static async Task AdvancedDeploymentOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Publish()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithRuntime("linux-x64")
        .WithSelfContained()
        .WithReadyToRun()
        .WithSingleFile()
        .WithTrimmed()
        .WithNoLogo()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish test.csproj --configuration Release --runtime linux-x64 --nologo --self-contained --property:PublishReadyToRun=true --property:PublishSingleFile=true --property:PublishTrimmed=true");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Publish()
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("PUBLISH_ENV", "production")
        .WithArchitecture("x64")
        .WithOperatingSystem("linux")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish test.csproj --arch x64 --os linux");

      await Task.CompletedTask;
    }

    public static async Task MSBuildPropertiesAndPublishConfig_Should_IncludeAllOptions()
    {
      string command = DotNet.Publish()
        .WithProject("test.csproj")
        .WithConfiguration("Release")
        .WithProperty("PublishProfile", "Production")
        .WithProperty("EnvironmentName", "Staging")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithVerbosity("minimal")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish test.csproj --configuration Release --verbosity minimal --source https://api.nuget.org/v3/index.json --property:PublishProfile=Production --property:EnvironmentName=Staging");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.Publish("test.csproj")
        .WithConfiguration("Release")
        .WithRuntime("win-x64")
        .WithNoSelfContained()
        .WithNoBuild()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish test.csproj --configuration Release --runtime win-x64 --no-build --no-self-contained");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Publish()
        .WithProject("nonexistent.csproj")
        .WithConfiguration("Release")
        .WithRuntime("win-x64")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet publish nonexistent.csproj --configuration Release --runtime win-x64 --no-restore");

      await Task.CompletedTask;
    }
  }
}
