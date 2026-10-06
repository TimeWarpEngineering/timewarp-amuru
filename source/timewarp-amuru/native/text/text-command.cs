#region Purpose
// Maps text-command failures onto CommandOutput without throwing.
#endregion

#region Design
// SelectString uses grep's exit codes: 0 matches, 1 none, 2 error.
// ReplaceInFiles uses sed's contract: 0 even when nothing matched, 1 on error.
// Argument and regex failures omit the path; I/O failures include it.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextCommand
{
  public const int NoMatchExitCode = 1;
  public const int ErrorExitCode = 2;

  public static CommandOutput Fail(string operation, string? path, Exception exception, int exitCode)
  {
    ArgumentNullException.ThrowIfNull(exception);
    string detail = exception switch
    {
      FileNotFoundException or DirectoryNotFoundException => "No such file or directory",
      UnauthorizedAccessException => "Permission denied",
      _ => exception.Message
    };

    string stderr = exception is ArgumentException || string.IsNullOrEmpty(path)
      ? $"{operation}: {detail}"
      : $"{operation}: {path}: {detail}";

    return new CommandOutput(string.Empty, stderr, exitCode);
  }

  public static string DescribeIo(Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    return exception switch
    {
      FileNotFoundException or DirectoryNotFoundException => "No such file or directory",
      UnauthorizedAccessException => "Permission denied",
      _ => exception.Message
    };
  }
}
