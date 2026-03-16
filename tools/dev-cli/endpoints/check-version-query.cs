#region Purpose
// Check-version query - verifies if current version exists on NuGet.org
#endregion

using System.Diagnostics;
using System.Xml.Linq;
using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("check-version", Description = "Check if current version exists on NuGet.org")]
public sealed class CheckVersionQuery : IQuery<Unit>
{
  public sealed class Handler : IQueryHandler<CheckVersionQuery, Unit>
  {
    public async ValueTask<Unit> Handle(CheckVersionQuery query, CancellationToken cancellationToken)
    {
      Console.WriteLine("🔍 Checking if version exists on NuGet.org...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Console.WriteLine("❌ Not in a git repository");
        Environment.Exit(2);
      }

      string buildPropsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");

      if (!File.Exists(buildPropsPath))
      {
        Console.WriteLine($"❌ Directory.Build.props not found: {buildPropsPath}");
        Environment.Exit(2);
        return Unit.Value;
      }

      string version;
      try
      {
        XDocument doc = XDocument.Load(buildPropsPath);
        version = doc.Descendants("Version").FirstOrDefault()?.Value ?? "";

        if (string.IsNullOrWhiteSpace(version))
        {
          Console.WriteLine("❌ No <Version> element found in Directory.Build.props");
          Environment.Exit(2);
          return Unit.Value;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error parsing Directory.Build.props: {ex.Message}");
        Environment.Exit(2);
        return Unit.Value;
      }

      Console.WriteLine($"📦 Current version: {version}");

      string arguments = "package search TimeWarp.Amuru --exact-match --prerelease --source https://api.nuget.org/v3/index.json";
      string output = "";
      string error = "";
      int exitCode = 0;

      try
      {
        using Process process = new();
        process.StartInfo = new ProcessStartInfo
        {
          FileName = "dotnet",
          Arguments = arguments,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true
        };

        process.Start();
        output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        exitCode = process.ExitCode;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error running dotnet package search: {ex.Message}");
        Environment.Exit(2);
        return Unit.Value;
      }

      if (exitCode != 0)
      {
        Console.WriteLine($"⚠️  dotnet package search failed with exit code {exitCode}");
        if (!string.IsNullOrEmpty(error))
        {
          Console.Error.WriteLine(error);
        }
        Environment.Exit(2);
        return Unit.Value;
      }

      bool versionExists = output.Contains($"| TimeWarp.Amuru | {version}", StringComparison.OrdinalIgnoreCase) ||
                           output.Contains($"| {version} |", StringComparison.OrdinalIgnoreCase);

      if (!versionExists)
      {
        string[] lines = output.Split('\n');
        foreach (string line in lines)
        {
          if (line.Contains("TimeWarp.Amuru", StringComparison.OrdinalIgnoreCase) &&
              line.Contains(version, StringComparison.OrdinalIgnoreCase))
          {
            versionExists = true;
            break;
          }
        }
      }

      if (versionExists)
      {
        Console.WriteLine($"❌ TimeWarp.Amuru {version} is already published on NuGet.org");
        Console.WriteLine("   This version cannot be published again.");
        Environment.Exit(1);
      }
      else
      {
        Console.WriteLine($"✅ TimeWarp.Amuru {version} is NOT on NuGet.org");
        Console.WriteLine("   Safe to publish!");
        Environment.Exit(0);
      }

      return Unit.Value;
    }
  }
}
