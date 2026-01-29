#!/usr/bin/dotnet --

await RunTests<DotNetPublishTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetPublishTests

{

  public static async Task TestBasicDotNetPublishCommand()
  {
    string command = DotNet.Publish()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish");
    
    await Task.CompletedTask;
  }
  
  public static async Task TestPublishWithProjectOnly()
  {
    string command = DotNet.Publish()
      .WithProject("MyApp.csproj")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish MyApp.csproj");
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
  {
    string command = DotNet.Publish()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithFramework("net10.0")
      .WithRuntime("win-x64")
      .WithOutput("./publish")
      .WithNoRestore()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish test.csproj --configuration Release --framework net10.0 --runtime win-x64 --output ./publish --no-restore");
    
    await Task.CompletedTask;
  }

  public static async Task TestAdvancedDeploymentOptions()
  {
    string command = DotNet.Publish()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithRuntime("linux-x64")
      .WithSelfContained()
      .WithReadyToRun()
      .WithSingleFile()
      .WithTrimmed()
      .WithNoLogo()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish test.csproj --configuration Release --runtime linux-x64 --nologo --self-contained --property:PublishReadyToRun=true --property:PublishSingleFile=true --property:PublishTrimmed=true");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.Publish()
      .WithProject("test.csproj")
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("PUBLISH_ENV", "production")
      .WithArchitecture("x64")
      .WithOperatingSystem("linux")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish test.csproj --arch x64 --os linux");
    
    await Task.CompletedTask;
  }

  public static async Task TestMSBuildPropertiesAndPublishingConfiguration()
  {
    string command = DotNet.Publish()
      .WithProject("test.csproj")
      .WithConfiguration("Release")
      .WithProperty("PublishProfile", "Production")
      .WithProperty("EnvironmentName", "Staging")
      .WithSource("https://api.nuget.org/v3/index.json")
      .WithVerbosity("minimal")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish test.csproj --configuration Release --verbosity minimal --source https://api.nuget.org/v3/index.json --property:PublishProfile=Production --property:EnvironmentName=Staging");
    
    await Task.CompletedTask;
  }

  public static async Task TestPublishOverloadWithProjectParameter()
  {
    string command = DotNet.Publish("test.csproj")
      .WithConfiguration("Release")
      .WithRuntime("win-x64")
      .WithNoSelfContained()
      .WithNoBuild()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish test.csproj --configuration Release --runtime win-x64 --no-build --no-self-contained");
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandBuilderWithNonExistentProject()
  {
    // Verify that the command builder creates a valid command even with non-existent project
    string command = DotNet.Publish()
      .WithProject("nonexistent.csproj")
      .WithConfiguration("Release")
      .WithRuntime("win-x64")
      .WithNoRestore()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet publish nonexistent.csproj --configuration Release --runtime win-x64 --no-restore");
    
    await Task.CompletedTask;
  }
}