#!/usr/bin/dotnet --

await RunTests<DotNetNuGetTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetNuGetTests

{
  public static async Task BasicDotNetNuGetBuilderCreation()
  {
    // DotNet.NuGet() alone doesn't build a valid command - needs a subcommand
    DotNetNuGetBuilder nugetBuilder = DotNet.NuGet();
    
    nugetBuilder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task NuGetPushCommandBuilder()
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

  public static async Task NuGetDeleteCommandBuilder()
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

  public static async Task NuGetListSourcesCommandBuilder()
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

  public static async Task NuGetAddSourceCommandBuilder()
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

  public static async Task NuGetSourceManagementCommands()
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

  public static async Task NuGetLocalsCommandBuilder()
  {
    string clearCommand = DotNet.NuGet().Locals().Clear(NuGetCacheType.HttpCache).Build().ToCommandString();
    string listCommand = DotNet.NuGet().Locals().List(NuGetCacheType.GlobalPackages).Build().ToCommandString();
    
    clearCommand.ShouldBe("dotnet nuget locals http-cache --clear");
    
    listCommand.ShouldBe("dotnet nuget locals global-packages --list");
    
    await Task.CompletedTask;
  }

  public static async Task NuGetWhyCommandBuilder()
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

  public static async Task WorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.NuGet()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("NUGET_ENV", "test")
      .ListSources()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet nuget list source");
    
    await Task.CompletedTask;
  }

  public static async Task CommandExecutionListSources()
  {
    // Test command string generation for list sources
    string command = DotNet.NuGet()
      .ListSources()
      .WithFormat("Short")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet nuget list source --format Short");
    
    await Task.CompletedTask;
  }
}