#region Purpose
// Helper methods for running external processes asynchronously
// Used by dev-cli for bootstrapping - cannot depend on TimeWarp.Amuru
#endregion

#region Design
// Dev-cli intentionally uses raw System.Diagnostics.Process instead of TimeWarp.Amuru:
// - Prevents circular dependency (dev-cli builds the project which includes TimeWarp.Amuru)
// - Tool must be self-contained for bootstrapping the build system
// - See AGENTS.md: "Build scripts avoid circular dependency"
#endregion

#pragma warning disable RS0030 // ProcessStartInfo is allowed in dev-cli for bootstrapping

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

#pragma warning restore RS0030
