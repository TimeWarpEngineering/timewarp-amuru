#!/usr/bin/dotnet run

await RunTests<DotNetBuildCommandTests>();

internal sealed class DotNetBuildCommandTests
{
  public static async Task TestBasicDotNetBuildCommand()
  {
    string command = DotNet.Build()
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build");

    await Task.CompletedTask;
  }
  
  public static async Task TestBuildWithProjectOnly()
  {
    string command = DotNet.Build()
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build MyApp.csproj");

    await Task.CompletedTask;
  }

  public static async Task TestBuildFluentConfigurationMethods()
  {
    string command = DotNet.Build()
      .WithProject("test.csproj")
      .WithConfiguration("Debug")
      .WithFramework("net10.0")
      .WithNoRestore()
      .WithOutput("bin/Debug")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build test.csproj --configuration Debug --framework net10.0 --output bin/Debug --no-restore");

    await Task.CompletedTask;
  }

  public static async Task TestBuildMethodChainingWithAdvancedOptions()
  {
    string command = DotNet.Build()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithArchitecture("x64")
      .WithOperatingSystem("linux")
      .WithNoRestore()
      .WithNoDependencies()
      .WithNoIncremental()
      .WithVerbosity("minimal")
      .WithProperty("Platform", "AnyCPU")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build test.csproj --configuration Release --arch x64 --os linux --verbosity minimal --no-restore --no-dependencies --no-incremental --property:Platform=AnyCPU");

    await Task.CompletedTask;
  }

  public static async Task TestBuildWithWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    // They are process execution settings, not command arguments
    string command = DotNet.Build()
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("BUILD_ENV", "test")
      .WithNoLogo()
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build test.csproj --nologo");

    await Task.CompletedTask;
  }

  public static async Task TestBuildWithMSBuildPropertiesIncludingNoCacheOptions()
  {
    string command = DotNet.Build()
      .WithProject("test.csproj")
      .WithConfiguration("Debug")
      .WithProperty("RestoreNoCache", "true")
      .WithProperty("DisableImplicitNuGetFallbackFolder", "true")
      .WithProperty("RestoreIgnoreFailedSources", "true")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build test.csproj --configuration Debug --property:RestoreNoCache=true --property:DisableImplicitNuGetFallbackFolder=true --property:RestoreIgnoreFailedSources=true");

    await Task.CompletedTask;
  }

  public static async Task TestBuildOverloadWithProjectParameter()
  {
    string command = DotNet.Build("test.csproj")
      .WithConfiguration("Debug")
      .WithNoRestore()
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet build test.csproj --configuration Debug --no-restore");

    await Task.CompletedTask;
  }

  public static async Task TestBuildCommandExecutionGracefulHandling()
  {
    // Test that the builder creates a valid command even for non-existent projects
    string commandString = DotNet.Build()
      .WithProject("nonexistent.csproj")
      .WithConfiguration("Debug")
      .WithNoRestore()
      .Build()
      .ToCommandString();

    // The command string should be created correctly
    commandString.ShouldBe("dotnet build nonexistent.csproj --configuration Debug --no-restore");

    await Task.CompletedTask;
  }
}