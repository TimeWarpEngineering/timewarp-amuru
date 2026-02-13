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

    if (!string.IsNullOrEmpty(output)) Console.WriteLine(output);
    if (!string.IsNullOrEmpty(error)) Console.Error.WriteLine(error);

    return process.ExitCode;
  }
}
