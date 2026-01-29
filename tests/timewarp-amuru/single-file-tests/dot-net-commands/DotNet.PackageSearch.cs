#!/usr/bin/dotnet --

await RunTests<DotNetPackageSearchTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetPackageSearchTests

{
  public static async Task BasicDotNetPackageSearchCommand()
  {
    string command = DotNet.PackageSearch("TimeWarp.Cli")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet package search TimeWarp.Cli");
    
    await Task.CompletedTask;
  }

  public static async Task DotNetPackageSearchWithoutSearchTerm()
  {
    string command = DotNet.PackageSearch()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet package search");
    
    await Task.CompletedTask;
  }

  public static async Task FluentConfigurationMethods()
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

  public static async Task AdvancedSearchOptions()
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

  public static async Task MultipleSourcesConfiguration()
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

  public static async Task WorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.PackageSearch("TestPackage")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("NUGET_ENV", "test")
      .WithTake(1)
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet package search TestPackage --take 1");
    
    await Task.CompletedTask;
  }

  public static async Task CommandExecutionSearchWellKnownPackage()
  {
    // Test command string generation for well-known package search
    string command = DotNet.PackageSearch("Microsoft.Extensions.Logging")
      .WithTake(1)
      .WithFormat("table")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet package search Microsoft.Extensions.Logging --take 1 --format table");
    
    await Task.CompletedTask;
  }

  public static async Task ExactMatchSearch()
  {
    // Test command string for exact match search
    string command = DotNet.PackageSearch("TimeWarp.Cli")
      .WithExactMatch()
      .WithPrerelease()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet package search TimeWarp.Cli --exact-match --prerelease");
    
    await Task.CompletedTask;
  }
}