#!/usr/bin/env -S dotnet --

#region Purpose
// Executes dotnet builders against the SDK pinned by the repo global.json.
#endregion

#region Design
// Each smoke runs in an empty directory that copies global.json, so roll-forward stays on the 10.0 feature band
// and dotnet does not select a project from the repo. Assertions require the CLI to accept the emitted flag:
// a missing project is MSB1003, not MSB1009 from a terminal-logger mode token.
// Tool smokes stay out of the user tool store: list/restore/run/uninstall use the empty directory,
// and install/update use a NuGet config whose sources are cleared to an empty local folder.
// Search queries the public feed for a term with no hits and does not install anything.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  [TestTag("DotNetCliSmoke")]
  public class CliSmoke_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CliSmoke_Given_>();

    [Timeout(60000)]
    public static async Task Build_Should_AcceptTerminalLogger()
    {
      await AssertMissingProjectAcceptsTerminalLoggerAsync(directory =>
        DotNet.Build()
          .WithTerminalLogger("off")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Build());
    }

    [Timeout(60000)]
    public static async Task Build_WithZeroExitCodeValidation_Should_ThrowWhenProjectIsMissing()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(async () =>
          await DotNet.Build()
            .WithWorkingDirectory(directory)
            .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
            .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
            .WithZeroExitCodeValidation()
            .Build()
            .CaptureAsync());
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task Restore_Should_AcceptTerminalLogger()
    {
      await AssertMissingProjectAcceptsTerminalLoggerAsync(directory =>
        DotNet.Restore()
          .WithTerminalLogger("off")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Build());
    }

    [Timeout(60000)]
    public static async Task Run_Should_AcceptTerminalLogger()
    {
      await AssertMissingProjectAcceptsTerminalLoggerAsync(
        directory =>
          DotNet.Run()
            .WithTerminalLogger("off")
            .WithWorkingDirectory(directory)
            .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
            .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
            .Build(),
        "Couldn't find a project to run");
    }

    [Timeout(60000)]
    public static async Task Publish_Should_AcceptTerminalLogger()
    {
      await AssertMissingProjectAcceptsTerminalLoggerAsync(directory =>
        DotNet.Publish()
          .WithTerminalLogger("off")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Build());
    }

    [Timeout(60000)]
    public static async Task Pack_Should_AcceptTerminalLoggerWithoutFrameworkSwitch()
    {
      await AssertMissingProjectAcceptsTerminalLoggerAsync(directory =>
        DotNet.Pack()
          .WithTerminalLogger("off")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Build());
    }

    [Timeout(60000)]
    public static async Task Test_Should_AcceptCollectAndTerminalLogger()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Test()
          .WithTerminalLogger("off")
          .WithCollect("XPlat Code Coverage")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Build();

        string commandText = command.ToCommandString();
        commandText.ShouldContain("--tl:off");
        commandText.ShouldContain("--collect");
        commandText.ShouldContain("XPlat Code Coverage");

        CommandOutput output = await command.CaptureAsync();
        output.Combined.ShouldContain("MSB1003");
        output.Combined.ShouldNotContain("MSB1009");
        output.Combined.ShouldNotContain("Required argument missing for option: '--collect'");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task DevCerts_Should_RunCheck()
    {
      CommandOutput output = await DotNet.DevCerts()
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .Https()
        .WithCheck()
        .WithQuiet()
        .Build()
        .CaptureAsync();

      output.ExitCode.ShouldBeGreaterThanOrEqualTo(0);
      output.Combined.ShouldNotContain("Specify --help");
      output.Combined.ShouldNotContain("--export");
    }

    [Timeout(60000)]
    public static async Task NuGetWhy_Should_AcceptPositionalProject()
    {
      string project = Path.Combine(
        FindRepoRoot(),
        "source",
        "timewarp-amuru-tools",
        "timewarp-amuru-tools.csproj");

      CommandResult command = DotNet.NuGet()
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .Why("TimeWarp.Amuru.NotADependency")
        .WithProject(project)
        .WithFramework("net10.0")
        .Build();

      string commandText = command.ToCommandString();
      commandText.ShouldNotContain("--project");
      commandText.ShouldContain(project);
      commandText.ShouldContain("--framework net10.0");

      CommandOutput output = await command.CaptureAsync();
      output.Combined.ShouldContain("does not have a dependency");
      output.Combined.ShouldNotContain("--project");
    }

    [Timeout(60000)]
    public static async Task NuGetDelete_Should_FailFastWhenNonInteractive()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.NuGet()
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .Delete("NotARealPackage", "0.0.1")
          .WithSource(directory)
          .WithNonInteractive()
          .Build();

        string commandText = command.ToCommandString();
        commandText.ShouldContain("--non-interactive");
        commandText.ShouldNotContain("--configfile");

        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(30));
        CommandOutput output = await command.CaptureAsync(cancellation.Token);
        output.Success.ShouldBeFalse();
        output.Combined.ShouldContain("Not Found");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task Watch_Should_ReportMissingProject()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Watch()
          .WithList()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Run()
          .Build();

        command.ToCommandString().ShouldBe("dotnet watch --list run");

        CommandOutput output = await command.CaptureAsync();
        output.Combined.ShouldContain("Could not find a MSBuild project file");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolList_Should_ListEmptyToolPath()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List()
          .WithToolPath(directory)
          .Build();

        command.ToCommandString().ShouldContain("--tool-path");

        CommandOutput output = await command.CaptureAsync();
        output.Success.ShouldBeTrue();
        output.Stdout.ShouldContain("Package Id");
        output.Combined.ShouldNotContain("Unrecognized");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolRestore_Should_ReportMissingManifest()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Restore()
          .Build();

        command.ToCommandString().ShouldBe("dotnet tool restore");

        CommandOutput output = await command.CaptureAsync();
        output.ExitCode.ShouldBe(0);
        output.Combined.ShouldContain("Cannot find a manifest file");
        output.Combined.ShouldContain("No tools were restored.");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolRun_Should_ReportMissingCommand()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Run("not-a-real-tool")
          .Build();

        command.ToCommandString().ShouldBe("dotnet tool run not-a-real-tool");

        CommandOutput output = await command.CaptureAsync();
        output.Success.ShouldBeFalse();
        output.Combined.ShouldContain("Cannot find a tool in the manifest file that has a command named 'not-a-real-tool'");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolSearch_Should_AcceptDetailSkipAndTake()
    {
      CommandResult command = DotNet.Tool()
        .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
        .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
        .Search("zzznone-amuru")
        .WithDetail()
        .WithSkip(0)
        .WithTake(1)
        .WithPrerelease()
        .Build();

      command.ToCommandString().ShouldBe("dotnet tool search zzznone-amuru --detail --skip 0 --take 1 --prerelease");

      CommandOutput output = await command.CaptureAsync();
      output.ExitCode.ShouldBe(0);
      output.Combined.ShouldContain("Could not find any results.");
    }

    [Timeout(60000)]
    public static async Task ToolUninstall_Should_ReportMissingPackage()
    {
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Uninstall("NotARealPackage.Amuru.Test")
          .WithToolPath(directory)
          .Build();

        command.ToCommandString().ShouldContain("tool uninstall");
        command.ToCommandString().ShouldContain("--tool-path");

        CommandOutput output = await command.CaptureAsync();
        output.Success.ShouldBeFalse();
        output.Combined.ShouldContain("could not be found");
        output.Combined.ShouldNotContain("Unrecognized");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolInstall_Should_RejectUnknownPackageWithoutInstalling()
    {
      string directory = CreatePinnedEmptyDirectory();
      string sourceDirectory = Directory.CreateTempSubdirectory("amuru-tool-source-").FullName;
      try
      {
        string toolPath = Path.Combine(directory, "tools");
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Install("NotARealPackage.Amuru.Test")
          .WithToolPath(toolPath)
          .WithConfigFile(WriteClearedNuGetConfig(directory, sourceDirectory))
          .WithIgnoreFailedSources()
          .Build();

        string commandText = command.ToCommandString();
        commandText.ShouldContain("tool install");
        commandText.ShouldContain("--configfile");
        commandText.ShouldContain("--ignore-failed-sources");

        CommandOutput output = await command.CaptureAsync();
        output.Success.ShouldBeFalse();
        output.Combined.ShouldContain("is not found in NuGet feeds");
        output.Combined.ShouldContain(sourceDirectory);
        output.Combined.ShouldNotContain("api.nuget.org");
        output.Combined.ShouldNotContain("Unrecognized");
        Directory.Exists(toolPath).ShouldBeFalse();
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
        Directory.Delete(sourceDirectory, recursive: true);
      }
    }

    [Timeout(60000)]
    public static async Task ToolUpdate_Should_RejectUnknownPackageWithoutUpdating()
    {
      string directory = CreatePinnedEmptyDirectory();
      string sourceDirectory = Directory.CreateTempSubdirectory("amuru-tool-source-").FullName;
      try
      {
        CommandResult command = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Update("NotARealPackage.Amuru.Test")
          .WithToolPath(directory)
          .WithConfigFile(WriteClearedNuGetConfig(directory, sourceDirectory))
          .WithIgnoreFailedSources()
          .Build();

        string commandText = command.ToCommandString();
        commandText.ShouldContain("tool update");
        commandText.ShouldContain("--configfile");
        commandText.ShouldContain("--ignore-failed-sources");

        CommandOutput output = await command.CaptureAsync();
        output.Success.ShouldBeFalse();
        output.Combined.ShouldContain("is not found in NuGet feeds");
        output.Combined.ShouldContain(sourceDirectory);
        output.Combined.ShouldNotContain("api.nuget.org");
        output.Combined.ShouldNotContain("Unrecognized");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
        Directory.Delete(sourceDirectory, recursive: true);
      }
    }

    private static async Task AssertMissingProjectAcceptsTerminalLoggerAsync(
      Func<string, CommandResult> createCommand,
      string acceptedError = "MSB1003")
    {
      ArgumentNullException.ThrowIfNull(createCommand);
      ArgumentException.ThrowIfNullOrWhiteSpace(acceptedError);
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandResult command = createCommand(directory);
        string commandText = command.ToCommandString();
        commandText.ShouldContain("--tl:off");
        commandText.ShouldNotContain("--tl off");

        CommandOutput output = await command.CaptureAsync();
        output.Combined.ShouldContain(acceptedError);
        output.Combined.ShouldNotContain("MSB1009");
        output.Combined.ShouldNotContain("MSB1001");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    private static string CreatePinnedEmptyDirectory()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-cli-smoke-").FullName;
      File.Copy(Path.Combine(FindRepoRoot(), "global.json"), Path.Combine(directory, "global.json"));
      return directory;
    }

    private static string WriteClearedNuGetConfig(string directory, string sourceDirectory)
    {
      string configPath = Path.Combine(directory, "nuget.config");
      File.WriteAllText(
        configPath,
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
        + "<configuration>\n"
        + "  <packageSources>\n"
        + "    <clear />\n"
        + "    <add key=\"local\" value=\"" + sourceDirectory + "\" />\n"
        + "  </packageSources>\n"
        + "</configuration>\n");
      return configPath;
    }

    private static string FindRepoRoot()
    {
      DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
      while (directory is not null)
      {
        if (File.Exists(Path.Combine(directory.FullName, "global.json"))
          && File.Exists(Path.Combine(directory.FullName, "timewarp-amuru.slnx")))
        {
          return directory.FullName;
        }

        directory = directory.Parent;
      }

      throw new InvalidOperationException(
        "The repo root (global.json and timewarp-amuru.slnx) was not found above the working directory.");
    }
  }
}
