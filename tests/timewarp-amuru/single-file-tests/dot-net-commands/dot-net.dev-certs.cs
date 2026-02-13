#!/usr/bin/dotnet --

#region Purpose
// Tests for DotNet.DevCerts() - validates the fluent API for managing development certificates
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: DotNet (the static class providing command builders)
// Action: DevCerts (the method being tested)
// Tests verify command string generation for HTTPS certificate management
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class DevCerts_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<DevCerts_Given_>();

    public static async Task BasicBuilder_Should_CreateBuilder()
    {
      DotNetDevCertsBuilder devCertsBuilder = DotNet.DevCerts();

      devCertsBuilder.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task Https_Should_BuildHttpsCommand()
    {
      string command = DotNet.DevCerts().Https()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet dev-certs https");

      await Task.CompletedTask;
    }

    public static async Task HttpsWithCheck_Should_IncludeCheckOption()
    {
      string command = DotNet.DevCerts()
        .Https()
        .WithCheck()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet dev-certs https --check");

      await Task.CompletedTask;
    }

    public static async Task HttpsWithClean_Should_IncludeCleanOption()
    {
      string command = DotNet.DevCerts()
        .Https()
        .WithClean()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet dev-certs https --clean");

      await Task.CompletedTask;
    }

    public static async Task HttpsWithExport_Should_IncludeExportOptions()
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

    public static async Task HttpsWithTrust_Should_IncludeTrustOption()
    {
      string command = DotNet.DevCerts()
        .Https()
        .WithTrust()
        .Build()
        .ToCommandString();

      command.ShouldBe("dotnet dev-certs https --trust");

      await Task.CompletedTask;
    }

    public static async Task HttpsWithNoPassword_Should_IncludeNoPasswordOption()
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

    public static async Task HttpsWithVerboseAndQuiet_Should_IncludeOptions()
    {
      string verboseCommand = DotNet.DevCerts().Https().WithVerbose().Build().ToCommandString();
      string quietCommand = DotNet.DevCerts().Https().WithQuiet().Build().ToCommandString();

      verboseCommand.ShouldBe("dotnet dev-certs https --verbose");
      quietCommand.ShouldBe("dotnet dev-certs https --quiet");

      await Task.CompletedTask;
    }

    public static async Task HttpsWithPemFormat_Should_IncludeFormatOption()
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

    public static async Task WorkingDirectoryAndEnvVars_Should_NotAppearInCommandString()
    {
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

    public static async Task HttpsCheckWithQuiet_Should_BuildCorrectCommand()
    {
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
}
