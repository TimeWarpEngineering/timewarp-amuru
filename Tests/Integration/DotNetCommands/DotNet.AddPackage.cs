#!/usr/bin/dotnet run

await RunTests<DotNetAddPackageTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetAddPackageTests
{
  public static async Task TestBasicAddPackageCommand()
  {
    string command = DotNet.AddPackage("TestPackage")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet add package TestPackage");

    await Task.CompletedTask;
  }
  
  public static async Task TestAddPackageWithProject()
  {
    string command = DotNet.AddPackage("TestPackage")
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet add MyApp.csproj package TestPackage");

    await Task.CompletedTask;
  }

  public static async Task TestAddPackageWithVersionOverload()
  {
    string command = DotNet.AddPackage("TestPackage", "1.0.0")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet add package TestPackage --version 1.0.0");

    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
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

  public static async Task TestPackageSpecificOptions()
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

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
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

  public static async Task TestMultipleSourcesConfiguration()
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

  public static async Task TestCommandExecutionGracefulHandling()
  {
    // Test that command string is built correctly even for non-existent project
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