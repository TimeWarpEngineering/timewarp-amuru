#!/usr/bin/dotnet --

await RunTests<DotNetSlnCommandTests>();

internal sealed class DotNetSlnCommandTests
{
  public static async Task BasicDotNetSlnCommand()
  {
    // DotNet.Sln() alone doesn't build a valid command - needs a subcommand
    // This test verifies the builder is created
    DotNetSlnBuilder builder = DotNet.Sln();
    
    builder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task DotNetSlnWithSolutionFileParameter()
  {
    // Test that we can create a builder with solution file
    DotNetSlnBuilder builder = DotNet.Sln("MySolution.sln");
    
    builder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task SolutionAddCommand()
  {
    string command = DotNet.Sln("MySolution.sln")
      .Add("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln MySolution.sln add MyApp.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task SolutionAddWithMultipleProjects()
  {
    string command = DotNet.Sln("MySolution.sln")
      .Add("MyApp.csproj", "MyLibrary.csproj", "MyTests.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln MySolution.sln add MyApp.csproj MyLibrary.csproj MyTests.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task SolutionListCommand()
  {
    string command = DotNet.Sln("MySolution.sln")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln MySolution.sln list");
    
    await Task.CompletedTask;
  }

  public static async Task SolutionRemoveCommand()
  {
    string command = DotNet.Sln("MySolution.sln")
      .Remove("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln MySolution.sln remove MyApp.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task SolutionMigrateCommand()
  {
    string command = DotNet.Sln("MySolution.sln")
      .Migrate()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln MySolution.sln migrate");
    
    await Task.CompletedTask;
  }

  public static async Task WorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Sln()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("DOTNET_ENV", "test")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln list");
    
    await Task.CompletedTask;
  }

  public static async Task CommandBuilderWithNonExistentSolution()
  {
    // Verify that the command builder creates a valid command even with non-existent solution
    string command = DotNet.Sln("nonexistent.sln")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet sln nonexistent.sln list");
    
    await Task.CompletedTask;
  }
}