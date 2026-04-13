#region Purpose
// Helper methods for running external processes asynchronously
// Provides simple wrappers around Shell.Builder for common dev-cli operations
#endregion

using System.Text;

namespace DevCli;

public static class ProcessHelpers
{
  public static async Task<int> RunProcessAsync(string fileName, string arguments)
  {
    string[] args = ParseArguments(arguments);

    CommandOutput output = await Shell.Builder(fileName)
      .WithArguments(args)
      .CaptureAsync();

    if (!string.IsNullOrEmpty(output.Stdout))
      await TimeWarpTerminal.Default.WriteLineAsync(output.Stdout);

    if (!string.IsNullOrEmpty(output.Stderr))
      await TimeWarpTerminal.Default.WriteErrorLineAsync(output.Stderr);

    return output.ExitCode;
  }

  private static string[] ParseArguments(string arguments)
  {
    if (string.IsNullOrWhiteSpace(arguments))
      return [];

    List<string> result = new();
    StringBuilder current = new();
    bool inQuotes = false;

    for (int i = 0; i < arguments.Length; i++)
    {
      char c = arguments[i];

      if (c == '"')
      {
        inQuotes = !inQuotes;
        continue;
      }

      if (c == ' ' && !inQuotes)
      {
        if (current.Length > 0)
        {
          result.Add(current.ToString());
          current.Clear();
        }

        continue;
      }

      current.Append(c);
    }

    if (current.Length > 0)
      result.Add(current.ToString());

    return result.ToArray();
  }
}
