namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Fluent builder for configuring mock command behavior.
/// </summary>
public sealed class MockSetup
{
  private readonly MockState state;
  private readonly string executable;
  private readonly string[] arguments;
  private readonly MockSetupData setupData = new();
  
  internal MockSetup(MockState state, string executable, string[] arguments)
  {
    this.state = state;
    this.executable = executable;
    this.arguments = arguments;
  }
  
  /// <summary>
  /// Configures the mock to return specific output and exit code.
  /// </summary>
  /// <param name="stdout">The stdout output to return</param>
  /// <param name="stderr">The stderr output to return (optional)</param>
  /// <param name="exitCode">The exit code to return (default 0)</param>
  /// <returns>This instance for method chaining</returns>
  public MockSetup Returns(string stdout, string? stderr = null, int exitCode = 0)
  {
    setupData.Stdout = stdout;
    setupData.Stderr = stderr ?? string.Empty;
    setupData.ExitCode = exitCode;
    state.AddSetup(executable, arguments, setupData);
    return this;
  }
  
  /// <summary>
  /// Configures the mock to return an error.
  /// </summary>
  /// <param name="stderr">The error message to return on stderr</param>
  /// <param name="exitCode">The exit code (default 1)</param>
  /// <returns>This instance for method chaining</returns>
  public MockSetup ReturnsError(string stderr, int exitCode = 1)
  {
    setupData.Stdout = string.Empty;
    setupData.Stderr = stderr;
    setupData.ExitCode = exitCode;
    state.AddSetup(executable, arguments, setupData);
    return this;
  }
  
  /// <summary>
  /// Configures the mock to throw an exception when called.
  /// </summary>
  /// <typeparam name="TException">The type of exception to throw</typeparam>
  /// <param name="message">The exception message (optional)</param>
  /// <returns>This instance for method chaining</returns>
  public MockSetup Throws<TException>(string? message = null) where TException : Exception, new()
  {
    if (message != null)
    {
      setupData.Exception = (TException)Activator.CreateInstance(typeof(TException), message)!;
    }
    else
    {
      setupData.Exception = new TException();
    }
    
    state.AddSetup(executable, arguments, setupData);
    return this;
  }
  
  /// <summary>
  /// Configures the mock to throw a specific exception instance.
  /// </summary>
  /// <param name="exception">The exception to throw</param>
  /// <returns>This instance for method chaining</returns>
  public MockSetup Throws(Exception exception)
  {
    setupData.Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    state.AddSetup(executable, arguments, setupData);
    return this;
  }
  
  /// <summary>
  /// Adds a delay before the mock returns its result.
  /// Useful for testing timeout scenarios.
  /// </summary>
  /// <param name="delay">The delay duration</param>
  /// <returns>This instance for method chaining</returns>
  public MockSetup Delays(TimeSpan delay)
  {
    setupData.Delay = delay;
    return this;
  }
}