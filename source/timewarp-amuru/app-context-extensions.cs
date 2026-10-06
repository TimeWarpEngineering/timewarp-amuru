#region Purpose
// Extension members on AppContext exposing .NET 10 file-based app entry-point metadata
// (script path and directory) as typed properties instead of string-keyed GetData calls.
#endregion

namespace TimeWarp.Amuru;

#pragma warning disable CA1034 // Nested types should not be visible
/// <summary>
/// Extension members on <see cref="AppContext"/> for .NET 10 file-based app (runfile) metadata.
/// </summary>
public static class AppContextExtensions
{
  extension(AppContext)
  {
    /// <summary>
    /// The full path of the entry-point script file, or null when not running as a file-based app.
    /// </summary>
    public static string? EntryPointFilePath()
      => AppContext.GetData("EntryPointFilePath") as string;

    /// <summary>
    /// The directory containing the entry-point script file, or null when not running as a file-based app.
    /// </summary>
    public static string? EntryPointFileDirectoryPath()
      => AppContext.GetData("EntryPointFileDirectoryPath") as string;
  }
}
#pragma warning restore CA1034
