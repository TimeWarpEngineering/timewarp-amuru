#region Purpose
// Defines how CommandMock treats command executions that have no matching setup.
#endregion

namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Controls how <see cref="CommandMock"/> handles commands executed without a matching setup.
/// </summary>
public enum MockBehavior
{
  /// <summary>
  /// Unmocked commands throw <see cref="InvalidOperationException"/> instead of executing.
  /// This is the default: a typo in a setup can never silently run a real process inside a test.
  /// </summary>
  Strict,

  /// <summary>
  /// Unmocked commands fall through and execute the real process.
  /// Use for mixed tests that intentionally combine mocked and real commands.
  /// </summary>
  Loose,
}
