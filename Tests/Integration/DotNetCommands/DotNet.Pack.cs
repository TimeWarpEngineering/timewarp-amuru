#!/usr/bin/dotnet run

await RunTests<DotNetPackTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetPackTests
{
  public static async Task TestBasicPackCommand()
  {
    string command = DotNet.Pack()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack",
      $"Expected 'dotnet pack', got '{command}'"
    );
    
    await Task.CompletedTask;
  }
  
  public static async Task TestPackWithProjectOnly()
  {
    string command = DotNet.Pack()
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack MyApp.csproj",
      $"Expected 'dotnet pack MyApp.csproj', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
  {
    string command = DotNet.Pack()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithFramework("net10.0")
      .WithRuntime("win-x64")
      .WithOutput("./packages")
      .WithNoRestore()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack test.csproj --configuration Release --framework net10.0 --runtime win-x64 --output ./packages --no-restore",
      $"Expected correct pack command with configuration options, got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestPackageSpecificOptions()
  {
    string command = DotNet.Pack()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithVersionSuffix("beta")
      .IncludeSymbols()
      .IncludeSource()
      .WithServiceable()
      .WithNoLogo()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack test.csproj --configuration Release --version-suffix beta --nologo --include-symbols --include-source --serviceable",
      $"Expected correct pack command with package-specific options, got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Pack()
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("PACK_ENV", "production")
      .WithVerbosity("detailed")
      .WithTerminalLogger("on")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack test.csproj --verbosity detailed --tl on",
      $"Expected 'dotnet pack test.csproj --verbosity detailed --tl on', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestMSBuildPropertiesAndSources()
  {
    string command = DotNet.Pack()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithProperty("PackageVersion", "1.0.0")
      .WithProperty("PackageDescription", "Test package")
      .WithSource("https://api.nuget.org/v3/index.json")
      .WithNoBuild()
      .WithForce()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack test.csproj --configuration Release --source https://api.nuget.org/v3/index.json --no-build --force --property:PackageVersion=1.0.0 \"--property:PackageDescription=Test package\"",
      $"Expected correct pack command with MSBuild properties and sources, got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestPackOverloadWithProjectParameter()
  {
    string command = DotNet.Pack("test.csproj")
      .WithConfiguration("Release")
      .WithOutput("./dist")
      .WithVersionSuffix("rc1")
      .WithNoDependencies()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack test.csproj --configuration Release --output ./dist --version-suffix rc1 --no-dependencies",
      $"Expected correct pack command with overload, got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandBuilderWithNonExistentProject()
  {
    // Verify that the command builder creates a valid command even with non-existent project
    string command = DotNet.Pack()
      .WithProject("nonexistent.csproj")
      .WithConfiguration("Release")
      .WithOutput("./packages")
      .WithNoRestore()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "dotnet pack nonexistent.csproj --configuration Release --output ./packages --no-restore",
      $"Expected correct command string even for non-existent projects, got '{command}'"
    );
    
    await Task.CompletedTask;
  }
}