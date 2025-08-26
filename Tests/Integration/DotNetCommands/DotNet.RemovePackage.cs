#!/usr/bin/dotnet run

await RunTests<DotNetRemovePackageTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetRemovePackageTests
{
  public static async Task TestBasicRemovePackageCommand()
  {
    string command = DotNet.RemovePackage("TestPackage")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove package TestPackage",
      $"Expected 'dotnet remove package TestPackage', got '{command}'"
    );
    
    await Task.CompletedTask;
  }
  
  public static async Task TestRemovePackageWithProject()
  {
    string command = DotNet.RemovePackage("TestPackage")
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove MyApp.csproj package TestPackage",
      $"Expected 'dotnet remove MyApp.csproj package TestPackage', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
  {
    string command = DotNet.RemovePackage("Microsoft.Extensions.Logging")
      .WithProject("test.csproj")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove test.csproj package Microsoft.Extensions.Logging",
      $"Expected 'dotnet remove test.csproj package Microsoft.Extensions.Logging', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryConfiguration()
  {
    // Note: Working directory doesn't appear in ToCommandString()
    string command = DotNet.RemovePackage("Newtonsoft.Json")
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove test.csproj package Newtonsoft.Json",
      $"Expected 'dotnet remove test.csproj package Newtonsoft.Json', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestEnvironmentVariables()
  {
    // Note: Environment variables don't appear in ToCommandString()
    string command = DotNet.RemovePackage("TestPackage")
      .WithProject("test.csproj")
      .WithEnvironmentVariable("NUGET_ENV", "test")
      .WithEnvironmentVariable("REMOVE_PACKAGE_LOG", "verbose")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove test.csproj package TestPackage",
      $"Expected 'dotnet remove test.csproj package TestPackage', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestPackageNameValidation()
  {
    string command = DotNet.RemovePackage("Valid.Package.Name")
      .WithProject("MyProject.csproj")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove MyProject.csproj package Valid.Package.Name",
      $"Expected 'dotnet remove MyProject.csproj package Valid.Package.Name', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestMultipleConfigurationChaining()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.RemovePackage("ChainedPackage")
      .WithProject("test.csproj")
      .WithWorkingDirectory("/project")
      .WithEnvironmentVariable("BUILD_ENV", "test")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove test.csproj package ChainedPackage",
      $"Expected 'dotnet remove test.csproj package ChainedPackage', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandExecutionGracefulHandling()
  {
    // Test that command string is built correctly even for non-existent project
    string command = DotNet.RemovePackage("TestPackage")
      .WithProject("nonexistent.csproj")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet remove nonexistent.csproj package TestPackage",
      $"Expected correct command string even for non-existent projects, got '{command}'"
    );
    
    // With WithNoValidation(), CaptureAsync returns output on failure
    CommandOutput output = await DotNet.RemovePackage("TestPackage")
      .WithProject("nonexistent.csproj")
      .WithNoValidation()
      .Build()
      .CaptureAsync();
    
    AssertTrue(
      output != null,
      "CaptureAsync with WithNoValidation() should return non-null even for failed commands"
    );
    
    await Task.CompletedTask;
  }
}