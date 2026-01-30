#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.UserSecrets() - validates the fluent API for managing user secrets
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: UserSecrets (the method being tested)
// Tests verify command string generation for init, set, remove, list, and clear operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class UserSecrets_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<UserSecrets_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetUserSecretsBuilder userSecretsBuilder = DotNet.UserSecrets();

      userSecretsBuilder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Init_Should_BuildInitCommand()
    {
      string command = DotNet.UserSecrets()
        .Init()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets init");

      await Task.CompletedTask;
    }

    public static async Task InitWithProject_Should_IncludeProjectOption()
    {
      string command = DotNet.UserSecrets()
        .WithProject("MyApp.csproj")
        .Init()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets init --project MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task InitWithId_Should_IncludeIdOption()
    {
      string command = DotNet.UserSecrets()
        .WithId("my-app-secrets")
        .Init()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets init --id my-app-secrets");

      await Task.CompletedTask;
    }

    public static async Task Set_Should_BuildSetCommand()
    {
      string command = DotNet.UserSecrets()
        .Set("ConnectionString", "Server=localhost;Database=MyApp;")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets set ConnectionString Server=localhost;Database=MyApp;");

      await Task.CompletedTask;
    }

    public static async Task SetWithProject_Should_IncludeProjectOption()
    {
      string command = DotNet.UserSecrets()
        .WithProject("MyApp.csproj")
        .Set("ApiKey", "secret-key-value")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets set ApiKey secret-key-value --project MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task Remove_Should_BuildRemoveCommand()
    {
      string command = DotNet.UserSecrets()
        .Remove("ConnectionString")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets remove ConnectionString");

      await Task.CompletedTask;
    }

    public static async Task List_Should_BuildListCommand()
    {
      string command = DotNet.UserSecrets()
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets list");

      await Task.CompletedTask;
    }

    public static async Task Clear_Should_BuildClearCommand()
    {
      string command = DotNet.UserSecrets()
        .Clear()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets clear");

      await Task.CompletedTask;
    }

    public static async Task AllOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.UserSecrets()
        .WithProject("MyApp.csproj")
        .WithId("my-app-secrets")
        .Set("DatabaseConnection", "Server=localhost;")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets set DatabaseConnection Server=localhost; --project MyApp.csproj --id my-app-secrets");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.UserSecrets()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_ENV", "test")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets list");

      await Task.CompletedTask;
    }

    public static async Task ListWithProject_Should_IncludeProjectOption()
    {
      string command = DotNet.UserSecrets()
        .WithProject("test.csproj")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet user-secrets list --project test.csproj");

      await Task.CompletedTask;
    }
  }
}
