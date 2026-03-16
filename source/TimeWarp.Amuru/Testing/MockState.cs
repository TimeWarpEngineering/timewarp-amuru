namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Internal state for command mocking, tracking setups and calls.
/// </summary>
internal sealed class MockState
{
  private readonly Dictionary<string, MockSetupData> setups = new();
  private readonly List<MockCall> calls = new();
  private readonly Lock syncLock = new();
  
  /// <summary>
  /// Adds a mock setup for a command.
  /// </summary>
  internal void AddSetup(string executable, string[] arguments, MockSetupData setupData)
  {
    using (syncLock.EnterScope())
    {
      string key = CreateKey(executable, arguments);
      setups[key] = setupData;
    }
  }
  
  /// <summary>
  /// Tries to get a mock setup for a command.
  /// </summary>
  internal bool TryGetSetup(string executable, string[] arguments, out MockSetupData? setupData)
  {
    using (syncLock.EnterScope())
    {
      string key = CreateKey(executable, arguments);
      return setups.TryGetValue(key, out setupData);
    }
  }
  
  /// <summary>
  /// Records that a command was called.
  /// </summary>
  internal void RecordCall(string executable, string[] arguments)
  {
    using (syncLock.EnterScope())
    {
      calls.Add(new MockCall(executable, arguments, DateTime.UtcNow));
    }
  }
  
  /// <summary>
  /// Checks if a command was called with specific arguments.
  /// </summary>
  internal bool WasCalled(string executable, string[] arguments)
  {
    using (syncLock.EnterScope())
    {
      string key = CreateKey(executable, arguments);
      return calls.Any(c => CreateKey(c.Executable, c.Arguments) == key);
    }
  }
  
  /// <summary>
  /// Gets the number of times a command was called.
  /// </summary>
  internal int GetCallCount(string executable, string[] arguments)
  {
    using (syncLock.EnterScope())
    {
      string key = CreateKey(executable, arguments);
      return calls.Count(c => CreateKey(c.Executable, c.Arguments) == key);
    }
  }
  
  /// <summary>
  /// Resets all setups and call history.
  /// </summary>
  internal void Reset()
  {
    using (syncLock.EnterScope())
    {
      setups.Clear();
      calls.Clear();
    }
  }
  
  private static string CreateKey(string executable, string[] arguments)
  {
    return $"{executable}|{string.Join("|", arguments)}";
  }
  
  /// <summary>
  /// Represents a recorded command call.
  /// </summary>
  private sealed record MockCall(string Executable, string[] Arguments, DateTime Timestamp);
}

/// <summary>
/// Data for a mock setup.
/// </summary>
internal sealed class MockSetupData
{
  public string? Stdout { get; set; }
  public string? Stderr { get; set; }
  public int ExitCode { get; set; }
  public Exception? Exception { get; set; }
  public TimeSpan? Delay { get; set; }
}