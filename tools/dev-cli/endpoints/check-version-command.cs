#region Purpose
// Check if the source version matches the git tag
#endregion
#region Design
// Reads version from source/Directory.Build.props and compares to git tag
#endregion

using System.Xml.Linq;

namespace DevCli.Commands;

[NuruRoute("check-version", Description = "Verify version matches git tag")]
internal sealed class CheckVersionCommand : ICommand<Unit>
{
  [Option("tag", Description = "Git tag to verify against (defaults to GITHUB_REF_NAME or git describe)")]
  public string? Tag { get; set; }

  internal sealed class Handler : ICommandHandler<CheckVersionCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(CheckVersionCommand command, CancellationToken ct)
    {
      ArgumentNullException.ThrowIfNull(command);

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Terminal.WriteErrorLine("❌ Not in a git repository");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      // Get version from source/Directory.Build.props
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      if (!File.Exists(propsPath))
      {
        Terminal.WriteErrorLine($"❌ Version props not found: {propsPath}");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      XDocument doc = XDocument.Load(propsPath);
      string? sourceVersion = doc.Descendants("Version").FirstOrDefault()?.Value;

      if (string.IsNullOrEmpty(sourceVersion))
      {
        Terminal.WriteErrorLine("❌ Could not find <Version> in Directory.Build.props");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      // Get git tag
      string? gitTag = command.Tag;
      
      if (string.IsNullOrEmpty(gitTag))
      {
        // Try GITHUB_REF_NAME first (set by GitHub Actions for releases)
        gitTag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        
        // Remove 'v' prefix if present
        if (!string.IsNullOrEmpty(gitTag) && gitTag.StartsWith('v'))
        {
          gitTag = gitTag[1..];
        }
      }

      if (string.IsNullOrEmpty(gitTag))
      {
        // Fall back to git describe
        CommandOutput result = await Shell.Builder("git")
          .WithArguments("describe", "--tags", "--abbrev=0")
          .CaptureAsync(ct);
        
        gitTag = result.Stdout.Trim();
        if (!string.IsNullOrEmpty(gitTag) && gitTag.StartsWith('v'))
        {
          gitTag = gitTag[1..];
        }
      }

      Terminal.WriteLine($"Source version: {sourceVersion}");
      Terminal.WriteLine($"Git tag:        {gitTag}");

      if (sourceVersion == gitTag)
      {
        Terminal.WriteLine("✅ Version matches git tag");
        return Unit.Value;
      }
      else
      {
        Terminal.WriteErrorLine($"❌ Version mismatch: source={sourceVersion}, tag={gitTag}");
        Environment.ExitCode = 1;
        return Unit.Value;
      }
    }
  }
}
