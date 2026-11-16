#region Purpose
// Outcome types for the dev workflow release path (WorkflowCommand.Handler).
#endregion
#region Design
// Split from workflow-command.cs (task 147) to keep that file under its line cap.
// Each release gate returns a typed outcome with its own status enum, so the
// handler never collapses distinct verdicts into one message.
#endregion

namespace DevCli.Commands;

public sealed partial class WorkflowCommand
{
  internal sealed partial class Handler
  {
    // A packable project at its evaluated version (always the props version once the
    // lockstep gate passes); FileName is what the CI artifact must contain and what
    // gets pushed.
    private sealed record ReleasePackage(string PackageId, string Version)
    {
      public string FileName => $"{PackageId}.{Version}.nupkg";
    }

    private enum AncestorCheckStatus
    {
      Ancestor,
      NotAncestor,
      MasterUnresolvable,
      GitError
    }

    private sealed record AncestorCheckOutcome(AncestorCheckStatus Status, string? Detail);

    private enum TagPinStatus
    {
      NoTag,
      Match,
      Mismatch,
      GitError
    }

    private sealed record TagPinOutcome(TagPinStatus Status, string? TagCommit, string? HeadCommit, string? Detail);

    private enum LocateRunStatus
    {
      Found,
      GhUnavailable,
      GhFailed,
      NoMatchingRun
    }

    private sealed record LocateRunOutcome(LocateRunStatus Status, string HeadSha, IReadOnlyList<CiRunSummary> CandidateRuns, string? Detail);

    private enum DownloadArtifactStatus
    {
      Downloaded,
      Exhausted
    }

    private sealed record DownloadArtifactOutcome(DownloadArtifactStatus Status, CiRunSummary? Run, string? ArtifactName, IReadOnlyList<ExpiredArtifactEncounter> ExpiredEncounters);

    private sealed record ExpiredArtifactEncounter(long RunId, string Event, IReadOnlyList<string> ArtifactNames);
  }
}
