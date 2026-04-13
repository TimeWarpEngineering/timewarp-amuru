#region Purpose
// Represents the result of a command execution with output capture
// Wraps CliWrap CommandResult and provides formatted output methods
#endregion

#region Design
// - Wraps CliWrap CommandResult to expose exit code, timing, and success status
// - Stores captured stdout and stderr as strings
// - Delegates properties (ExitCode, IsSuccess, timing) to underlying CliWrap result
// - Multiple output formats: ToString (one-line), ToSummary (compact), ToDetailedString (full)
// - WriteToConsole for direct terminal output with color formatting via TimeWarp.Terminal
// - Used by CaptureAsync and RunAndCaptureAsync to return execution results
#endregion

namespace TimeWarp.Amuru;

public class ExecutionResult
{
  private CliWrap.CommandResult Result { get; }
  public string StandardOutput { get; }
  public string StandardError { get; }
  
  public ExecutionResult(CliWrap.CommandResult result, string standardOutput, string standardError)
  {
    Result = result;
    StandardOutput = standardOutput;
    StandardError = standardError;
  }
  
  public int ExitCode => Result.ExitCode;
  public bool IsSuccess => Result.IsSuccess;
  public DateTimeOffset StartTime => Result.StartTime;
  public DateTimeOffset ExitTime => Result.ExitTime;
  public TimeSpan RunTime => Result.RunTime;
  
  public override string ToString()
  {
    string status = IsSuccess ? "Success" : "Failed";
    return $"[{status}] Exit: {ExitCode}, Runtime: {RunTime.TotalSeconds:F2}s";
  }
  
  /// <summary>
  /// Returns a one-line summary of the execution result.
  /// </summary>
  public string ToSummary() => 
    $"Exit: {ExitCode} | Runtime: {RunTime.TotalSeconds:F2}s | Output: {StandardOutput.Length} chars";
  
  /// <summary>
  /// Returns a detailed, multi-line string representation of the execution result.
  /// </summary>
  public string ToDetailedString()
  {
    StringBuilder sb = new();
    sb.AppendLine("=== Execution Result ===");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Status: {(IsSuccess ? "SUCCESS" : "FAILED")}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Exit Code: {ExitCode}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"Runtime: {RunTime}");
    
    if (!string.IsNullOrEmpty(StandardOutput))
    {
      sb.AppendLine("\nStandard Output:");
      sb.AppendLine(StandardOutput);
    }
    
    if (!string.IsNullOrEmpty(StandardError))
    {
      sb.AppendLine("\nStandard Error:");
      sb.AppendLine(StandardError);
    }
    
    return sb.ToString();
  }
  
  /// <summary>
  /// Writes the execution result to the terminal with appropriate formatting and colors.
  /// </summary>
  public void WriteToConsole()
  {
    string status = IsSuccess ? "SUCCESS".Green() : "FAILED".Red();
    TimeWarpTerminal.Default.WriteLine($"[{status}] Exit Code: {ExitCode}");
    
    if (!string.IsNullOrEmpty(StandardOutput))
    {
      TimeWarpTerminal.Default.WriteLine(StandardOutput);
    }
    
    if (!string.IsNullOrEmpty(StandardError))
    {
      TimeWarpTerminal.Default.WriteErrorLine(StandardError.Yellow());
    }
  }
}
