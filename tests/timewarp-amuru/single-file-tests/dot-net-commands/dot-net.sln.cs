#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Sln() - validates the fluent API for managing solution files
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Sln (the method being tested)
// Tests verify command string generation for add, remove, list, and migrate operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Sln_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Sln_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetSlnBuilder builder = DotNet.Sln();

      builder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task SolutionParameter_Should_CreateBuilderWithSolution()
    {
      DotNetSlnBuilder builder = DotNet.Sln("MySolution.sln");

      builder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Add_Should_BuildAddCommand()
    {
      string command = DotNet.Sln("MySolution.sln")
        .Add("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln MySolution.sln add MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task AddMultiple_Should_IncludeAllProjects()
    {
      string command = DotNet.Sln("MySolution.sln")
        .Add("MyApp.csproj", "MyLibrary.csproj", "MyTests.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln MySolution.sln add MyApp.csproj MyLibrary.csproj MyTests.csproj");

      await Task.CompletedTask;
    }

    public static async Task List_Should_BuildListCommand()
    {
      string command = DotNet.Sln("MySolution.sln")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln MySolution.sln list");

      await Task.CompletedTask;
    }

    public static async Task Remove_Should_BuildRemoveCommand()
    {
      string command = DotNet.Sln("MySolution.sln")
        .Remove("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln MySolution.sln remove MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task Migrate_Should_BuildMigrateCommand()
    {
      string command = DotNet.Sln("MySolution.sln")
        .Migrate()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln MySolution.sln migrate");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Sln()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_ENV", "test")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln list");

      await Task.CompletedTask;
    }

    public static async Task NonExistentSolution_Should_BuildValidCommand()
    {
      string command = DotNet.Sln("nonexistent.sln")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet sln nonexistent.sln list");

      await Task.CompletedTask;
    }
  }
}
