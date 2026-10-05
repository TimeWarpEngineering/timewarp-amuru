#!/usr/bin/env -S dotnet --

#region Purpose
// Executes dotnet builders against the SDK pinned by the repo global.json.
#endregion

#region Design
// Each smoke runs in an empty directory that copies global.json, so roll-forward stays on the 10.0 feature band
// and dotnet does not select a project from the repo. Assertions require the CLI to accept the emitted flag:
// a missing project is MSB1003, not MSB1009 from a terminal-logger mode token.
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
