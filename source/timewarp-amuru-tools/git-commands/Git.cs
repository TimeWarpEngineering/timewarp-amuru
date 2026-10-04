#region Purpose
// Shared helpers for Git command execution and error text.
#endregion

#region Design
// GitBuilder is the only place a repository path becomes a working directory.
// A null or blank path leaves the process working directory unchanged, so callers
// that omit a path keep operating on process CWD.
// ErrorText keeps git's own message on result records: stderr when present, otherwise stdout.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for Git operations, providing strongly-typed access to common git utilities.
/// </summary>
public static partial class Git
{
  private static ShellBuilder GitBuilder(string? repositoryPath, params string[] arguments)
  {
    ShellBuilder builder = Shell.Builder("git")
      .WithArguments(arguments)
      .WithNoValidation();

    if (!string.IsNullOrWhiteSpace(repositoryPath))
    {
      builder = builder.WithWorkingDirectory(repositoryPath);
    }

    return builder;
  }

  private static string ErrorText(CommandOutput result)
  {
    string errorMessage = string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stdout
      : result.Stderr;

    return errorMessage.Trim();
  }

  private static string ErrorTextOr(CommandOutput result, string fallback)
  {
    string errorText = ErrorText(result);
    return string.IsNullOrWhiteSpace(errorText) ? fallback : errorText;
  }
}
