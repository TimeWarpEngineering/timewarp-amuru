#region Purpose
// Fluent builder that turns fzf options and an input source into a runnable command.
#endregion

#region Design
// Option flags live in Arguments. SelectWithFzf forwards that list and the builder's
// CommandOptions (validation, working directory, environment) to the fzf stage.
// FromInput writes each item as its own stdin line. Echo is not used, so a value such as
// "-n" stays data, and Windows does not need an echo executable.
// FromFiles walks the working directory in-process for a file-name glob and feeds relative
// paths on stdin. Unix find is not used: Windows find searches file contents, not names.
// FromCommand splits on unquoted whitespace. Single and double quotes group one argument,
// and backslashes stay literal so Windows paths are not treated as escapes.
// The file list and stdin text are captured when Build runs.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for Fzf (fuzzy finder) integration.
/// </summary>
public static class Fzf
{
  /// <summary>
  /// Creates a fluent builder for the 'fzf' command.
  /// </summary>
  /// <returns>A FzfBuilder for configuring the fzf command</returns>
  public static FzfBuilder Builder()
  {
    return new FzfBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'fzf' commands.
/// </summary>
public partial class FzfBuilder
{
  private CommandOptions Options = new();
  private List<string> Arguments = new();
  private List<string> InputItems = new();
  private string? InputCommand;
  private string? InputGlob;
  private bool UseStdin;

  internal CommandOptions StageOptions => Options;

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithWorkingDirectory(string directory)
  {
    Options = Options.WithWorkingDirectory(directory);
    return this;
  }

  /// <summary>
  /// Adds an environment variable for the command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  public CommandResult Build()
  {
    string[] fzfArguments = CopyArguments();

    if (InputItems.Count > 0)
    {
      return Shell.Run("fzf", fzfArguments, Options, BuildStdin(InputItems));
    }

    if (!string.IsNullOrEmpty(InputGlob))
    {
      return Shell.Run("fzf", fzfArguments, Options, BuildFileListInput(InputGlob));
    }

    if (!string.IsNullOrEmpty(InputCommand)
      && TrySplitCommand(InputCommand, out string executable, out string[] commandArguments))
    {
      return Shell.Run(executable, commandArguments, Options).Pipe("fzf", Options, fzfArguments);
    }

    // FromStdin and the default path both leave stdin to the caller.
    if (UseStdin)
    {
      return Shell.Run("fzf", fzfArguments, Options);
    }

    return Shell.Run("fzf", fzfArguments, Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Passes the command through to the terminal with full interactive control.
  /// This allows fzf to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Executes the command with true TTY passthrough for TUI applications.
  /// Unlike PassthroughAsync which pipes Console streams, this method
  /// allows the child process to inherit the terminal's TTY characteristics.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output is inherited)</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Executes fzf interactively and returns the selected item(s).
  /// The fzf UI is displayed on the console, but the selection is captured and returned.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected item(s) as a string, or empty string if cancelled</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }

  internal string[] CopyArguments() => [.. Arguments];

  private static string BuildStdin(List<string> items)
  {
    StringBuilder builder = new();
    foreach (string item in items)
    {
      builder.Append(item);
      builder.Append('\n');
    }

    return builder.ToString();
  }

  private string BuildFileListInput(string pattern)
  {
    if (pattern.Contains('/', StringComparison.Ordinal) || pattern.Contains('\\', StringComparison.Ordinal))
    {
      throw new ArgumentException
      (
        "FromFiles expects a file-name glob such as \"*.cs\", not a directory path.",
        nameof(pattern)
      );
    }

    string searchRoot = string.IsNullOrEmpty(Options.WorkingDirectory)
      ? Directory.GetCurrentDirectory()
      : Options.WorkingDirectory;

    EnumerationOptions enumerationOptions = new()
    {
      RecurseSubdirectories = true,
      IgnoreInaccessible = true,
      AttributesToSkip = FileAttributes.None,
      MatchType = MatchType.Simple,
      ReturnSpecialDirectories = false
    };

    StringBuilder builder = new();
    foreach (string filePath in Directory.EnumerateFiles(searchRoot, pattern, enumerationOptions))
    {
      string relative = Path.GetRelativePath(searchRoot, filePath);
      builder.Append("./");
      builder.Append(relative.Replace('\\', '/'));
      builder.Append('\n');
    }

    return builder.ToString();
  }

  private static bool TrySplitCommand(string command, out string executable, out string[] arguments)
  {
    List<string> tokens = [];
    StringBuilder current = new();
    bool inSingle = false;
    bool inDouble = false;
    bool tokenStarted = false;

    foreach (char c in command)
    {
      if (inSingle)
      {
        if (c == '\'')
        {
          inSingle = false;
        }
        else
        {
          current.Append(c);
        }

        continue;
      }

      if (inDouble)
      {
        if (c == '"')
        {
          inDouble = false;
        }
        else
        {
          current.Append(c);
        }

        continue;
      }

      if (c == '\'')
      {
        inSingle = true;
        tokenStarted = true;
        continue;
      }

      if (c == '"')
      {
        inDouble = true;
        tokenStarted = true;
        continue;
      }

      if (char.IsWhiteSpace(c))
      {
        if (tokenStarted)
        {
          tokens.Add(current.ToString());
          current.Clear();
          tokenStarted = false;
        }

        continue;
      }

      current.Append(c);
      tokenStarted = true;
    }

    if (tokenStarted)
    {
      tokens.Add(current.ToString());
    }

    if (tokens.Count == 0 || string.IsNullOrWhiteSpace(tokens[0]))
    {
      executable = string.Empty;
      arguments = [];
      return false;
    }

    executable = tokens[0];
    arguments = tokens.Count == 1 ? [] : [.. tokens.Skip(1)];
    return true;
  }
}
