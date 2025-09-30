#!/usr/bin/dotnet run

await RunTests<DotNetWorkloadCommandTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetWorkloadCommandTests

{
  public static async Task TestBasicDotNetWorkloadBuilderCreation()
  {
    // DotNet.Workload() alone doesn't build a valid command - needs a subcommand
    DotNetWorkloadBuilder workloadBuilder = DotNet.Workload();
    
    workloadBuilder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadInfoCommand()
  {
    string command = DotNet.Workload()
      .Info()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload --info");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadVersionCommand()
  {
    string command = DotNet.Workload()
      .Version()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload --version");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadInstallCommand()
  {
    string command = DotNet.Workload()
      .Install("maui")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload install maui");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadInstallWithMultipleWorkloadsAndOptions()
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

  public static async Task TestWorkloadListCommand()
  {
    string command = DotNet.Workload()
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload list");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadListWithVerbosity()
  {
    string command = DotNet.Workload()
      .List()
      .WithVerbosity("detailed")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload list --verbosity detailed");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadSearchCommand()
  {
    string command = DotNet.Workload()
      .Search()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload search");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadSearchWithSearchString()
  {
    string command = DotNet.Workload()
      .Search("maui")
      .WithVerbosity("minimal")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload search maui --verbosity minimal");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadUninstallCommand()
  {
    string command = DotNet.Workload()
      .Uninstall("maui")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload uninstall maui");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadUninstallWithMultipleWorkloads()
  {
    string command = DotNet.Workload()
      .Uninstall("maui", "android", "ios")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload uninstall maui android ios");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadUpdateCommand()
  {
    string command = DotNet.Workload()
      .Update()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload update");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadUpdateWithComprehensiveOptions()
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

  public static async Task TestWorkloadRepairCommand()
  {
    string command = DotNet.Workload()
      .Repair()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload repair");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadRepairWithComprehensiveOptions()
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

  public static async Task TestWorkloadCleanCommand()
  {
    string command = DotNet.Workload()
      .Clean()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload clean");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadCleanWithAllOption()
  {
    string command = DotNet.Workload()
      .Clean()
      .WithAll()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload clean --all");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadRestoreCommand()
  {
    string command = DotNet.Workload()
      .Restore()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload restore");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadRestoreWithProject()
  {
    string command = DotNet.Workload()
      .Restore("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload restore MyApp.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadRestoreWithComprehensiveOptions()
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

  public static async Task TestWorkloadConfigCommand()
  {
    string command = DotNet.Workload()
      .Config()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload config");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadConfigWithUpdateModeWorkloadSet()
  {
    string command = DotNet.Workload()
      .Config()
      .WithUpdateModeWorkloadSet()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload config --update-mode workload-set");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadConfigWithUpdateModeManifests()
  {
    string command = DotNet.Workload()
      .Config()
      .WithUpdateModeManifests()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload config --update-mode manifests");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadConfigWithCustomUpdateMode()
  {
    string command = DotNet.Workload()
      .Config()
      .WithUpdateMode("manifests")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload config --update-mode manifests");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Workload()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("DOTNET_ENV", "test")
      .List()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload list");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadListCommandExecution()
  {
    // Test command string generation for list operation
    string command = DotNet.Workload()
      .List()
      .WithVerbosity("quiet")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload list --verbosity quiet");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadSearchCommandExecution()
  {
    // Test command string generation for search operation
    string command = DotNet.Workload()
      .Search("maui")
      .WithVerbosity("quiet")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload search maui --verbosity quiet");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkloadInfoCommandExecution()
  {
    // Test command string generation for info operation
    string command = DotNet.Workload()
      .Info()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet workload --info");
    
    await Task.CompletedTask;
  }
}