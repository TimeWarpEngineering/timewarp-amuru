#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Provides thread-safe command mocking for testing without dependency injection.
/// Uses AsyncLocal for test isolation and automatic cleanup.
/// </summary>
public static class CommandMock
{
  private static readonly AsyncLocal<MockState?> CurrentMockState = new();
  
  /// <summary>
  /// Gets whether mocking is currently enabled for this async context.
  /// </summary>
  internal static bool IsEnabled => CurrentMockState.Value != null;
  
  /// <summary>
  /// Gets the current mock state for this async context.
  /// </summary>
  internal static MockState? State => CurrentMockState.Value;
  
  /// <summary>
  /// Enables command mocking for the current test scope.
  /// Returns an IDisposable that automatically cleans up when disposed.
  /// </summary>
  /// <remarks>
  /// By default the scope is <see cref="MockBehavior.Strict"/>: any command executed without a
  /// matching setup throws instead of running the real process. Pass <see cref="MockBehavior.Loose"/>
  /// to allow unmocked commands to execute for mixed mocked/real tests.
  /// Note: <c>Pipe</c> compositions bypass mocking entirely (and throw under strict mode) — use
  /// loose mode when testing pipelines.
  /// Dispose the returned scope in the same async context that called Enable(); disposing from a
  /// different context cannot clear the originating context's state (AsyncLocal semantics).
  /// </remarks>
  /// <param name="behavior">How executions without a matching setup are handled</param>
  /// <returns>A disposable scope that cleans up mocking when disposed</returns>
  public static IDisposable Enable(MockBehavior behavior = MockBehavior.Strict)
  {
    if (CurrentMockState.Value != null)
    {
      throw new InvalidOperationException("CommandMock is already enabled in this context. Did you forget to dispose a previous mock?");
    }

    MockState newState = new(behavior);
    CurrentMockState.Value = newState;
    return new MockScope(() => CurrentMockState.Value = null);
  }
  
  /// <summary>
  /// Sets up a mock for a specific command and arguments.
  /// </summary>
  /// <param name="executable">The command to mock</param>
  /// <param name="arguments">The expected arguments (exact match)</param>
  /// <returns>A fluent builder for configuring the mock behavior</returns>
  public static MockSetup Setup(string executable, params string[] arguments)
  {
    MockState? state = CurrentMockState.Value;
    if (state == null)
    {
      throw new InvalidOperationException("CommandMock is not enabled. Call CommandMock.Enable() first.");
    }
    
    return new MockSetup(state, executable, arguments);
  }
  
  /// <summary>
  /// Verifies that a command was called with specific arguments.
  /// </summary>
  /// <param name="executable">The command that should have been called</param>
  /// <param name="arguments">The expected arguments</param>
  /// <exception cref="InvalidOperationException">If the command was not called</exception>
  public static void VerifyCalled(string executable, params string[] arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    
    MockState? state = CurrentMockState.Value;
    if (state == null)
    {
      throw new InvalidOperationException("CommandMock is not enabled. Cannot verify calls.");
    }
    
    if (!state.WasCalled(executable, arguments))
    {
      string argString = arguments.Length > 0 ? string.Join(" ", arguments) : "(no arguments)";
      throw new InvalidOperationException($"Expected command '{executable} {argString}' was not called.");
    }
  }
  
  /// <summary>
  /// Gets the number of times a command was called with specific arguments.
  /// </summary>
  /// <param name="executable">The command to check</param>
  /// <param name="arguments">The arguments to match</param>
  /// <returns>The number of times the command was called</returns>
  public static int CallCount(string executable, params string[] arguments)
  {
    MockState? state = CurrentMockState.Value;
    if (state == null)
    {
      return 0;
    }
    
    return state.GetCallCount(executable, arguments);
  }
  
  /// <summary>
  /// Resets all mock setups and call history.
  /// </summary>
  public static void Reset()
  {
    MockState? state = CurrentMockState.Value;
    state?.Reset();
  }
}