#region Purpose
// Unified bash-style aliases over Native.FileSystem Commands and Direct APIs.
#endregion

#region Design
// Thin renames so scripts can `global using static` bash names.
// Rm keeps separate recursive/force bools for 1.0 compatibility; a flags enum is deferred.
// New verbs (Cp/Mv/Mkdir/Touch/Test/Find/Stat) wrap the 1.1 FileSystem operations.
#endregion

namespace TimeWarp.Amuru.Native.Aliases;

/// <summary>
/// Unified bash-style aliases for all native commands.
/// Provides familiar command names for users coming from bash/Unix environments.
/// Use with: global using static TimeWarp.Amuru.Native.Aliases.Bash;
/// </summary>
public static class Bash
{
  // ===== File System Operations =====
  
  /// <summary>
  /// Reads file content (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Cat(string path) =>
    FileSystem.Commands.GetContent(path);

  /// <summary>
  /// Lists directory contents (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Ls(string path = ".") =>
    FileSystem.Commands.GetChildItem(path);

  /// <summary>
  /// Gets current working directory (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Pwd() =>
    FileSystem.Commands.GetLocation();

  /// <summary>
  /// Changes current directory (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Cd(string path) =>
    FileSystem.Commands.SetLocation(path);

  /// <summary>
  /// Removes a file or directory (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Rm(string path, bool recursive = false, bool force = false) =>
    FileSystem.Commands.RemoveItem(path, recursive, force);

  /// <summary>
  /// Copies a file or directory (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Cp(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true) =>
    FileSystem.Commands.CopyItem(source, destination, recursive, overwrite, preserveAttributes);

  /// <summary>
  /// Moves or renames a file or directory (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Mv(string source, string destination, bool overwrite = false) =>
    FileSystem.Commands.MoveItem(source, destination, overwrite);

  /// <summary>
  /// Creates a directory and any missing parents (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Mkdir(string path) =>
    FileSystem.Commands.Mkdir(path);

  /// <summary>
  /// Creates an empty file or updates its last-write time (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Touch(string path) =>
    FileSystem.Commands.Touch(path);

  /// <summary>
  /// Tests whether a path exists (Commands version - exit 0 when it exists).
  /// </summary>
  public static CommandOutput Test(string path) =>
    FileSystem.Commands.TestPath(path);

  /// <summary>
  /// Tests whether a path exists as the specified item type (Commands version).
  /// </summary>
  public static CommandOutput Test(string path, FileSystem.ItemType itemType) =>
    FileSystem.Commands.TestPath(path, itemType);

  /// <summary>
  /// Recursively finds file-system entries (Commands version - paths on stdout).
  /// </summary>
  public static CommandOutput Find(string path, FileSystem.FindCriteria? criteria = null) =>
    FileSystem.Commands.FindItem(path, criteria);

  /// <summary>
  /// Reads file-system metadata (Commands version - returns CommandOutput).
  /// </summary>
  public static CommandOutput Stat(string path) =>
    FileSystem.Commands.GetItemProperty(path);

  // ===== Direct API Overloads for Advanced Usage =====
  // Note: These have different return types, so they can coexist with Commands versions

  /// <summary>
  /// Reads file content (Direct version - returns IAsyncEnumerable for streaming).
  /// Use this for large files or LINQ operations.
  /// </summary>
  public static IAsyncEnumerable<string> CatDirect(string path) =>
    FileSystem.Direct.GetContent(path);

  /// <summary>
  /// Lists directory contents (Direct version - returns IAsyncEnumerable for streaming).
  /// Use this for LINQ operations on directory entries.
  /// </summary>
  public static IAsyncEnumerable<FileSystemInfo> LsDirect(string path = ".") =>
    FileSystem.Direct.GetChildItem(path);

  /// <summary>
  /// Gets current working directory (Direct version - returns string directly).
  /// </summary>
  public static string PwdDirect() =>
    FileSystem.Direct.GetLocation();

  /// <summary>
  /// Changes current directory (Direct version - throws exceptions on failure).
  /// </summary>
  public static void CdDirect(string path) =>
    FileSystem.Direct.SetLocation(path);

  /// <summary>
  /// Removes a file or directory (Direct version - throws exceptions on failure).
  /// </summary>
  public static void RmDirect(string path, bool recursive = false, bool force = false) =>
    FileSystem.Direct.RemoveItem(path, recursive, force);

  /// <summary>
  /// Copies a file or directory (Direct version - throws exceptions on failure).
  /// </summary>
  public static void CpDirect(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true) =>
    FileSystem.Direct.CopyItem(source, destination, recursive, overwrite, preserveAttributes);

  /// <summary>
  /// Moves or renames a file or directory (Direct version - throws exceptions on failure).
  /// </summary>
  public static void MvDirect(string source, string destination, bool overwrite = false) =>
    FileSystem.Direct.MoveItem(source, destination, overwrite);

  /// <summary>
  /// Creates a directory and any missing parents (Direct version).
  /// </summary>
  public static FileSystemInfo MkdirDirect(string path) =>
    FileSystem.Direct.Mkdir(path);

  /// <summary>
  /// Creates an empty file or updates its last-write time (Direct version).
  /// </summary>
  public static FileSystemInfo TouchDirect(string path) =>
    FileSystem.Direct.Touch(path);

  /// <summary>
  /// Tests whether a path exists (Direct version - returns bool).
  /// </summary>
  public static bool TestDirect(string path) =>
    FileSystem.Direct.TestPath(path);

  /// <summary>
  /// Recursively finds file-system entries (Direct version - streams FileSystemInfo).
  /// </summary>
  public static IAsyncEnumerable<FileSystemInfo> FindDirect(
    string path,
    FileSystem.FindCriteria? criteria = null) =>
    FileSystem.Direct.FindItem(path, criteria);

  /// <summary>
  /// Reads file-system metadata (Direct version - returns ItemProperty).
  /// </summary>
  public static FileSystem.ItemProperty StatDirect(string path) =>
    FileSystem.Direct.GetItemProperty(path);

  // ===== Future Text Operations (placeholders) =====
  
  // public static CommandOutput Grep(string pattern, string input) =>
  //   Text.Commands.SelectString(pattern, input);
  
  // public static CommandOutput Sed(string pattern, string replacement, string input) =>
  //   Text.Commands.ReplaceString(pattern, replacement, input);
  
  // public static CommandOutput Awk(string script, string input) =>
  //   Text.Commands.ProcessText(script, input);
  
  // ===== Future Process Operations (placeholders) =====
  
  // public static CommandOutput Ps() =>
  //   Process.Commands.GetProcess();
  
  // public static CommandOutput Kill(int pid) =>
  //   Process.Commands.StopProcess(pid);
}