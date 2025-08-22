namespace TimeWarp.Amuru;

public sealed class ScriptContext : IDisposable
{
  private readonly string OriginalDirectory;
  private readonly Action? OnExit;
  private static ScriptContext? Current;

  public string ScriptDirectory { get; }
  public string ScriptFilePath { get; }

  private ScriptContext(string scriptFilePath, string scriptDirectory, Action? onExit = null)
  {
    OriginalDirectory = Directory.GetCurrentDirectory();
    ScriptFilePath = scriptFilePath;
    ScriptDirectory = scriptDirectory;
    OnExit = onExit;
    
    // Register for process exit events to ensure cleanup
    if (Current == null)
    {
      Current = this;
      AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }
  }

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
    Current?.Cleanup();
  }

  private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
  {
    Current?.Cleanup();
  }

  private void Cleanup()
  {
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

  public void Dispose()
  {
    if (Current == this)
    {
      Cleanup();
      AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
      AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
      Current = null;
    }
  }
}