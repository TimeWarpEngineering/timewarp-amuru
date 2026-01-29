#!/usr/bin/dotnet --

await RunTests<DotNetReferenceTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
internal sealed class DotNetReferenceTests
{
  public static async Task TestBasicReferenceBuilderCreation()
  {
    // DotNet.Reference() alone doesn't build a valid command - needs a subcommand
    DotNetReferenceBuilder builder = DotNet.Reference();
    
    builder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task TestReferenceWithProjectParameter()
  {
    // Test that we can create a builder with project file
    DotNetReferenceBuilder builder = DotNet.Reference("MyApp.csproj");
    
    builder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task TestReferenceAddCommand()
  {
    string command = DotNet.Reference("MyApp.csproj")
      .Add("MyLibrary.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference --project MyApp.csproj add MyLibrary.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestReferenceAddWithMultipleProjects()
  {
    string command = DotNet.Reference("MyApp.csproj")
      .Add("MyLibrary.csproj", "MyOtherLibrary.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference --project MyApp.csproj add MyLibrary.csproj MyOtherLibrary.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestReferenceListCommand()
  {
    string command = DotNet.Reference("MyApp.csproj")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference --project MyApp.csproj list");
    
    await Task.CompletedTask;
  }

  public static async Task TestReferenceRemoveCommand()
  {
    string command = DotNet.Reference("MyApp.csproj")
      .Remove("MyLibrary.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference --project MyApp.csproj remove MyLibrary.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Reference()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("DOTNET_ENV", "test")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference list");
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandExecutionGracefulHandling()
  {
    // Test that command string is built correctly even for non-existent project
    string command = DotNet.Reference("nonexistent.csproj")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet reference --project nonexistent.csproj list");
    
    await Task.CompletedTask;
  }
}