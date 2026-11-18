#region Purpose
// Entry point for `gh run` builders: list and download. Task 152.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Starts a <c>gh run</c> command (GitHub Actions workflow runs).
  /// </summary>
  /// <returns>The run sub-commands</returns>
  public static GhRunCommand Run() => new("run");
}

/// <summary>
/// The <c>gh run</c> sub-commands.
/// </summary>
public sealed class GhRunCommand
{
  private readonly string Verb;

  internal GhRunCommand(string verb)
  {
    Verb = verb;
  }

  /// <summary>
  /// Creates a builder for <c>gh run list</c>.
  /// </summary>
  /// <returns>A builder for listing workflow runs</returns>
  public GhRunListBuilder List() => new($"{Verb} list");

  /// <summary>
  /// Creates a builder for <c>gh run download</c> with no run id (gh prompts or uses the latest).
  /// </summary>
  /// <returns>A builder for downloading run artifacts</returns>
  public GhRunDownloadBuilder Download() => new($"{Verb} download", null);

  /// <summary>
  /// Creates a builder for <c>gh run download &lt;run-id&gt;</c>.
  /// </summary>
  /// <param name="runId">The workflow run id</param>
  /// <returns>A builder for downloading run artifacts</returns>
  public GhRunDownloadBuilder Download(string runId) => new($"{Verb} download", runId);
}
