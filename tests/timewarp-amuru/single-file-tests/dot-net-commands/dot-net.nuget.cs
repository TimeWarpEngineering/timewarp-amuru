#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.NuGet() - validates the fluent API for NuGet operations
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: NuGet (the method being tested)
// Tests verify command string generation for NuGet push, delete, sources, and locals
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class NuGet_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<NuGet_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetNuGetBuilder nugetBuilder = DotNet.NuGet();

      nugetBuilder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Push_Should_BuildPushCommand()
    {
      string command = DotNet.NuGet()
        .Push("package.nupkg")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithApiKey("test-key")
        .WithTimeout(300)
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget push package.nupkg --source https://api.nuget.org/v3/index.json --timeout 300 --api-key test-key");

      await Task.CompletedTask;
    }

    public static async Task Delete_Should_BuildDeleteCommand()
    {
      string command = DotNet.NuGet()
        .Delete("MyPackage", "1.0.0")
        .WithSource("https://api.nuget.org/v3/index.json")
        .WithApiKey("test-key")
        .WithInteractive()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget delete MyPackage 1.0.0 --source https://api.nuget.org/v3/index.json --api-key test-key --interactive");

      await Task.CompletedTask;
    }

    public static async Task ListSources_Should_BuildListSourcesCommand()
    {
      string command = DotNet.NuGet()
        .ListSources()
        .WithFormat("Detailed")
        .WithConfigFile("nuget.config")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget list source --format Detailed --configfile nuget.config");

      await Task.CompletedTask;
    }

    public static async Task AddSource_Should_BuildAddSourceCommand()
    {
      string command = DotNet.NuGet()
        .AddSource("https://my-private-feed.com/v3/index.json")
        .WithName("MyPrivateFeed")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget add source https://my-private-feed.com/v3/index.json --name MyPrivateFeed --username testuser --password testpass");

      await Task.CompletedTask;
    }

    public static async Task SourceManagement_Should_BuildCorrectCommands()
    {
      string enableCommand = DotNet.NuGet().EnableSource("MySource").Build().ToCommandString();
      string disableCommand = DotNet.NuGet().DisableSource("MySource").Build().ToCommandString();
      string removeCommand = DotNet.NuGet().RemoveSource("MySource").Build().ToCommandString();
      string updateCommand = DotNet.NuGet().UpdateSource("MySource").WithSource("https://new-url.com").Build().ToCommandString();

      enableCommand.ShouldBe("dotnet nuget enable source MySource");
      disableCommand.ShouldBe("dotnet nuget disable source MySource");
      removeCommand.ShouldBe("dotnet nuget remove source MySource");
      updateCommand.ShouldBe("dotnet nuget update source MySource --source https://new-url.com");

      await Task.CompletedTask;
    }

    public static async Task Locals_Should_BuildLocalsCommands()
    {
      string clearCommand = DotNet.NuGet().Locals().Clear(NuGetCacheType.HttpCache).Build().ToCommandString();
      string listCommand = DotNet.NuGet().Locals().List(NuGetCacheType.GlobalPackages).Build().ToCommandString();

      clearCommand.ShouldBe("dotnet nuget locals http-cache --clear");
      listCommand.ShouldBe("dotnet nuget locals global-packages --list");

      await Task.CompletedTask;
    }

    public static async Task Why_Should_BuildWhyCommand()
    {
      string command = DotNet.NuGet()
        .Why("Microsoft.Extensions.Logging")
        .WithProject("MyApp.csproj")
        .WithFramework("net10.0")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget why --project MyApp.csproj --framework net10.0 Microsoft.Extensions.Logging");

      await Task.CompletedTask;
    }

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
      string command = DotNet.NuGet()
        .WithWorkingDirectory("/tmp")
        .WithEnvironmentVariable("NUGET_ENV", "test")
        .ListSources()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget list source");

      await Task.CompletedTask;
    }

    public static async Task ListSourcesWithFormat_Should_IncludeFormatOption()
    {
      string command = DotNet.NuGet()
        .ListSources()
        .WithFormat("Short")
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet nuget list source --format Short");

      await Task.CompletedTask;
    }
  }
}
