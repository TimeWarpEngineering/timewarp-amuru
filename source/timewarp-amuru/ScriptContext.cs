#region Purpose
// Scoped working-directory management for file-based app scripts: enter the script's
// (or a relative) directory on creation, restore the original directory on dispose or process exit.
#endregion

#region Design
// - Contexts nest: a stack tracks live instances so each Dispose restores its own original
//   directory and runs its own onExit, innermost first. Out-of-order dispose unwinds the stack
//   down to (and including) the disposed instance.
// - Process-exit and unhandled-exception handlers unwind the whole stack, best-effort.
// - All static state is guarded by a Lock; handlers are registered once while any context lives.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Establishes a working-directory scope for a file-based app script and restores the
/// original directory when disposed (or on process exit as a fallback).
/// </summary>
public sealed class ScriptContext : IDisposable
{
  private readonly string OriginalDirectory;
  private readonly Action? OnExit;
  private bool Disposed;

  private static readonly Lock SyncLock = new();
  private static readonly Stack<ScriptContext> LiveContexts = new();

  /// <summary>
  /// The directory containing the entry-point script.
  /// </summary>
  public string ScriptDirectory { get; }

  /// <summary>
  /// The full path of the entry-point script file.
  /// </summary>
  public string ScriptFilePath { get; }

  private ScriptContext(string scriptFilePath, string scriptDirectory, Action? onExit = null)
  {
    OriginalDirectory = Directory.GetCurrentDirectory();
    ScriptFilePath = scriptFilePath;
    ScriptDirectory = scriptDirectory;
    OnExit = onExit;

    using (SyncLock.EnterScope())
    {
      if (LiveContexts.Count == 0)
      {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
      }

      LiveContexts.Push(this);
    }
  }

  /// <summary>
  /// Creates a context for the current file-based app and, by default, changes
  /// the working directory to the script's directory.
  /// </summary>
  /// <param name="changeToScriptDirectory">Whether to change the working directory to the script directory</param>
  /// <param name="onExit">Optional callback invoked when the context is disposed or the process exits</param>
  /// <returns>The created context; dispose it to restore the original working directory</returns>
  /// <exception cref="InvalidOperationException">Not running as a file-based app</exception>
  public static ScriptContext FromEntryPoint(bool changeToScriptDirectory = true, Action? onExit = null)
  {
    string? scriptPath = AppContext.GetData("EntryPointFilePath") as string
      ?? throw new InvalidOperationException("Not running as file-based app");
    string? scriptDir = AppContext.GetData("EntryPointFileDirectoryPath") as string
      ?? throw new InvalidOperationException("Unable to determine script directory");

    ScriptContext context = new(scriptPath, scriptDir, onExit);

    if (changeToScriptDirectory)
      Directory.SetCurrentDirectory(scriptDir);

    return context;
  }

  /// <summary>
  /// Creates a context for the current file-based app and, by default, changes the
  /// working directory to a path relative to the script's directory.
  /// </summary>
  /// <param name="relativePath">Target directory relative to the script directory (default: parent)</param>
  /// <param name="changeToTargetDirectory">Whether to change the working directory to the target</param>
  /// <param name="onExit">Optional callback invoked when the context is disposed or the process exits</param>
  /// <returns>The created context; dispose it to restore the original working directory</returns>
  /// <exception cref="InvalidOperationException">Not running as a file-based app</exception>
  public static ScriptContext FromRelativePath(string relativePath = "..", bool changeToTargetDirectory = true, Action? onExit = null)
  {
    string? scriptDir = AppContext.GetData("EntryPointFileDirectoryPath") as string
      ?? throw new InvalidOperationException("Unable to determine script directory");
    string scriptPath = AppContext.GetData("EntryPointFilePath") as string ?? "";

    string targetDir = Path.GetFullPath(Path.Combine(scriptDir, relativePath));
    ScriptContext context = new(scriptPath, scriptDir, onExit);

    if (changeToTargetDirectory)
      Directory.SetCurrentDirectory(targetDir);

    return context;
  }

  private static void OnProcessExit(object? sender, EventArgs e)
  {
    UnwindAll();
  }

  private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
  {
    UnwindAll();
  }

  private static void UnwindAll()
  {
    using (SyncLock.EnterScope())
    {
      while (LiveContexts.Count > 0)
      {
        LiveContexts.Pop().Cleanup();
      }
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Process-exit/unhandled-exception cleanup path is best-effort; failures must not throw during teardown."
  )]
  private void Cleanup()
  {
    if (Disposed)
    {
      return;
    }

    Disposed = true;

    try
    {
      Directory.SetCurrentDirectory(OriginalDirectory);
      OnExit?.Invoke();
    }
    catch
    {
      // Best effort cleanup - don't throw in cleanup handlers
    }
  }

  /// <summary>
  /// Restores the working directory captured at creation and invokes the onExit callback.
  /// Nested contexts unwind innermost-first; disposing an outer context also unwinds
  /// any inner contexts still alive above it.
  /// </summary>
  public void Dispose()
  {
    using (SyncLock.EnterScope())
    {
      if (Disposed)
      {
        return;
      }

      // Unwind the stack down to and including this instance so each context
      // restores its own original directory in reverse creation order.
      while (LiveContexts.Count > 0)
      {
        ScriptContext top = LiveContexts.Pop();
        top.Cleanup();
        if (ReferenceEquals(top, this))
        {
          break;
        }
      }

      if (LiveContexts.Count == 0)
      {
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
      }
    }
  }
}
