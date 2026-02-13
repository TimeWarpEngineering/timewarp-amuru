#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Reference() - validates the fluent API for managing project references
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Reference (the method being tested)
// Tests verify command string generation for add, remove, and list reference operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Reference_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Reference_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetReferenceBuilder builder = DotNet.Reference();

      builder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task ProjectParameter_Should_CreateBuilderWithProject()
    {
      DotNetReferenceBuilder builder = DotNet.Reference("MyApp.csproj");

      builder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Add_Should_BuildAddCommand()
    {
      string command = DotNet.Reference("MyApp.csproj")
        .Add("MyLibrary.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference --project MyApp.csproj add MyLibrary.csproj");

      await Task.CompletedTask;
    }

    public static async Task AddMultiple_Should_IncludeAllProjects()
    {
      string command = DotNet.Reference("MyApp.csproj")
        .Add("MyLibrary.csproj", "MyOtherLibrary.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference --project MyApp.csproj add MyLibrary.csproj MyOtherLibrary.csproj");

      await Task.CompletedTask;
    }

    public static async Task List_Should_BuildListCommand()
    {
      string command = DotNet.Reference("MyApp.csproj")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference --project MyApp.csproj list");

      await Task.CompletedTask;
    }

    public static async Task Remove_Should_BuildRemoveCommand()
    {
      string command = DotNet.Reference("MyApp.csproj")
        .Remove("MyLibrary.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference --project MyApp.csproj remove MyLibrary.csproj");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Reference()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_ENV", "test")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference list");

      await Task.CompletedTask;
    }

    public static async Task NonExistentProject_Should_BuildValidCommand()
    {
      string command = DotNet.Reference("nonexistent.csproj")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet reference --project nonexistent.csproj list");

      await Task.CompletedTask;
    }
  }
}
