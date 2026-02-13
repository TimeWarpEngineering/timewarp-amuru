#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.AddPackage() - validates the fluent API for adding NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: AddPackage (the method being tested)
// Tests verify command string generation for various configuration options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class AddPackage_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<AddPackage_Given_>();

    public static async Task BasicPackageName_Should_BuildBasicCommand()
    {
      string command = DotNet.AddPackage("TestPackage")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add package TestPackage");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.AddPackage("TestPackage")
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add MyApp.csproj package TestPackage");

      await Task.CompletedTask;
    }

    public static async Task VersionOverload_Should_IncludeVersion()
    {
      string command = DotNet.AddPackage("TestPackage", "1.0.0")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add package TestPackage --version 1.0.0");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.AddPackage("Microsoft.Extensions.Logging")
        .WithProject("test.csproj")
        .WithFramework("net10.0")
        .WithVersion("8.0.0")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add test.csproj package Microsoft.Extensions.Logging --framework net10.0 --version 8.0.0 --no-restore");

      await Task.CompletedTask;
    }

    public static async Task PackageOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.AddPackage("Newtonsoft.Json")
        .WithProject("test.csproj")
        .WithVersion("13.0.3")
        .WithPrerelease()
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithPackageDirectory("./packages")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add test.csproj package Newtonsoft.Json --version 13.0.3 --package-directory ./packages --source https://api.nuget.org/v3/index.json --prerelease");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.AddPackage("TestPackage")
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("NUGET_ENV", "test")
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add test.csproj package TestPackage --interactive");

      await Task.CompletedTask;
    }

    public static async Task MultipleSources_Should_IncludeAllSources()
    {
      string command = DotNet.AddPackage("TestPackage")
        .WithProject("test.csproj")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithSource("https://my-private-feed.com/v3/index.json")
        .WithFramework("net10.0")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add test.csproj package TestPackage --framework net10.0 --source https://api.nuget.org/v3/index.json --source https://my-private-feed.com/v3/index.json --no-restore");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.AddPackage("TestPackage")
        .WithProject("nonexistent.csproj")
        .WithVersion("1.0.0")
        .WithNoRestore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet add nonexistent.csproj package TestPackage --version 1.0.0 --no-restore");

      await Task.CompletedTask;
    }
  }
}
