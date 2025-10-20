#!/usr/bin/dotnet --

await RunTests<DotNetFluentApiTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetFluentApiTests

{
  public static async Task TestBasicDotNetRunBuilderCreation()
  {
    string command = DotNet.Run()
      .Build()
      .ToCommandString();
      
    command.ShouldBe("dotnet run");
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
  {
    string command = DotNet.Run()
      .WithProject("test.csproj")
      .WithConfiguration("Debug")
      .WithFramework("net10.0")
      .WithNoRestore()
      .WithArguments("--help")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet run --project test.csproj --configuration Debug --framework net10.0 --no-restore -- --help");
    
    await Task.CompletedTask;
  }

  public static async Task TestMethodChaining()
  {
    string command = DotNet.Run()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithNoRestore()
      .WithNoBuild()
      .WithVerbosity("minimal")
      .WithArguments("arg1", "arg2")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet run --project test.csproj --configuration Release --verbosity minimal --no-restore --no-build -- arg1 arg2");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Run()
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet run --project test.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestExtendedOptions()
  {
    string command = DotNet.Run()
      .WithProject("test.csproj")
      .WithArchitecture("x64")
      .WithOperatingSystem("linux")
      .WithLaunchProfile("Development")
      .WithForce()
      .WithInteractive()
      .WithTerminalLogger("auto")
      .WithProperty("Configuration", "Debug")
      .WithProperty("Platform", "AnyCPU")
      .WithProcessEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
      .WithNoLaunchProfile()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet run --project test.csproj --arch x64 --os linux --launch-profile Development --tl auto --no-launch-profile --force --interactive --property:Configuration=Debug --property:Platform=AnyCPU -e ASPNETCORE_ENVIRONMENT=Development");
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandExecutionWithGracefulHandling()
  {
    // Test command string generation for non-existent project
    string command = DotNet.Run()
      .WithProject("nonexistent.csproj")
      .WithConfiguration("Debug")
      .WithNoRestore()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet run --project nonexistent.csproj --configuration Debug --no-restore");
    
    await Task.CompletedTask;
  }
}