#!/usr/bin/dotnet --

await RunTests<DotNetPackTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetPackTests
{
  public static async Task TestBasicPackCommand()
  {
    string command = DotNet.Pack()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet pack");
    
    await Task.CompletedTask;
  }
  
  public static async Task TestPackWithProjectOnly()
  {
    string command = DotNet.Pack()
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet pack MyApp.csproj");
    
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
    
    command.ShouldBe("dotnet pack test.csproj --configuration Release --framework net10.0 --runtime win-x64 --output ./packages --no-restore");
    
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
    
    command.ShouldBe("dotnet pack test.csproj --configuration Release --version-suffix beta --nologo --include-symbols --include-source --serviceable");
    
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
    
    command.ShouldBe("dotnet pack test.csproj --verbosity detailed --tl on");
    
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

    command.ShouldBe("dotnet pack test.csproj --configuration Release --source https://api.nuget.org/v3/index.json --no-build --force --property:PackageVersion=1.0.0 \"--property:PackageDescription=Test package\"");

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
    
    command.ShouldBe("dotnet pack test.csproj --configuration Release --output ./dist --version-suffix rc1 --no-dependencies");
    
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
    
    command.ShouldBe("dotnet pack nonexistent.csproj --configuration Release --output ./packages --no-restore");
    
    await Task.CompletedTask;
  }
}