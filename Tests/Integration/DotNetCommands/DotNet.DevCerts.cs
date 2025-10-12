#!/usr/bin/dotnet --

await RunTests<DotNetDevCertsCommandTests>();

// Define a class to hold the test methods (NOT static so it can be used as generic parameter)
 
internal sealed class DotNetDevCertsCommandTests

{
  public static async Task TestBasicDotNetDevCertsBuilderCreation()
  {
    // DotNet.DevCerts() alone doesn't build a valid command - needs a subcommand
    DotNetDevCertsBuilder devCertsBuilder = DotNet.DevCerts();
    
    devCertsBuilder.ShouldNotBeNull();
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsCommand()
  {
    string command = DotNet.DevCerts().Https()
      .Build()
      .ToCommandString();
      
    command.ShouldBe("dotnet dev-certs https");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithCheckOption()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithCheck()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --check");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithCleanOption()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithClean()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --clean");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithExportOptions()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithExport()
      .WithExportPath("./localhost.pfx")
      .WithPassword("testpassword")
      .WithFormat("Pfx")
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --export --export-path ./localhost.pfx --password testpassword --format Pfx");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithTrustOption()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithTrust()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --trust");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithNoPasswordOption()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithExport()
      .WithExportPath("./localhost.pfx")
      .WithNoPassword()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --export --export-path ./localhost.pfx --no-password");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithVerboseAndQuietOptions()
  {
    string verboseCommand = DotNet.DevCerts().Https().WithVerbose().Build().ToCommandString();
    string quietCommand = DotNet.DevCerts().Https().WithQuiet().Build().ToCommandString();
    
    verboseCommand.ShouldBe("dotnet dev-certs https --verbose");
    
    quietCommand.ShouldBe("dotnet dev-certs https --quiet");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsHttpsWithPemFormat()
  {
    string command = DotNet.DevCerts()
      .Https()
      .WithExport()
      .WithExportPath("./localhost.pem")
      .WithFormat("Pem")
      .WithNoPassword()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --export --export-path ./localhost.pem --format Pem --no-password");
    
    await Task.CompletedTask;
  }

  public static async Task TestWorkingDirectoryAndEnvironmentVariables()
  {
    // Note: Working directory and environment variables don't appear in ToCommandString()
    string command = DotNet.DevCerts()
      .WithWorkingDirectory("/tmp")
      .WithEnvironmentVariable("DOTNET_ENV", "test")
      .Https()
      .WithCheck()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --check");
    
    await Task.CompletedTask;
  }

  public static async Task TestDevCertsCheckCommandExecution()
  {
    // Test command string generation for check operation
    string command = DotNet.DevCerts()
      .Https()
      .WithCheck()
      .WithQuiet()
      .Build()
      .ToCommandString();
    
    command.ShouldBe("dotnet dev-certs https --check --quiet");
    
    await Task.CompletedTask;
  }
}