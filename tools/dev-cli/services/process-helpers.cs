#region Purpose
// Helper methods for running external processes asynchronously
#endregion

using System.Diagnostics;

namespace DevCli;

public static class ProcessHelpers
{
  public static async Task<int> RunProcessAsync(string fileName, string arguments)
  {
    using Process process = new();
    process.StartInfo = new ProcessStartInfo
    {
      FileName = fileName,
      Arguments = arguments,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };

    process.Start();
    string output = await process.StandardOutput.ReadToEndAsync();
    string error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    if (!string.IsNullOrEmpty(output)) await TimeWarpTerminal.Default.WriteLineAsync(output);
    if (!string.IsNullOrEmpty(error)) await TimeWarpTerminal.Default.WriteErrorLineAsync(error);

    return process.ExitCode;
  }
}
