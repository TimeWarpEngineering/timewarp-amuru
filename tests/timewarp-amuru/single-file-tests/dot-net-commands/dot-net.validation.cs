#!/usr/bin/env -S dotnet --

#region Purpose
// Proves each newly covered dotnet builder reports a failing command by default and throws under WithZeroExitCodeValidation.
#endregion

#region Design
// Each case is a read-only SDK failure in an empty directory that copies global.json.
// Strict validation is set on the leaf builder so a copied options instance must be replaceable.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace DotNet_
{
  [TestTag("DotNetCommands")]
  public class Validation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Validation_Given_>();

    [Timeout(60000)]
    public static async Task BaseBuilder_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetBuilder builder = DotNet.Builder()
          .WithArguments("not-a-command")
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1");
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task DevCerts_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetDevCertsHttpsBuilder builder = DotNet.DevCerts()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Https()
          .WithFormat("nope");
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task NuGet_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        string configFile = Path.Combine(directory, "missing.config");
        DotNetNuGetListSourceBuilder builder = DotNet.NuGet()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .ListSources()
          .WithConfigFile(configFile);
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task Reference_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetReferenceListBuilder builder = DotNet.Reference()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List();
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task Sln_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetSlnListBuilder builder = DotNet.Sln()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List();
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task UserSecrets_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetUserSecretsListBuilder builder = DotNet.UserSecrets()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List();
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(120000)]
    public static async Task Watch_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetWatchRunBuilder builder = DotNet.Watch()
          .WithList()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .Run();
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task Workload_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetWorkloadListBuilder builder = DotNet.Workload()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List()
          .WithVerbosity("not-a-level");
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(120000)]
    public static async Task New_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        DotNetNewListBuilder builder = DotNet.New()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List("NotARealTemplateXYZ");
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    [Timeout(60000)]
    public static async Task Tool_Should_ReportFailureUnlessZeroExitValidation()
    {
      await AssertValidationAsync(static (directory, strict) =>
      {
        string toolPath = Path.Combine(directory, "missing-tools");
        DotNetToolListBuilder builder = DotNet.Tool()
          .WithWorkingDirectory(directory)
          .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
          .WithEnvironmentVariable("DOTNET_NOLOGO", "1")
          .List()
          .WithToolPath(toolPath);
        if (strict)
        {
          builder = builder.WithZeroExitCodeValidation();
        }

        return builder.Build();
      });
    }

    private static async Task AssertValidationAsync(Func<string, bool, CommandResult> create)
    {
      ArgumentNullException.ThrowIfNull(create);
      string directory = CreatePinnedEmptyDirectory();
      try
      {
        CommandOutput output = await create(directory, false).CaptureAsync();
        output.Success.ShouldBeFalse();
        output.ExitCode.ShouldNotBe(0);

        await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(async () =>
          await create(directory, true).CaptureAsync());
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    private static string CreatePinnedEmptyDirectory()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-validation-").FullName;
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
