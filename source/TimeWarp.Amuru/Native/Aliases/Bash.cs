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