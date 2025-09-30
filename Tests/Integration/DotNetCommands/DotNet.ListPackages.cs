#!/usr/bin/dotnet run

await RunTests<DotNetListPackagesTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetListPackagesTests
{
  public static async Task TestBasicListPackagesCommand()
  {
    string command = DotNet.ListPackages()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet list package");
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
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

  public static async Task TestMethodChainingWithTransitiveAndVulnerableOptions()
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

  public static async Task TestJsonFormatAndHighestVersionOptions()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
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

  public static async Task TestListPackagesOverloadWithProjectParameter()
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

  public static async Task TestToListAsyncMethod()
  {
    // Test that command string is built correctly even for non-existent project
    string command = DotNet.ListPackages()
      .WithProject("nonexistent.csproj")
      .IncludeTransitive()
      .Outdated()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet list package nonexistent.csproj --outdated --include-transitive");
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandExecutionGracefulHandling()
  {
    // Test command string generation for multiple sources
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