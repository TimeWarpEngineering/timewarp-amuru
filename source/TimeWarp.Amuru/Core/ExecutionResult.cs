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
  /// Writes the execution result to the console with appropriate formatting and colors.
  /// </summary>
  public void WriteToConsole()
  {
    Console.ForegroundColor = IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
    Console.WriteLine($"[{(IsSuccess ? "SUCCESS" : "FAILED")}] Exit Code: {ExitCode}");
    Console.ResetColor();
    
    if (!string.IsNullOrEmpty(StandardOutput))
    {
      Console.WriteLine(StandardOutput);
    }
    
    if (!string.IsNullOrEmpty(StandardError))
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.WriteLine(StandardError);
      Console.ResetColor();
    }
  }
}