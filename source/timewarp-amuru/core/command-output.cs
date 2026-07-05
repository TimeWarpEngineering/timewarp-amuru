#region Purpose
// Represents the complete output from a command execution with lazy-computed views
// Provides thread-safe access to stdout, stderr, and combined output
#endregion

#region Design
// - Lazy-computed string properties (Stdout, Stderr, Combined) with double-checked locking
// - Thread-safe: uses lock(syncLock) for lazy initialization
// - Stores raw OutputLine list for flexible processing and line-level access
// - Factory method Empty() for creating empty outputs (failed commands, tests)
// - Two constructors: one from OutputLine list (streaming), one from strings (reconstruction)
// - Convenience methods GetLines/GetStdoutLines/GetStderrLines for line-array access
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the complete output from a command execution with lazy-computed views.
/// </summary>
public class CommandOutput
{
  private readonly List<OutputLine> lines;
  private readonly object syncLock = new();

  private string? stdout;
  private string? stderr;
  private string? combined;

  /// <summary>
  /// Gets the exit code of the command.
  /// </summary>
  public int ExitCode { get; }

  /// <summary>
  /// Gets whether the command succeeded (exit code is 0).
  /// </summary>
  public bool Success => ExitCode == 0;

  /// <summary>
  /// Gets how long the command ran. Zero when the command never ran
  /// (see <see cref="CommandResult.NeverRanExitCode"/>) or when produced by a mock.
  /// </summary>
  public TimeSpan RunTime { get; init; }

  /// <summary>
  /// Gets only the stdout output as a single string.
  /// This property is lazy-computed and thread-safe.
  /// </summary>
  public string Stdout
  {
    get
    {
      if (stdout == null)
      {
        lock (syncLock)
        {
          stdout ??= string.Join("\n",
            lines.Where(l => !l.IsError).Select(l => l.Text));
        }
      }

      return stdout;
    }
  }

  /// <summary>
  /// Gets only the stderr output as a single string.
  /// This property is lazy-computed and thread-safe.
  /// </summary>
  public string Stderr
  {
    get
    {
      if (stderr == null)
      {
        lock (syncLock)
        {
          stderr ??= string.Join("\n",
            lines.Where(l => l.IsError).Select(l => l.Text));
        }
      }

      return stderr;
    }
  }

  /// <summary>
  /// Gets the combined stdout and stderr output in the order they were produced.
  /// This property is lazy-computed and thread-safe.
  /// </summary>
  public string Combined
  {
    get
    {
      if (combined == null)
      {
        lock (syncLock)
        {
          combined ??= string.Join("\n",
            lines.Select(l => l.Text));
        }
      }

      return combined;
    }
  }

  /// <summary>
  /// Gets the output lines for direct access and processing.
  /// </summary>
  public IReadOnlyList<OutputLine> OutputLines => lines.AsReadOnly();

  /// <summary>
  /// Gets the combined output split into lines (convenience method).
  /// Interior blank lines are preserved; a trailing newline does not produce a final empty entry.
  /// </summary>
  public string[] GetLines() => SplitLines(Combined);

  /// <summary>
  /// Gets the stdout output split into lines.
  /// Interior blank lines are preserved; a trailing newline does not produce a final empty entry.
  /// </summary>
  public string[] GetStdoutLines() => SplitLines(Stdout);

  /// <summary>
  /// Gets the stderr output split into lines.
  /// Interior blank lines are preserved; a trailing newline does not produce a final empty entry.
  /// </summary>
  public string[] GetStderrLines() => SplitLines(Stderr);

  private static string[] SplitLines(string text)
  {
    if (string.IsNullOrEmpty(text))
    {
      return [];
    }

    string[] result = text.TrimEnd('\n').Split('\n');
    for (int i = 0; i < result.Length; i++)
    {
      result[i] = result[i].TrimEnd('\r');
    }

    return result;
  }

  /// <summary>
  /// Initializes a new instance of the CommandOutput class.
  /// </summary>
  /// <param name="outputLines">The captured output lines with metadata</param>
  /// <param name="exitCode">The exit code of the command</param>
  public CommandOutput(IReadOnlyList<OutputLine> outputLines, int exitCode)
  {
    lines = outputLines != null ? new List<OutputLine>(outputLines) : new List<OutputLine>();
    ExitCode = exitCode;
  }

  /// <summary>
  /// Initializes a new instance of the CommandOutput class with separate stdout and stderr.
  /// True interleaving cannot be reconstructed from two separate strings, so <see cref="Combined"/>
  /// and <see cref="OutputLines"/> order all stdout lines before all stderr lines.
  /// </summary>
  /// <param name="stdoutText">The stdout output as a single string</param>
  /// <param name="stderrText">The stderr output as a single string</param>
  /// <param name="exitCode">The exit code of the command</param>
  public CommandOutput(string stdoutText, string stderrText, int exitCode)
  {
    lines = new List<OutputLine>();

    if (!string.IsNullOrEmpty(stdoutText))
    {
      foreach (string line in stdoutText.Split('\n'))
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          lines.Add(new OutputLine(line, false));
        }
      }
    }

    if (!string.IsNullOrEmpty(stderrText))
    {
      foreach (string line in stderrText.Split('\n'))
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          lines.Add(new OutputLine(line, true));
        }
      }
    }

    ExitCode = exitCode;
  }

  /// <summary>
  /// Creates an empty CommandOutput for failed or null commands.
  /// </summary>
  /// <param name="exitCode">The exit code (defaults to 0)</param>
  /// <returns>A new CommandOutput with no output</returns>
  public static CommandOutput Empty(int exitCode = 0)
  {
    return new CommandOutput(new List<OutputLine>(), exitCode);
  }

  /// <summary>
  /// Returns a one-line status summary: success flag, exit code, and runtime.
  /// </summary>
  public override string ToString()
  {
    string status = Success ? "Success" : "Failed";
    return $"[{status}] Exit: {ExitCode}, Runtime: {RunTime.TotalSeconds:F2}s";
  }

  /// <summary>
  /// Returns a one-line summary of the execution result.
  /// </summary>
  public string ToSummary() =>
    $"Exit: {ExitCode} | Runtime: {RunTime.TotalSeconds:F2}s | Output: {Stdout.Length} chars";

  /// <summary>
  /// Returns a detailed, multi-line string representation of the execution result.
  /// </summary>
  public string ToDetailedString()
  {
    StringBuilder sb = new();
    sb.AppendLine("=== Execution Result ===");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Status: {(Success ? "SUCCESS" : "FAILED")}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Exit Code: {ExitCode}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Runtime: {RunTime}");

    if (!string.IsNullOrEmpty(Stdout))
    {
      sb.AppendLine("\nStandard Output:");
      sb.AppendLine(Stdout);
    }

    if (!string.IsNullOrEmpty(Stderr))
    {
      sb.AppendLine("\nStandard Error:");
      sb.AppendLine(Stderr);
    }

    return sb.ToString();
  }

  /// <summary>
  /// Writes the result to the terminal with status coloring: exit line, stdout, and stderr (yellow).
  /// </summary>
  public void WriteToConsole()
  {
    string status = Success ? "SUCCESS".Green() : "FAILED".Red();
    TimeWarpTerminal.Default.WriteLine($"[{status}] Exit Code: {ExitCode}");

    if (!string.IsNullOrEmpty(Stdout))
    {
      TimeWarpTerminal.Default.WriteLine(Stdout);
    }

    if (!string.IsNullOrEmpty(Stderr))
    {
      TimeWarpTerminal.Default.WriteErrorLine(Stderr.Yellow());
    }
  }
}