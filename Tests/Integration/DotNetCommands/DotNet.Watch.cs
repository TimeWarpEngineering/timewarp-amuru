#!/usr/bin/dotnet --

await RunTests<DotNetWatchCommandTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetWatchCommandTests

{
  public static async Task TestBasicDotNetWatchBuilderCreation()
  {
    // DotNet.Watch() alone doesn't build a valid command - needs a subcommand
    DotNetWatchBuilder watchBuilder = DotNet.Watch();
    
    watchBuilder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchRunCommand()
  {
    string command = DotNet.Watch()
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchTestCommand()
  {
    string command = DotNet.Watch()
      .Test()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch test");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchBuildCommand()
  {
    string command = DotNet.Watch()
      .Build()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch build");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithProject()
  {
    string command = DotNet.Watch()
      .WithProject("MyApp.csproj")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --project MyApp.csproj run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithBasicOptions()
  {
    string command = DotNet.Watch()
      .WithQuiet()
      .WithVerbose()
      .WithList()
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --quiet --verbose --list run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithNoOptions()
  {
    string command = DotNet.Watch()
      .WithNoRestore()
      .WithNoLaunchProfile()
      .WithNoHotReload()
      .WithNoBuild()
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --no-restore --no-launch-profile --no-hot-reload --no-build run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithIncludeExcludePatterns()
  {
    string command = DotNet.Watch()
      .WithInclude("**/*.cs")
      .WithInclude("**/*.cshtml")
      .WithExclude("**/bin/**")
      .WithExclude("**/obj/**")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --include **/*.cs --include **/*.cshtml --exclude **/bin/** --exclude **/obj/** run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithBuildConfiguration()
  {
    string command = DotNet.Watch()
      .WithConfiguration("Release")
      .WithTargetFramework("net10.0")
      .WithRuntime("linux-x64")
      .WithVerbosity("detailed")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --framework net10.0 --configuration Release --runtime linux-x64 --verbosity detailed run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithPropertiesAndLaunchProfile()
  {
    string command = DotNet.Watch()
      .WithProperty("Configuration=Debug")
      .WithProperty("Platform=x64")
      .WithLaunchProfile("Development")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --property Configuration=Debug --property Platform=x64 --launch-profile Development run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithAdditionalArguments()
  {
    string command = DotNet.Watch()
      .WithArguments("--environment", "Development")
      .WithArgument("--port")
      .WithArgument("5000")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch run --environment Development --port 5000");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Watch()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch run");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchWithComprehensiveOptions()
  {
    string command = DotNet.Watch()
      .WithProject("MyApp.csproj")
      .WithConfiguration("Release")
      .WithTargetFramework("net10.0")
      .WithVerbosity("minimal")
      .WithInclude("**/*.cs")
      .WithExclude("**/bin/**")
      .WithProperty("DefineConstants=RELEASE")
      .WithNoRestore()
      .WithArguments("--environment", "Production")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --project MyApp.csproj --no-restore --include **/*.cs --exclude **/bin/** --property DefineConstants=RELEASE --framework net10.0 --configuration Release --verbosity minimal run --environment Production");
    
    await Task.CompletedTask;
  }

  public static async Task TestWatchListCommandExecution()
  {
    // Test command string generation for list operation
    string command = DotNet.Watch()
      .WithList()
      .WithProject("test.csproj")
      .Run()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet watch --project test.csproj --list run");
    
    await Task.CompletedTask;
  }
}