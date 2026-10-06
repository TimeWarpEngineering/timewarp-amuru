#region Purpose
// Scoped working-directory management for file-based app scripts: enter the script's
// (or a relative) directory on creation, restore the original directory on dispose or process exit.
#endregion

#region Design
// - Contexts nest: a stack tracks live instances so each Dispose restores its own original
//   directory and runs its own onExit, innermost first. Out-of-order dispose unwinds the stack
//   down to (and including) the disposed instance.
// - The working directory changes before the instance is pushed. A failed directory change
//   leaves the stack untouched, so a caller that never receives the instance cannot leak it.
// - Dispose and process-exit unwind pop and mark contexts under the lock, then restore
//   directories and invoke onExit outside the lock. onExit may construct or dispose another
//   ScriptContext without deadlocking.
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

    string? targetDirectory = changeToScriptDirectory ? scriptDir : null;
    return Activate(scriptPath, scriptDir, targetDirectory, onExit);
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
    string? targetDirectory = changeToTargetDirectory ? targetDir : null;
    return Activate(scriptPath, scriptDir, targetDirectory, onExit);
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Activation must not leave a live stack entry or a changed directory when directory change or registration fails."
  )]
  private static ScriptContext Activate(
    string scriptPath,
    string scriptDirectory,
    string? targetDirectory,
    Action? onExit)
  {
    ScriptContext context = new(scriptPath, scriptDirectory, onExit);
    try
    {
      if (targetDirectory is not null)
      {
        Directory.SetCurrentDirectory(targetDirectory);
      }

      context.PushLive();
      return context;
    }
    catch (Exception)
    {
      try
      {
        Directory.SetCurrentDirectory(context.OriginalDirectory);
      }
      catch
      {
        // The original failure is the one the caller must see.
      }

      context.AbandonIfPushed();
      throw;
    }
  }

  private void PushLive()
  {
    using (SyncLock.EnterScope())
    {
      LiveContexts.Push(this);
      if (LiveContexts.Count == 1)
      {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
      }
    }
  }

  private void AbandonIfPushed()
  {
    using (SyncLock.EnterScope())
    {
      if (Disposed)
      {
        return;
      }

      if (LiveContexts.Count > 0 && ReferenceEquals(LiveContexts.Peek(), this))
      {
        LiveContexts.Pop();
        Disposed = true;
        UnregisterHandlersIfIdle();
      }
    }
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
    List<ScriptContext> unwound = new();
    using (SyncLock.EnterScope())
    {
      while (LiveContexts.Count > 0)
      {
        ScriptContext top = LiveContexts.Pop();
        top.Disposed = true;
        unwound.Add(top);
      }

      UnregisterHandlersIfIdle();
    }

    FinishUnwind(unwound);
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Dispose and process-exit cleanup is best-effort; a failing onExit must not skip the rest of the unwind."
  )]
  private static void FinishUnwind(List<ScriptContext> unwound)
  {
    foreach (ScriptContext context in unwound)
    {
      try
      {
        Directory.SetCurrentDirectory(context.OriginalDirectory);
        context.OnExit?.Invoke();
      }
      catch
      {
        // Best effort cleanup - don't throw in cleanup handlers
      }
    }
  }

  /// <summary>
  /// Restores the working directory captured at creation and invokes the onExit callback.
  /// Nested contexts unwind innermost-first; disposing an outer context also unwinds
  /// any inner contexts still alive above it.
  /// </summary>
  public void Dispose()
  {
    List<ScriptContext> unwound = new();
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
        top.Disposed = true;
        unwound.Add(top);
        if (ReferenceEquals(top, this))
        {
          break;
        }
      }

      UnregisterHandlersIfIdle();
    }

    FinishUnwind(unwound);
  }

  private static void UnregisterHandlersIfIdle()
  {
    if (LiveContexts.Count != 0)
    {
      return;
    }

    AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
    AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
  }
}
