#region Purpose
// Shared Commands exception mapping to CommandOutput (stderr + non-zero exit).
#endregion

#region Design
// One mapper so new verbs share the shipped GetContent/RemoveItem error text shape.
// CA1031 is suppressed here: unexpected failures become exit 1, not thrown.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  private static CommandOutput Invoke(string operation, string path, Action action)
  {
    ArgumentNullException.ThrowIfNull(action);
    try
    {
      action();
      return new CommandOutput(string.Empty, string.Empty, 0);
    }
    catch (Exception exception)
    {
      return Fail(operation, path, exception);
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  private static CommandOutput Invoke(string operation, string path, Func<string> stdoutFactory)
  {
    ArgumentNullException.ThrowIfNull(stdoutFactory);
    try
    {
      return new CommandOutput(stdoutFactory(), string.Empty, 0);
    }
    catch (Exception exception)
    {
      return Fail(operation, path, exception);
    }
  }

  private static CommandOutput Fail(string operation, string path, Exception exception)
  {
    string message = exception switch
    {
      FileNotFoundException or DirectoryNotFoundException => "No such file or directory",
      UnauthorizedAccessException => "Permission denied",
      _ => exception.Message
    };

    return new CommandOutput(
      string.Empty,
      $"{operation}: {path}: {message}",
      1
    );
  }

  private static string FormatChildItem(FileSystemInfo entry, string displayName)
  {
    string type = entry is DirectoryInfo ? "d" : "-";
    string size = entry is FileInfo file
      ? file.Length.ToString(CultureInfo.InvariantCulture).PadLeft(10)
      : "<DIR>".PadLeft(10);
    return $"{type}  {size}  {entry.LastWriteTime:yyyy-MM-dd HH:mm}  {displayName}";
  }
}
