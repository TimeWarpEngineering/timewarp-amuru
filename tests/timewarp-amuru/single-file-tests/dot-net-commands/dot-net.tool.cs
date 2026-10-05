#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for DotNet.Tool() - validates the fluent API for dotnet tool commands.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: Tool (install, list, restore, run, search, uninstall, update)
// Tests verify command string generation. The real SDK is exercised in dot-net.cli-smoke.cs.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Tool_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Tool_Given_>();

    public static async Task BasicBuilder_Should_CreateSubcommandBuilders()
    {
      DotNetToolBuilder tool = DotNet.Tool();

      tool.ShouldNotBeNull();
      tool.Install("Some.Package").ShouldNotBeNull();
      tool.List().ShouldNotBeNull();
      tool.Restore().ShouldNotBeNull();
      tool.Run("my-tool").ShouldNotBeNull();
      tool.Search("term").ShouldNotBeNull();
      tool.Uninstall("Some.Package").ShouldNotBeNull();
      tool.Update("Some.Package").ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Install_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Install("Some.Package")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool install Some.Package");

      await Task.CompletedTask;
    }

    public static async Task InstallFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .Install("Some.Package")
        .Global()
        .WithToolPath("./tools")
        .WithVersion("1.2.3")
        .WithConfigFile("./nuget.config")
        .WithToolManifest("./dotnet-tools.json")
        .WithFramework("net10.0")
        .WithArchitecture("x64")
        .WithSource("https://example.test/v3/index.json")
        .WithSource("https://example.test/extra.json")
        .WithPrerelease()
        .WithIgnoreFailedSources()
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool install Some.Package --global --tool-path ./tools --version 1.2.3 --configfile ./nuget.config --tool-manifest ./dotnet-tools.json --framework net10.0 --arch x64 --add-source https://example.test/v3/index.json --add-source https://example.test/extra.json --prerelease --ignore-failed-sources --interactive");

      await Task.CompletedTask;
    }

    public static async Task InstallLocalAfterGlobal_Should_EmitLocalOnly()
    {
      string command = DotNet.Tool()
        .Install("Some.Package")
        .Global()
        .Local()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool install Some.Package --local");

      await Task.CompletedTask;
    }

    public static async Task InstallBlankOptionalValues_Should_OmitFlags()
    {
      string command = DotNet.Tool()
        .Install("Some.Package")
        .WithToolPath(" ")
        .WithVersion("")
        .WithConfigFile("   ")
        .WithToolManifest("\t")
        .WithFramework("")
        .WithArchitecture(" ")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool install Some.Package");

      await Task.CompletedTask;
    }

    public static async Task InstallNullPackageId_Should_ThrowArgumentNullException()
    {
      ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => DotNet.Tool().Install(null!));
      exception.ParamName.ShouldBe("packageId");

      await Task.CompletedTask;
    }

    public static async Task List_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool list");

      await Task.CompletedTask;
    }

    public static async Task ListFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .List()
        .Global()
        .WithToolPath("./tools")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool list --global --tool-path ./tools");

      await Task.CompletedTask;
    }

    public static async Task ListGlobalAfterLocal_Should_EmitGlobalOnly()
    {
      string command = DotNet.Tool()
        .List()
        .Local()
        .Global()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool list --global");

      await Task.CompletedTask;
    }

    public static async Task ListBlankToolPath_Should_OmitFlag()
    {
      string command = DotNet.Tool()
        .List()
        .Local()
        .WithToolPath(" ")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool list --local");

      await Task.CompletedTask;
    }

    public static async Task Restore_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Restore()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool restore");

      await Task.CompletedTask;
    }

    public static async Task RestoreFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .Restore()
        .WithConfigFile("./nuget.config")
        .WithToolManifest("./dotnet-tools.json")
        .WithSource("https://example.test/v3/index.json")
        .WithSource("https://example.test/extra.json")
        .WithIgnoreFailedSources()
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool restore --configfile ./nuget.config --tool-manifest ./dotnet-tools.json --add-source https://example.test/v3/index.json --add-source https://example.test/extra.json --ignore-failed-sources --interactive");

      await Task.CompletedTask;
    }

    public static async Task RestoreBlankOptionalValues_Should_OmitFlags()
    {
      string command = DotNet.Tool()
        .Restore()
        .WithConfigFile(" ")
        .WithToolManifest("")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool restore");

      await Task.CompletedTask;
    }

    public static async Task Run_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Run("my-tool")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool run my-tool");

      await Task.CompletedTask;
    }

    public static async Task RunWithArguments_Should_AppendToolArguments()
    {
      string command = DotNet.Tool()
        .Run("my-tool")
        .WithArguments("--version", "--help")
        .WithArgument("extra")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool run my-tool --version --help extra");

      await Task.CompletedTask;
    }

    public static async Task RunNullCommandName_Should_ThrowArgumentNullException()
    {
      ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => DotNet.Tool().Run(null!));
      exception.ParamName.ShouldBe("commandName");

      await Task.CompletedTask;
    }

    public static async Task Search_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Search("dotnet-ef")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool search dotnet-ef");

      await Task.CompletedTask;
    }

    public static async Task SearchFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .Search("dotnet-ef")
        .WithDetail()
        .WithSkip(0)
        .WithTake(5)
        .WithPrerelease()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool search dotnet-ef --detail --skip 0 --take 5 --prerelease");

      await Task.CompletedTask;
    }

    public static async Task SearchNullTerm_Should_ThrowArgumentNullException()
    {
      ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => DotNet.Tool().Search(null!));
      exception.ParamName.ShouldBe("searchTerm");

      await Task.CompletedTask;
    }

    public static async Task Uninstall_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Uninstall("Some.Package")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool uninstall Some.Package");

      await Task.CompletedTask;
    }

    public static async Task UninstallFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .Uninstall("Some.Package")
        .Local()
        .WithToolPath("./tools")
        .WithToolManifest("./dotnet-tools.json")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool uninstall Some.Package --local --tool-path ./tools --tool-manifest ./dotnet-tools.json");

      await Task.CompletedTask;
    }

    public static async Task UninstallGlobalAfterLocal_Should_EmitGlobalOnly()
    {
      string command = DotNet.Tool()
        .Uninstall("Some.Package")
        .Local()
        .Global()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool uninstall Some.Package --global");

      await Task.CompletedTask;
    }

    public static async Task UninstallNullPackageId_Should_ThrowArgumentNullException()
    {
      ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => DotNet.Tool().Uninstall(null!));
      exception.ParamName.ShouldBe("packageId");

      await Task.CompletedTask;
    }

    public static async Task Update_Should_BuildBasicCommand()
    {
      string command = DotNet.Tool()
        .Update("Some.Package")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool update Some.Package");

      await Task.CompletedTask;
    }

    public static async Task UpdateFluentConfiguration_Should_BuildCompleteCommand()
    {
      string command = DotNet.Tool()
        .Update("Some.Package")
        .Global()
        .WithToolPath("./tools")
        .WithConfigFile("./nuget.config")
        .WithToolManifest("./dotnet-tools.json")
        .WithSource("https://example.test/v3/index.json")
        .WithSource("https://example.test/extra.json")
        .WithPrerelease()
        .WithIgnoreFailedSources()
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool update Some.Package --global --tool-path ./tools --configfile ./nuget.config --tool-manifest ./dotnet-tools.json --add-source https://example.test/v3/index.json --add-source https://example.test/extra.json --prerelease --ignore-failed-sources --interactive");

      await Task.CompletedTask;
    }

    public static async Task UpdateLocalAfterGlobal_Should_EmitLocalOnly()
    {
      string command = DotNet.Tool()
        .Update("Some.Package")
        .Global()
        .Local()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool update Some.Package --local");

      await Task.CompletedTask;
    }

    public static async Task UpdateNullPackageId_Should_ThrowArgumentNullException()
    {
      ArgumentNullException exception = Should.Throw<ArgumentNullException>(() => DotNet.Tool().Update(null!));
      exception.ParamName.ShouldBe("packageId");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.Tool()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .WithNoValidation()
        .List()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet tool list");

      await Task.CompletedTask;
    }
  }
}
