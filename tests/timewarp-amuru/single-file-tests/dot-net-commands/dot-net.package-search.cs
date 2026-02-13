#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.PackageSearch() - validates the fluent API for searching NuGet packages
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: PackageSearch (the method being tested)
// Tests verify command string generation for package search options
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class PackageSearch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<PackageSearch_Given_>();

    public static async Task SearchTerm_Should_BuildBasicCommand()
    {
      string command = DotNet.PackageSearch("TimeWarp.Cli")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search TimeWarp.Cli");

      await Task.CompletedTask;
    }

    public static async Task NoSearchTerm_Should_BuildEmptySearchCommand()
    {
      string command = DotNet.PackageSearch()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.PackageSearch("Microsoft.Extensions.Logging")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithTake(5)
        .WithSkip(0)
        .WithFormat("table")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search Microsoft.Extensions.Logging --source https://api.nuget.org/v3/index.json --take 5 --skip 0 --format table");

      await Task.CompletedTask;
    }

    public static async Task AdvancedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.PackageSearch("Newtonsoft.Json")
        .WithExactMatch()
        .WithPrerelease()
        .WithFormat("json")
        .WithVerbosity("detailed")
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search Newtonsoft.Json --format json --verbosity detailed --exact-match --interactive --prerelease");

      await Task.CompletedTask;
    }

    public static async Task MultipleSources_Should_IncludeAllSources()
    {
      string command = DotNet.PackageSearch("TestPackage")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithSource("https://pkgs.dev.azure.com/example/feed")
        .WithTake(10)
        .WithConfigFile("nuget.config")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search TestPackage --source https://api.nuget.org/v3/index.json --source https://pkgs.dev.azure.com/example/feed --take 10 --configfile nuget.config");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.PackageSearch("TestPackage")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("NUGET_ENV", "test")
        .WithTake(1)
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search TestPackage --take 1");

      await Task.CompletedTask;
    }

    public static async Task WellKnownPackage_Should_BuildCorrectCommand()
    {
      string command = DotNet.PackageSearch("Microsoft.Extensions.Logging")
        .WithTake(1)
        .WithFormat("table")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search Microsoft.Extensions.Logging --take 1 --format table");

      await Task.CompletedTask;
    }

    public static async Task ExactMatchWithPrerelease_Should_IncludeBothOptions()
    {
      string command = DotNet.PackageSearch("TimeWarp.Cli")
        .WithExactMatch()
        .WithPrerelease()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet package search TimeWarp.Cli --exact-match --prerelease");

      await Task.CompletedTask;
    }
  }
}
