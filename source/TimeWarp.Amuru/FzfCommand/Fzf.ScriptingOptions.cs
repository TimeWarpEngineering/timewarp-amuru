namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // Scripting Options
  /// <summary>
  /// Specifies the initial query string.
  /// </summary>
  /// <param name="query">Initial query</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithQuery(string query)
  {
    Arguments.Add($"--query={query}");
    return this;
  }

  /// <summary>
  /// Automatically selects the only match.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithSelect1()
  {
    Arguments.Add("--select-1");
    return this;
  }

  /// <summary>
  /// Exits immediately when there's no match.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithExit0()
  {
    Arguments.Add("--exit-0");
    return this;
  }

  /// <summary>
  /// Enables filter mode (non-interactive).
  /// </summary>
  /// <param name="filter">Filter string</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithFilter(string filter)
  {
    Arguments.Add($"--filter={filter}");
    return this;
  }

  /// <summary>
  /// Prints the query as the first line of output.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPrintQuery()
  {
    Arguments.Add("--print-query");
    return this;
  }

  /// <summary>
  /// Specifies keys that complete fzf.
  /// </summary>
  /// <param name="keys">Comma-separated list of keys</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithExpect(string keys)
  {
    Arguments.Add($"--expect={keys}");
    return this;
  }

  /// <summary>
  /// Reads input delimited by ASCII NUL characters.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithRead0()
  {
    Arguments.Add("--read0");
    return this;
  }

  /// <summary>
  /// Prints output delimited by ASCII NUL characters.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithPrint0()
  {
    Arguments.Add("--print0");
    return this;
  }

  /// <summary>
  /// Enables synchronous search for multi-staged filtering.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithSync()
  {
    Arguments.Add("--sync");
    return this;
  }

  /// <summary>
  /// Starts HTTP server on default port.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithListen()
  {
    Arguments.Add("--listen");
    return this;
  }

  /// <summary>
  /// Starts HTTP server on specified port.
  /// </summary>
  /// <param name="port">Port number</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithListen(int port)
  {
    Arguments.Add($"--listen={port}");
    return this;
  }

  /// <summary>
  /// Starts HTTP server on specified address and port.
  /// </summary>
  /// <param name="address">Address and port</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithListen(string address)
  {
    Arguments.Add($"--listen={address}");
    return this;
  }
}