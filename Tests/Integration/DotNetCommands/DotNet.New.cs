#!/usr/bin/dotnet --

await RunTests<DotNetNewTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetNewTests

{
  public static async Task TestBasicNewBuilderCreation()
  {
    string command = DotNet.New("console")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet new console");
    
    await Task.CompletedTask;
  }

  public static async Task TestNewWithoutTemplateName()
  {
    string command = DotNet.New()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet new");
    
    await Task.CompletedTask;
  }

  public static async Task TestFluentConfigurationMethods()
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

  public static async Task TestTemplateArgumentsAndAdvancedOptions()
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

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
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

  public static async Task TestNewListSubcommand()
  {
    string command = DotNet.New().List("console")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet new list console");
    
    await Task.CompletedTask;
  }

  public static async Task TestNewSearchSubcommand()
  {
    string command = DotNet.New().Search("blazor")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet new search blazor");
    
    await Task.CompletedTask;
  }

  public static async Task TestCommandExecutionWithDryRun()
  {
    // Test command string generation for dry run
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