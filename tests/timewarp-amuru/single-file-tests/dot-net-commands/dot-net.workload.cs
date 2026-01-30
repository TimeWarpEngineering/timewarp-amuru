#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.Workload() - validates the fluent API for managing .NET workloads
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Workload (the method being tested)
// Tests verify command string generation for install, uninstall, update, repair, list, search, and config operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Workload_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Workload_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetWorkloadBuilder workloadBuilder = DotNet.Workload();

      workloadBuilder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Info_Should_BuildInfoCommand()
    {
      string command = DotNet.Workload()
        .Info()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload --info");

      await Task.CompletedTask;
    }

    public static async Task Version_Should_BuildVersionCommand()
    {
      string command = DotNet.Workload()
        .Version()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload --version");

      await Task.CompletedTask;
    }

    public static async Task Install_Should_BuildInstallCommand()
    {
      string command = DotNet.Workload()
        .Install("maui")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload install maui");

      await Task.CompletedTask;
    }

    public static async Task InstallMultiple_Should_IncludeAllWorkloads()
    {
      string command = DotNet.Workload()
        .Install("maui", "android", "ios")
        .WithConfigFile("nuget.config")
        .WithIncludePreview()
        .WithSkipManifestUpdate()
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithVersion("8.0.100")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload install maui android ios --configfile nuget.config --include-previews --skip-manifest-update --source https://api.nuget.org/v3/index.json --version 8.0.100");

      await Task.CompletedTask;
    }

    public static async Task List_Should_BuildListCommand()
    {
      string command = DotNet.Workload()
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload list");

      await Task.CompletedTask;
    }

    public static async Task ListWithVerbosity_Should_IncludeVerbosityOption()
    {
      string command = DotNet.Workload()
        .List()
        .WithVerbosity("detailed")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload list --verbosity detailed");

      await Task.CompletedTask;
    }

    public static async Task Search_Should_BuildSearchCommand()
    {
      string command = DotNet.Workload()
        .Search()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload search");

      await Task.CompletedTask;
    }

    public static async Task SearchWithTerm_Should_IncludeSearchTerm()
    {
      string command = DotNet.Workload()
        .Search("maui")
        .WithVerbosity("minimal")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload search maui --verbosity minimal");

      await Task.CompletedTask;
    }

    public static async Task Uninstall_Should_BuildUninstallCommand()
    {
      string command = DotNet.Workload()
        .Uninstall("maui")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload uninstall maui");

      await Task.CompletedTask;
    }

    public static async Task UninstallMultiple_Should_IncludeAllWorkloads()
    {
      string command = DotNet.Workload()
        .Uninstall("maui", "android", "ios")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload uninstall maui android ios");

      await Task.CompletedTask;
    }

    public static async Task Update_Should_BuildUpdateCommand()
    {
      string command = DotNet.Workload()
        .Update()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload update");

      await Task.CompletedTask;
    }

    public static async Task UpdateWithComprehensiveOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Workload()
        .Update()
        .WithAdvertisingManifestsOnly()
        .WithConfigFile("nuget.config")
        .WithDisableParallel()
        .WithFromPreviousSdk()
        .WithIncludePreview()
        .WithInteractive()
        .WithNoCache()
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithTempDir("/tmp")
        .WithVerbosity("diagnostic")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload update --advertising-manifests-only --configfile nuget.config --disable-parallel --from-previous-sdk --include-previews --interactive --no-cache --source https://api.nuget.org/v3/index.json --temp-dir /tmp --verbosity diagnostic");

      await Task.CompletedTask;
    }

    public static async Task Repair_Should_BuildRepairCommand()
    {
      string command = DotNet.Workload()
        .Repair()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload repair");

      await Task.CompletedTask;
    }

    public static async Task RepairWithComprehensiveOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Workload()
        .Repair()
        .WithConfigFile("nuget.config")
        .WithDisableParallel()
        .WithIgnoreFailedSources()
        .WithInteractive()
        .WithNoCache()
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithTempDir("/tmp")
        .WithVerbosity("detailed")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload repair --configfile nuget.config --disable-parallel --ignore-failed-sources --interactive --no-cache --source https://api.nuget.org/v3/index.json --temp-dir /tmp --verbosity detailed");

      await Task.CompletedTask;
    }

    public static async Task Clean_Should_BuildCleanCommand()
    {
      string command = DotNet.Workload()
        .Clean()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload clean");

      await Task.CompletedTask;
    }

    public static async Task CleanWithAll_Should_IncludeAllOption()
    {
      string command = DotNet.Workload()
        .Clean()
        .WithAll()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload clean --all");

      await Task.CompletedTask;
    }

    public static async Task Restore_Should_BuildRestoreCommand()
    {
      string command = DotNet.Workload()
        .Restore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload restore");

      await Task.CompletedTask;
    }

    public static async Task RestoreWithProject_Should_IncludeProjectPath()
    {
      string command = DotNet.Workload()
        .Restore("MyApp.csproj")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload restore MyApp.csproj");

      await Task.CompletedTask;
    }

    public static async Task RestoreWithComprehensiveOptions_Should_IncludeAllOptions()
    {
      string command = DotNet.Workload()
        .Restore("MyApp.csproj")
        .WithConfigFile("nuget.config")
        .WithDisableParallel()
        .WithIncludePreview()
        .WithInteractive()
        .WithNoCache()
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithTempDir("/tmp")
        .WithVerbosity("normal")
        .WithVersion("8.0.100")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload restore MyApp.csproj --configfile nuget.config --disable-parallel --include-previews --interactive --no-cache --source https://api.nuget.org/v3/index.json --temp-dir /tmp --verbosity normal --version 8.0.100");

      await Task.CompletedTask;
    }

    public static async Task Config_Should_BuildConfigCommand()
    {
      string command = DotNet.Workload()
        .Config()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload config");

      await Task.CompletedTask;
    }

    public static async Task ConfigWithUpdateModeWorkloadSet_Should_IncludeUpdateMode()
    {
      string command = DotNet.Workload()
        .Config()
        .WithUpdateModeWorkloadSet()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload config --update-mode workload-set");

      await Task.CompletedTask;
    }

    public static async Task ConfigWithUpdateModeManifests_Should_IncludeUpdateMode()
    {
      string command = DotNet.Workload()
        .Config()
        .WithUpdateModeManifests()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload config --update-mode manifests");

      await Task.CompletedTask;
    }

    public static async Task ConfigWithCustomUpdateMode_Should_IncludeUpdateMode()
    {
      string command = DotNet.Workload()
        .Config()
        .WithUpdateMode("manifests")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload config --update-mode manifests");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Workload()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_ENV", "test")
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload list");

      await Task.CompletedTask;
    }

    public static async Task ListWithQuietVerbosity_Should_IncludeVerbosity()
    {
      string command = DotNet.Workload()
        .List()
        .WithVerbosity("quiet")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload list --verbosity quiet");

      await Task.CompletedTask;
    }

    public static async Task SearchWithQuietVerbosity_Should_IncludeVerbosity()
    {
      string command = DotNet.Workload()
        .Search("maui")
        .WithVerbosity("quiet")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload search maui --verbosity quiet");

      await Task.CompletedTask;
    }

    public static async Task InfoExecution_Should_BuildInfoCommand()
    {
      string command = DotNet.Workload()
        .Info()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet workload --info");

      await Task.CompletedTask;
    }
  }
}
