#region Purpose
// Constructs CommandResult instances from an executable, arguments, and command options.
// Centralizes command path resolution, script argument normalization, and optional stdin wiring.
#endregion

#region Design
// - This is the command construction surface; CommandResult is the execution surface.
// - Returns NullCommandResult for invalid input so higher layers can degrade gracefully.
// - Applies CliConfiguration path overrides before building the CliWrap command.
// - Inserts "--" for .cs targets so dotnet file-based apps receive arguments correctly.
// - Applies CommandOptions in one place to keep ShellBuilder and other builders thin.
// - Optional standardInput is attached as a PipeSource only when explicitly provided.
#endregion

#region Responsibilities
// This file is responsible for:
// - validating executable/options input before command construction
// - normalizing arguments for .NET file-based app execution
// - creating the underlying CliWrap command
// - attaching execution options and standard input
// It is not responsible for:
// - running commands
// - capturing output
// - interactive passthrough or TTY behavior
// - streaming output
#endregion

namespace TimeWarp.Amuru;

internal static class CommandExtensions
{
  private const string CSharpScriptExtension = ".cs";

  internal static CommandResult Run
  (
    string executable,
    params string[]? arguments
  )
  {
    return Run(executable, arguments, new CommandOptions());
  }

  internal static CommandResult Run
  (
    string executable,
    string[]? arguments,
    CommandOptions commandOptions,
    string? standardInput = null
  )
  {
    // Input validation
    if (string.IsNullOrWhiteSpace(executable))
    {
      return CommandResult.NullCommandResult;
    }

    if (commandOptions == null)
    {
      return CommandResult.NullCommandResult;
    }

    // Check for configured command path override
    executable = CliConfiguration.GetCommandPath(executable);

    // Handle .cs script files specially
    if (executable.EndsWith(CSharpScriptExtension, StringComparison.OrdinalIgnoreCase))
    {
      // Insert -- at the beginning of arguments to prevent dotnet from intercepting them
      List<string> newArgs = ["--", .. (arguments ?? [])];
      arguments = [.. newArgs];
    }

    Command cliCommand = CliWrap.Cli.Wrap(executable)
      .WithArguments(arguments ?? []);

    // Apply configuration options
    cliCommand = commandOptions.ApplyTo(cliCommand);

    // Apply standard input if provided
    if (!string.IsNullOrEmpty(standardInput))
    {
      cliCommand = cliCommand.WithStandardInputPipe(PipeSource.FromString(standardInput));
    }

    return new CommandResult(cliCommand);
  }
}
