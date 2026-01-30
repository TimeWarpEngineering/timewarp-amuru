#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.New() - validates the fluent API for creating new .NET projects and items
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: New (the method being tested)
// Tests verify command string generation for project and item creation
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class New_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<New_Given_>();

    public static async Task TemplateName_Should_BuildBasicCommand()
    {
      string command = DotNet.New("console")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new console");

      await Task.CompletedTask;
    }

    public static async Task NoTemplateName_Should_BuildEmptyNewCommand()
    {
      string command = DotNet.New()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new");

      await Task.CompletedTask;
    }

    public static async Task FluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.New("console")
        .WithName("TestApp")
        .WithOutput("./test-output")
        .WithForce()
        .WithDryRun()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new console --output ./test-output --name TestApp --dry-run --force");

      await Task.CompletedTask;
    }

    public static async Task TemplateArgsAndAdvancedOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.New("web")
        .WithName("MyWebApp")
        .WithOutput("./web-output")
        .WithTemplateArg("--framework")
        .WithTemplateArg("net10.0")
        .WithVerbosity("detailed")
        .WithNoUpdateCheck()
        .WithDiagnostics()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new web --framework net10.0 --output ./web-output --name MyWebApp --verbosity detailed --no-update-check --diagnostics");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.New("classlib")
        .WithName("MyLibrary")
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("TEMPLATE_ENV", "test")
        .WithProject("test.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new classlib --name MyLibrary --project test.csproj");

      await Task.CompletedTask;
    }

    public static async Task ListSubcommand_Should_BuildListCommand()
    {
      string command = DotNet.New().List("console")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new list console");

      await Task.CompletedTask;
    }

    public static async Task SearchSubcommand_Should_BuildSearchCommand()
    {
      string command = DotNet.New().Search("blazor")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new search blazor");

      await Task.CompletedTask;
    }

    public static async Task DryRun_Should_IncludeDryRunOption()
    {
      string command = DotNet.New("console")
        .WithName("TestConsoleApp")
        .WithOutput("./dry-run-test")
        .WithDryRun()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet new console --output ./dry-run-test --name TestConsoleApp --dry-run");

      await Task.CompletedTask;
    }
  }
}
