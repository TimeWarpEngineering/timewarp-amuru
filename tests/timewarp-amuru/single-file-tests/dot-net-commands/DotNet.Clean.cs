#!/usr/bin/dotnet --

await RunTests<DotNetCleanCommandTests>();

internal sealed class DotNetCleanCommandTests
{
  public static async Task TestBasicDotNetCleanCommand()
  {
    string command = DotNet.Clean()
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean");

    await Task.CompletedTask;
  }
  
  public static async Task TestCleanWithProjectOnly()
  {
    string command = DotNet.Clean()
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean MyApp.csproj");

    await Task.CompletedTask;
  }

  public static async Task TestCleanFluentConfigurationMethods()
  {
    string command = DotNet.Clean()
      .WithProject("test.csproj")
      .WithConfiguration("Debug")
      .WithFramework("net10.0")
      .WithOutput("bin/Debug")
      .WithVerbosity("minimal")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean test.csproj --configuration Debug --framework net10.0 --output bin/Debug --verbosity minimal");

    await Task.CompletedTask;
  }

  public static async Task TestCleanMethodChainingWithRuntimeAndProperties()
  {
    string command = DotNet.Clean()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithRuntime("linux-x64")
      .WithNoLogo()
      .WithProperty("Platform", "AnyCPU")
      .WithProperty("CleanTargets", "All")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean test.csproj --configuration Release --runtime linux-x64 --nologo --property:Platform=AnyCPU --property:CleanTargets=All");

    await Task.CompletedTask;
  }

  public static async Task TestCleanWithWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Clean()
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("CLEAN_ENV", "test")
      .WithVerbosity("quiet")
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean test.csproj --verbosity quiet");

    await Task.CompletedTask;
  }

  public static async Task TestCleanOverloadWithProjectParameter()
  {
    string command = DotNet.Clean("test.csproj")
      .WithConfiguration("Debug")
      .WithNoLogo()
      .Build()
      .ToCommandString();

    command.ShouldBe("dotnet clean test.csproj --configuration Debug --nologo");

    await Task.CompletedTask;
  }

  public static async Task TestCleanCommandExecutionGracefulHandling()
  {
    // Test that the builder creates a valid command even for non-existent projects
    string command = DotNet.Clean()
      .WithProject("nonexistent.csproj")
      .WithConfiguration("Debug")
      .Build()
      .ToCommandString();

    // The command string should be created correctly
    command.ShouldBe("dotnet clean nonexistent.csproj --configuration Debug");

    await Task.CompletedTask;
  }
}