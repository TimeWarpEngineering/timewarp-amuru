#region Purpose
// Maps text-command failures onto CommandOutput without throwing.
#endregion

#region Design
// SelectString uses grep's exit codes: 0 matches, 1 none, 2 error.
// ReplaceInFiles uses sed's contract: 0 even when nothing matched, 1 on error.
// Argument and regex failures omit the path; I/O failures include it.
// An exception that carries a ReasonKey entry in Data already names its path in the
// message, so DescribeIo uses the bare reason to avoid printing the path twice.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextCommand
{
  public const int NoMatchExitCode = 1;
  public const int ErrorExitCode = 2;
  public const string ReasonKey = "TimeWarp.Amuru.Native.Text.Reason";

  public static CommandOutput Fail(string operation, string? path, Exception exception, int exitCode)
  {
    ArgumentNullException.ThrowIfNull(exception);
    string detail = DescribeIo(exception);

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
      _ when exception.Data[ReasonKey] is string reason => reason,
      _ => exception.Message
    };
  }

  public static bool IsFileError(Exception exception)
  {
    return exception is IOException or UnauthorizedAccessException or InvalidDataException;
  }
}
