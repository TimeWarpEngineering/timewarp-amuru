#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.RemovePackage() - validates the fluent API for removing NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: RemovePackage (the method being tested)
// Tests verify command string generation for package removal options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class RemovePackage_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RemovePackage_Given_>();

    public static async Task PackageName_Should_BuildBasicCommand()
    {
      string command = DotNet.RemovePackage("TestPackage")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove package TestPackage");

      await Task.CompletedTask;
    }

    public static async Task WithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.RemovePackage("TestPackage")
        .WithProject("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove MyApp.csproj package TestPackage");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCorrectCommand()
    {
      string command = DotNet.RemovePackage("Microsoft.Extensions.Logging")
        .WithProject("test.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove test.csproj package Microsoft.Extensions.Logging");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectory_Should_NotAppearInCommandString()
    {
      string command = DotNet.RemovePackage("Newtonsoft.Json")
        .WithProject("test.csproj")
        .WithWorkingDirectory("/tmp")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove test.csproj package Newtonsoft.Json");

      await Task.CompletedTask;
    }

    public static async Task EnvironmentVariables_Should_NotAppearInCommandString()
    {
      string command = DotNet.RemovePackage("TestPackage")
        .WithProject("test.csproj")
        .WithEnvironmentVariable("NUGET_ENV", "test")
        .WithEnvironmentVariable("REMOVE_PACKAGE_LOG", "verbose")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove test.csproj package TestPackage");

      await Task.CompletedTask;
    }

    public static async Task ValidPackageName_Should_BuildCorrectCommand()
    {
      string command = DotNet.RemovePackage("Valid.Package.Name")
        .WithProject("MyProject.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove MyProject.csproj package Valid.Package.Name");

      await Task.CompletedTask;
    }

    public static async Task MultipleConfiguration_Should_BuildCorrectCommand()
    {
      string command = DotNet.RemovePackage("ChainedPackage")
        .WithProject("test.csproj")
        .WithWorkingDirectory("/project")
        .WithEnvironmentVariable("BUILD_ENV", "test")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove test.csproj package ChainedPackage");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.RemovePackage("TestPackage")
        .WithProject("nonexistent.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet remove nonexistent.csproj package TestPackage");

      await Task.CompletedTask;
    }
  }
}
