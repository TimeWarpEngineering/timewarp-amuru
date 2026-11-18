#region Purpose
// Writes a captured command's output to the terminal for dev-cli commands.
#endregion

#region Design
// dev test and dev verify-samples run dotnet through the typed DotNet builders (task 150), capture
// the output, and print it here. The old RunProcessAsync took an executable name and a
// space-split argument string, which hid Shell.Builder("dotnet") from the Amuru analyzer.
#endregion

namespace DevCli;

public static class ProcessHelpers
{
  public static async Task<int> WriteOutputAsync(CommandOutput output)
  {
    ArgumentNullException.ThrowIfNull(output);

    if (!string.IsNullOrEmpty(output.Stdout))
      await TimeWarpTerminal.Default.WriteLineAsync(output.Stdout);

    if (!string.IsNullOrEmpty(output.Stderr))
      await TimeWarpTerminal.Default.WriteErrorLineAsync(output.Stderr);

    return output.ExitCode;
  }
}
