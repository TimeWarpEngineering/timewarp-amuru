#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.ListPackages() - validates the fluent API for listing NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: ListPackages (the method being tested)
// Tests verify command string generation for package listing options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class ListPackages_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ListPackages_Given_>();

    public static async Task NoOptions_Should_BuildBasicCommand()
    {
      string command = DotNet.ListPackages()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.ListPackages()
        .WithProject("test.csproj")
        .WithFramework("net10.0")
        .WithVerbosity("minimal")
        .WithFormat("console")
        .Outdated()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package test.csproj --framework net10.0 --verbosity minimal --format console --outdated");

      await Task.CompletedTask;
    }

    public static async Task TransitiveAndVulnerable_Should_IncludeAllOptions()
    {
      string command = DotNet.ListPackages()
        .WithProject("test.csproj")
        .IncludeTransitive()
        .Vulnerable()
        .Deprecated()
        .WithInteractive()
        .WithSource("https://api.nuget.org/v3/index.json")
        .IncludePrerelease()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package test.csproj --source https://api.nuget.org/v3/index.json --include-transitive --vulnerable --deprecated --interactive --include-prerelease");

      await Task.CompletedTask;
    }

    public static async Task JsonFormatAndHighestVersion_Should_IncludeOptions()
    {
      string command = DotNet.ListPackages()
        .WithProject("test.csproj")
        .WithFormat("json")
        .WithOutputVersion("1")
        .WithConfig("nuget.config")
        .Outdated()
        .HighestMinor()
        .HighestPatch()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("NUGET_PACKAGES", "./temp-packages")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package test.csproj --format json --output-version 1 --config nuget.config --outdated --highest-minor --highest-patch");

      await Task.CompletedTask;
    }

    public static async Task ProjectOverload_Should_SetProject()
    {
      string command = DotNet.ListPackages("test.csproj")
        .WithFramework("net8.0")
        .IncludeTransitive()
        .WithVerbosity("quiet")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package test.csproj --framework net8.0 --verbosity quiet --include-transitive");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.ListPackages()
        .WithProject("nonexistent.csproj")
        .IncludeTransitive()
        .Outdated()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package nonexistent.csproj --outdated --include-transitive");

      await Task.CompletedTask;
    }

    public static async Task MultipleSources_Should_IncludeAllSources()
    {
      string command = DotNet.ListPackages()
        .WithProject("test.csproj")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithSource("https://myget.org/F/myfeed/api/v3/index.json")
        .Outdated()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet list package test.csproj --source https://api.nuget.org/v3/index.json --source https://myget.org/F/myfeed/api/v3/index.json --outdated");

      await Task.CompletedTask;
    }
  }
}
