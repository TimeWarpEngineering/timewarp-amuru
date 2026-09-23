#region Purpose
// Dev CLI command for the TimeWarp.Amuru CI/CD pipeline
#endregion
#region Design
// Orchestrates the full CI/CD pipeline with mode detection (CiModeDetector from
// TimeWarp.Nuru.DevCli): explicit --mode wins, else GITHUB_EVENT_NAME maps
// pull_request -> pr, push -> merge, release -> release, workflow_dispatch -> merge
// (a manual dispatch never publishes by default; break-glass release requires the
// explicit --mode release that workflow.yml wires behind a confirm input).
//
// Modes:
//   pr/merge:  clean -> build -> verify-samples -> test
//   release:   tag-gate -> check-version -> locate-run -> download-artifact -> verify -> push
//              (no rebuild — promotes the exact .nupkg set that the green master push
//              CI run already built, tested and uploaded for this commit; the org
//              convention from timewarp-nuru task 458-002)
//
// One version, lockstep: every packable project (TimeWarp.Amuru and
// TimeWarp.Amuru.Tools) ships at the single <Version> in source/Directory.Build.props,
// the release SSOT the tag derives from. The packable set is derived from IsPackable
// via MSBuild evaluation, and each project's effective <Version> is evaluated the
// same way so the verify step can refuse any project that drifts from the props
// version (a stray per-csproj <Version>). check-version and `dev release` guard the
// whole derived set. The push uses --skip-duplicate only so a partial-publish resume
// (same commit, tag-pinned) is idempotent for packages already on the feed.
//
// The timewarp-software rebuild dispatch after a successful push is best-effort: a
// failure must never fail a release that already reached NuGet (the site rebuilds
// nightly). Cross-repo repository_dispatch cannot use the default GITHUB_TOKEN, so
// workflow.yml mints a short-lived GitHub App installation token and passes it as
// REBUILD_DISPATCH_TOKEN; GH_TOKEN stays the job token so locate-run and
// download-artifact can read this repo's Actions artifacts.
#endregion

namespace DevCli.Commands;

using System.ComponentModel;
using System.Globalization;
using DevCli.Endpoints;

[NuruRoute("workflow", Description = "Run full CI/CD pipeline")]
internal sealed class WorkflowCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME if not specified)")]
  public string? Mode { get; set; }

  [Option("api-key", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private const string ArtifactsSubPath = "artifacts/packages";

    private readonly ITerminal Terminal;
    private readonly IRepoCleanService RepoCleanService;
    private readonly NuGetVersionService NuGetVersionService;
    private readonly IRepoConfigService ConfigService;
    private readonly IPackableProjectService PackableProjectService;

    public Handler
    (
      ITerminal terminal,
      IRepoCleanService repoCleanService,
      NuGetVersionService nuGetVersionService,
      IRepoConfigService configService,
      IPackableProjectService packableProjectService
    )
    {
      Terminal = terminal;
      RepoCleanService = repoCleanService;
      NuGetVersionService = nuGetVersionService;
      ConfigService = configService;
      PackableProjectService = packableProjectService;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      CiMode mode = CiModeDetector.DetermineMode(command.Mode, eventName);

      if (string.IsNullOrEmpty(command.Mode))
      {
        string displayEventName = eventName ?? "(not set)";
        Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      }

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  CI/CD Pipeline - Mode: {mode}");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("");

      if (mode == CiMode.Release)
      {
        await RunReleaseWorkflowAsync(command.ApiKey, ct);
      }
      else
      {
        await RunPrWorkflowAsync(ct);
      }

      return Value;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // pr / merge
    // ─────────────────────────────────────────────────────────────────────────

    private async Task RunPrWorkflowAsync(CancellationToken ct)
    {
      Terminal.WriteLine("Pipeline: clean -> build -> verify-samples -> test");
      Terminal.WriteLine("");

      Environment.ExitCode = 0;

      WriteStepBanner("Step 1/4: Clean");
      CleanCommand.Handler cleanHandler = new(Terminal, RepoCleanService);
      await cleanHandler.Handle(new CleanCommand(), ct);

      if (StopOnFailure("Clean"))
      {
        return;
      }

      WriteStepBanner("Step 2/4: Build");
      BuildCommand.Handler buildHandler = new();
      await buildHandler.Handle(new BuildCommand(), ct);

      if (StopOnFailure("Build"))
      {
        return;
      }

      WriteStepBanner("Step 3/4: Verify Samples");
      VerifySamplesCommand.Handler verifySamplesHandler = new(Terminal);
      await verifySamplesHandler.Handle(new VerifySamplesCommand(), ct);

      if (StopOnFailure("Verify Samples"))
      {
        return;
      }

      WriteStepBanner("Step 4/4: Test");
      TestCommand.Handler testHandler = new();
      await testHandler.Handle(new TestCommand(), ct);

      if (StopOnFailure("Test"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED");
      Terminal.WriteLine("===============================================================================");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // release (promote, never rebuild)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task RunReleaseWorkflowAsync(string? apiKey, CancellationToken ct)
    {
      Terminal.WriteLine("Pipeline: tag-gate -> check-version -> locate-run -> download-artifact -> verify -> push");
      Terminal.WriteLine("");

      Environment.ExitCode = 0;

      string? repoRoot = Git.FindRoot();
      if (repoRoot is null)
      {
        Terminal.WriteErrorLine("Error: could not find repository root.");
        AbortPipeline("repository root not found");
        return;
      }

      // Step 1: Release Gate — tag assertion (release event only), tag pin, ancestor-of-master
      WriteStepBanner("Step 1/6: Release Gate — Tag Assertions");

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");
      string? propsVersion = ReadPropsVersion(repoRoot);

      if (eventName == "release")
      {
        string? refName = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        TagAssertionResult tagResult = TagAssertion.Validate(refName, propsVersion);

        if (!tagResult.IsValid)
        {
          Terminal.WriteErrorLine($"Release gate failed: {tagResult.Error}");
          AbortPipeline("release tag does not match source version");
          return;
        }

        Terminal.WriteLine($"Tag assertion passed: {tagResult.ExpectedTag}");
      }
      else
      {
        Terminal.WriteLine("Tag assertion skipped: GITHUB_EVENT_NAME is not 'release' (break-glass/local release has no triggering ref tag to assert against; the tag-pin check below still applies).");
      }

      if (string.IsNullOrWhiteSpace(propsVersion))
      {
        Terminal.WriteErrorLine("Release gate failed: could not read <Version> from source/Directory.Build.props.");
        AbortPipeline("props version unreadable");
        return;
      }

      TagPinOutcome tagPinOutcome = await CheckTagPinAsync(propsVersion);
      string pinTag = $"v{propsVersion}";

      switch (tagPinOutcome.Status)
      {
        case TagPinStatus.NoTag:
          Terminal.WriteLine($"Tag pin: {pinTag} not yet tagged.");
          break;

        case TagPinStatus.Match:
          Terminal.WriteLine($"Tag pin passed: HEAD is at {pinTag}.");
          break;

        case TagPinStatus.Mismatch:
          Terminal.WriteErrorLine($"Release gate failed: tag {pinTag} already exists at commit {ShortSha(tagPinOutcome.TagCommit)}; this run is at {ShortSha(tagPinOutcome.HeadCommit)}. A partial-publish resume must run from the tag's commit (or bump the version if source changed).");
          AbortPipeline("tag pin mismatch");
          return;

        case TagPinStatus.GitError:
          Terminal.WriteErrorLine($"Release gate failed: tag pin check could not run — {tagPinOutcome.Detail}");
          AbortPipeline("tag pin check could not run");
          return;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled tag pin status '{tagPinOutcome.Status}'.");
          AbortPipeline("unhandled tag pin status");
          return;
      }

      AncestorCheckOutcome ancestorOutcome = await CheckHeadAncestorOfMasterAsync();

      switch (ancestorOutcome.Status)
      {
        case AncestorCheckStatus.NotAncestor:
          Terminal.WriteErrorLine("Release gate failed: current commit is not an ancestor of master. Releases must be cut from commits on master.");
          AbortPipeline("commit not on master");
          return;

        case AncestorCheckStatus.MasterUnresolvable:
          Terminal.WriteErrorLine("Release gate failed: cannot resolve origin/master or master — ensure the checkout has full history (fetch-depth: 0) and a master ref exists.");
          AbortPipeline("master ref unresolvable");
          return;

        case AncestorCheckStatus.GitError:
          Terminal.WriteErrorLine($"Release gate failed: ancestor check could not run — {ancestorOutcome.Detail}");
          AbortPipeline("ancestor check could not run");
          return;

        case AncestorCheckStatus.Ancestor:
          Terminal.WriteLine("Ancestor check passed: HEAD is on master.");
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled ancestor check status '{ancestorOutcome.Status}'.");
          AbortPipeline("unhandled ancestor check status");
          return;
      }

      // Step 2: Check Version (DevCli gate: None/Partial proceed, All aborts) + derive packable set
      WriteStepBanner("Step 2/6: Check Version");
      CheckVersionCommand.Handler checkVersionHandler = new(Terminal, NuGetVersionService, ConfigService, PackableProjectService);
      await checkVersionHandler.Handle(new CheckVersionCommand(), ct);

      if (Environment.ExitCode != 0)
      {
        AbortPipeline("version already released");
        return;
      }

      IReadOnlyList<PackableProject> packableProjects = await PackableProjectService
        .GetPackableProjectsAsync(repoRoot, ct)
        .ConfigureAwait(false);

      if (packableProjects.Count == 0)
      {
        Terminal.WriteErrorLine("Release gate failed: no packable projects found under source/.");
        AbortPipeline("no packable projects found");
        return;
      }

      List<ReleasePackage> releasePackages = [];
      foreach (PackableProject project in packableProjects)
      {
        string? projectVersion = await ResolveProjectVersionAsync(repoRoot, project.ProjectPath, ct);

        if (string.IsNullOrWhiteSpace(projectVersion))
        {
          Terminal.WriteErrorLine($"Release gate failed: could not evaluate <Version> for {project.PackageId} ({Path.GetRelativePath(repoRoot, project.ProjectPath)}).");
          AbortPipeline("package version unresolvable");
          return;
        }

        releasePackages.Add(new ReleasePackage(project.PackageId, projectVersion));
      }

      // Lockstep gate: every packable project must evaluate to the props version. A
      // per-csproj <Version> override would otherwise ship a second version silently.
      List<ReleasePackage> driftedPackages = [.. releasePackages.Where(p => !string.Equals(p.Version, propsVersion, StringComparison.Ordinal))];

      if (driftedPackages.Count > 0)
      {
        Terminal.WriteErrorLine($"Release gate failed: package version(s) differ from source/Directory.Build.props <Version> {propsVersion}: {string.Join(", ", driftedPackages.Select(p => $"{p.PackageId} {p.Version}"))}. All packages ship at one version — remove any <Version> from the csproj (or set IsPackable=false with a stated reason).");
        AbortPipeline("package version differs from props version");
        return;
      }

      Terminal.WriteLine($"Packable set ({releasePackages.Count}) at {propsVersion}: {string.Join(", ", releasePackages.Select(p => p.PackageId))}");

      // Step 3: Locate CI Run
      WriteStepBanner("Step 3/6: Locate CI Run");

      LocateRunOutcome locateOutcome = await LocateCiRunAsync();

      switch (locateOutcome.Status)
      {
        case LocateRunStatus.GhUnavailable:
          Terminal.WriteErrorLine("Release gate failed: release mode promotes CI-built artifacts and requires the gh CLI. On runners GH_TOKEN is provided by workflow.yml; locally install gh and run 'gh auth login'.");
          AbortPipeline("gh CLI unavailable");
          return;

        case LocateRunStatus.GhFailed:
          Terminal.WriteErrorLine($"Release gate failed: gh run list failed — {locateOutcome.Detail}. If this is transient (network/rate limit), retry; for auth issues run 'gh auth login'.");
          AbortPipeline("gh run list failed");
          return;

        case LocateRunStatus.NoMatchingRun:
          Terminal.WriteErrorLine($"Release gate failed: no successful CI run of workflow.yml exists for commit {locateOutcome.HeadSha}. Only tested CI artifacts are published — this commit must pass CI first. If a run failed, fix and re-run it (gh run rerun <run-id>).");
          AbortPipeline("no successful CI run found");
          return;

        case LocateRunStatus.Found:
          break;

        default:
          Terminal.WriteErrorLine($"Release gate failed: unhandled locate-run status '{locateOutcome.Status}'.");
          AbortPipeline("unhandled locate-run status");
          return;
      }

      Terminal.WriteLine($"Found {locateOutcome.CandidateRuns.Count} candidate CI run(s) for commit {ShortSha(locateOutcome.HeadSha)}.");

      // Step 4: Download Artifact
      WriteStepBanner("Step 4/6: Download Artifact");

      DownloadArtifactOutcome downloadOutcome = await DownloadPackagesArtifactAsync(repoRoot, locateOutcome.CandidateRuns);

      if (downloadOutcome.Status == DownloadArtifactStatus.Exhausted)
      {
        if (downloadOutcome.ExpiredEncounters.Count > 0)
        {
          string expiredDetail = string.Join("; ", downloadOutcome.ExpiredEncounters.Select(e => $"run {e.RunId} ({e.Event}): {string.Join(", ", e.ArtifactNames)}"));
          Terminal.WriteErrorLine($"Release gate failed: every candidate CI run's Packages-* artifact has expired — {expiredDetail}. Re-run CI to produce a fresh tested artifact (gh run rerun {downloadOutcome.ExpiredEncounters[0].RunId}).");
        }
        else
        {
          Terminal.WriteErrorLine($"Release gate failed: no candidate CI run for commit {locateOutcome.HeadSha} uploaded a Packages-* artifact. Re-run CI to produce one (gh run rerun {locateOutcome.CandidateRuns[0].DatabaseId}).");
        }

        AbortPipeline("no usable CI artifact found");
        return;
      }

      Terminal.WriteLine($"Downloaded '{downloadOutcome.ArtifactName}' from run {downloadOutcome.Run!.DatabaseId} ({downloadOutcome.Run.Event}).");

      // Step 5: Verify Package Set — downloaded file names must equal the derived set,
      // every package at the props version.
      WriteStepBanner("Step 5/6: Verify Package Set");

      string artifactsDir = Path.Combine(repoRoot, ArtifactsSubPath);
      string[] actualNupkgPaths = Directory.Exists(artifactsDir) ? Directory.GetFiles(artifactsDir, "*.nupkg") : [];
      List<string> actualFileNames = [.. actualNupkgPaths.Select(path => Path.GetFileName(path))];

      HashSet<string> expectedFileNames = new(releasePackages.Select(p => p.FileName), StringComparer.Ordinal);
      HashSet<string> actualFileNameSet = new(actualFileNames, StringComparer.Ordinal);
      List<string> missing = [.. expectedFileNames.Where(name => !actualFileNameSet.Contains(name))];
      List<string> unexpected = [.. actualFileNames.Where(name => !expectedFileNames.Contains(name))];

      if (missing.Count > 0 || unexpected.Count > 0)
      {
        if (missing.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact is missing package(s): {string.Join(", ", missing)}.");
        }

        if (unexpected.Count > 0)
        {
          Terminal.WriteErrorLine($"Release gate failed: downloaded artifact has unexpected package(s): {string.Join(", ", unexpected)}.");
        }

        Terminal.WriteErrorLine($"CI run likely predates the version bump — re-run CI on commit {locateOutcome.HeadSha} and retry.");
        AbortPipeline("downloaded package set does not match derived packable set");
        return;
      }

      Terminal.WriteLine($"Package set verified: {string.Join(", ", releasePackages.Select(p => p.FileName))}");

      // Step 6: Push
      WriteStepBanner("Step 6/6: Push to NuGet");
      await PushPackagesAsync(repoRoot, releasePackages, apiKey);

      if (StopOnFailure("Push to NuGet"))
      {
        return;
      }

      await NotifySoftwareSiteAsync(repoRoot, propsVersion);

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED - Packages published to NuGet.org");
      Terminal.WriteLine("===============================================================================");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // release helpers
    // ─────────────────────────────────────────────────────────────────────────

    // Verifies that if tag v{version} already exists locally, HEAD is at that tag's
    // commit. This is what stops a break-glass resume from mixing packages built
    // from two different commits under one version. `git rev-parse -q --verify`
    // fails silently on a missing tag: nonzero exit with empty stderr means NoTag,
    // nonzero exit with stderr means a real git error.
    private static async Task<TagPinOutcome> CheckTagPinAsync(string version)
    {
      string tag = $"v{version}";

      CommandOutput verifyResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "-q", "--verify", $"refs/tags/{tag}")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (verifyResult.ExitCode != 0)
      {
        if (!string.IsNullOrWhiteSpace(verifyResult.Stderr))
        {
          return new TagPinOutcome(TagPinStatus.GitError, null, null, verifyResult.Stderr.Trim());
        }

        return new TagPinOutcome(TagPinStatus.NoTag, null, null, null);
      }

      CommandOutput tagCommitResult = await Shell.Builder("git")
        .WithArguments("rev-parse", $"{tag}^{{commit}}")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (tagCommitResult.ExitCode != 0)
      {
        return new TagPinOutcome(TagPinStatus.GitError, null, null, tagCommitResult.Stderr.Trim());
      }

      CommandOutput headResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (headResult.ExitCode != 0)
      {
        return new TagPinOutcome(TagPinStatus.GitError, null, null, headResult.Stderr.Trim());
      }

      string tagCommit = tagCommitResult.Stdout.Trim();
      string headCommit = headResult.Stdout.Trim();

      return string.Equals(tagCommit, headCommit, StringComparison.Ordinal)
        ? new TagPinOutcome(TagPinStatus.Match, tagCommit, headCommit, null)
        : new TagPinOutcome(TagPinStatus.Mismatch, tagCommit, headCommit, null);
    }

    // git merge-base --is-ancestor exit codes: 0 = ancestor, 1 = NOT ancestor,
    // >1 = git error — a git error must not be reported as "not an ancestor".
    private async Task<AncestorCheckOutcome> CheckHeadAncestorOfMasterAsync()
    {
      string masterRef = "origin/master";

      CommandOutput verifyResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "--verify", "origin/master")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (verifyResult.ExitCode != 0)
      {
        masterRef = "master";

        CommandOutput fallbackVerifyResult = await Shell.Builder("git")
          .WithArguments("rev-parse", "--verify", "master")
          .WithNoValidation()
          .CaptureAsync(CancellationToken.None);

        if (fallbackVerifyResult.ExitCode != 0)
        {
          return new AncestorCheckOutcome(AncestorCheckStatus.MasterUnresolvable, null);
        }

        Terminal.WriteLine("origin/master not found; using local master.");
      }

      CommandOutput ancestorResult = await Shell.Builder("git")
        .WithArguments("merge-base", "--is-ancestor", "HEAD", masterRef)
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (ancestorResult.ExitCode == 0)
      {
        return new AncestorCheckOutcome(AncestorCheckStatus.Ancestor, null);
      }

      if (ancestorResult.ExitCode == 1)
      {
        return new AncestorCheckOutcome(AncestorCheckStatus.NotAncestor, null);
      }

      return new AncestorCheckOutcome(AncestorCheckStatus.GitError, ancestorResult.Stderr.Trim());
    }

    // Resolves HEAD and asks gh for every successful workflow.yml run at that commit,
    // ordered by CiRunPromotion.OrderCandidateRuns (pull_request runs excluded,
    // push-event preferred, then newest). GhUnavailable (gh could not be launched),
    // GhFailed (gh exited nonzero — stderr kept) and NoMatchingRun (gh exited 0 with
    // nothing at this commit) are distinct verdicts with different remedies.
    private static async Task<LocateRunOutcome> LocateCiRunAsync()
    {
      CommandOutput headResult = await Shell.Builder("git")
        .WithArguments("rev-parse", "HEAD")
        .WithNoValidation()
        .CaptureAsync(CancellationToken.None);

      if (headResult.ExitCode != 0)
      {
        throw new InvalidOperationException($"Could not determine HEAD commit: {headResult.Stderr.Trim()}");
      }

      string headSha = headResult.Stdout.Trim();

      CommandOutput runListResult;
      try
      {
        runListResult = await Shell.Builder("gh")
          .WithArguments("run", "list", "--workflow", "workflow.yml", "--commit", headSha, "--status", "success", "--json", "databaseId,event,headSha,createdAt")
          .WithNoValidation()
          .CaptureAsync(CancellationToken.None);
      }
      catch (Win32Exception)
      {
        return new LocateRunOutcome(LocateRunStatus.GhUnavailable, headSha, [], null);
      }

      if (runListResult.ExitCode != 0)
      {
        return new LocateRunOutcome(LocateRunStatus.GhFailed, headSha, [], runListResult.Stderr.Trim());
      }

      List<CiRunSummary>? runs = JsonSerializer.Deserialize(runListResult.Stdout, DevCliJsonContext.Default.ListCiRunSummary);
      IReadOnlyList<CiRunSummary> candidateRuns = CiRunPromotion.OrderCandidateRuns(runs ?? [], headSha);

      return candidateRuns.Count == 0
        ? new LocateRunOutcome(LocateRunStatus.NoMatchingRun, headSha, [], null)
        : new LocateRunOutcome(LocateRunStatus.Found, headSha, candidateRuns, null);
    }

    // Walks candidateRuns (already ordered) until one has a non-expired Packages-*
    // artifact. A run with no matching artifact is skipped silently; a run whose only
    // matches are expired is recorded so the final abort message can tell "expired"
    // apart from "never uploaded". On the winning run the artifacts directory is
    // cleared first — Clean does not run in release mode, so a leftover file from a
    // prior local attempt must not survive into the verified set.
    private async Task<DownloadArtifactOutcome> DownloadPackagesArtifactAsync(string repoRoot, IReadOnlyList<CiRunSummary> candidateRuns)
    {
      string artifactsDir = Path.Combine(repoRoot, ArtifactsSubPath);
      List<ExpiredArtifactEncounter> expiredEncounters = [];

      foreach (CiRunSummary run in candidateRuns)
      {
        CommandOutput artifactsResult = await Shell.Builder("gh")
          .WithArguments("api", $"repos/{{owner}}/{{repo}}/actions/runs/{run.DatabaseId}/artifacts")
          .WithNoValidation()
          .CaptureAsync(CancellationToken.None);

        if (artifactsResult.ExitCode != 0)
        {
          throw new InvalidOperationException($"Failed to list artifacts for run {run.DatabaseId}: {artifactsResult.Stderr.Trim()}");
        }

        RunArtifactListResponse? artifactList = JsonSerializer.Deserialize(artifactsResult.Stdout, DevCliJsonContext.Default.RunArtifactListResponse);
        PackagesArtifactOutcome selectOutcome = CiRunPromotion.SelectPackagesArtifact(artifactList?.Artifacts ?? []);

        if (selectOutcome.Status == PackagesArtifactStatus.Expired)
        {
          expiredEncounters.Add(new ExpiredArtifactEncounter(run.DatabaseId, run.Event, selectOutcome.ExpiredNames));
          continue;
        }

        if (selectOutcome.Status == PackagesArtifactStatus.NoneMatching)
        {
          continue;
        }

        RunArtifact artifact = selectOutcome.Artifact!;

        if (Directory.Exists(artifactsDir))
        {
          Directory.Delete(artifactsDir, recursive: true);
        }

        Directory.CreateDirectory(artifactsDir);

        Terminal.WriteLine($"Downloading artifact '{artifact.Name}' from run {run.DatabaseId} ({run.Event})...");

        int exitCode = await Shell.Builder("gh")
          .WithArguments("run", "download", run.DatabaseId.ToString(CultureInfo.InvariantCulture), "--name", artifact.Name, "--dir", artifactsDir)
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync();

        if (exitCode != 0)
        {
          throw new InvalidOperationException($"Failed to download artifact '{artifact.Name}' from run {run.DatabaseId}!");
        }

        return new DownloadArtifactOutcome(DownloadArtifactStatus.Downloaded, run, artifact.Name, expiredEncounters);
      }

      return new DownloadArtifactOutcome(DownloadArtifactStatus.Exhausted, null, null, expiredEncounters);
    }

    // Push order is cosmetic: NuGet does not validate inter-package dependencies at
    // push time. --skip-duplicate makes a partial-publish resume (check-version
    // Partial, same tag-pinned commit) idempotent: an already-published package's 409
    // is success and only the rest are pushed.
    private async Task PushPackagesAsync(string repoRoot, IReadOnlyList<ReleasePackage> packages, string? apiKey)
    {
      string artifactsDir = Path.Combine(repoRoot, ArtifactsSubPath);

      foreach (ReleasePackage package in packages)
      {
        string nupkgPath = Path.Combine(artifactsDir, package.FileName);

        if (!File.Exists(nupkgPath))
        {
          throw new FileNotFoundException($"Package not found: {nupkgPath}");
        }

        Terminal.WriteLine($"Pushing {package.FileName}...");

        List<string> args = ["nuget", "push", nupkgPath, "--source", "https://api.nuget.org/v3/index.json", "--skip-duplicate"];

        if (!string.IsNullOrEmpty(apiKey))
        {
          args.AddRange(["--api-key", apiKey]);
        }

        int exitCode = await Shell.Builder("dotnet")
          .WithArguments([.. args])
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync();

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"\n❌ NuGet push failed for {package.PackageId} with exit code {exitCode}");
          Environment.ExitCode = 1;
          return;
        }
      }

      Terminal.WriteLine("\n✅ All packages pushed successfully!");
    }

    // Signal timewarp-software to rebuild the site so the new release shows up
    // immediately instead of waiting for its nightly cron backstop. Best effort.
    // Locally gh's stored auth suffices; in Actions the job token cannot reach other
    // repos, so workflow.yml passes an App installation token as REBUILD_DISPATCH_TOKEN
    // which is injected as GH_TOKEN for this one call only.
    private async Task NotifySoftwareSiteAsync(string repoRoot, string coreVersion)
    {
      Terminal.WriteLine("\nNotifying timewarp-software to rebuild the site...");

      string? rebuildToken = Environment.GetEnvironmentVariable("REBUILD_DISPATCH_TOKEN");

      ShellBuilder dispatch = Shell.Builder("gh")
        .WithArguments(
          "api",
          "repos/TimeWarpEngineering/timewarp-software/dispatches",
          "-f", "event_type=rebuild",
          "-f", "client_payload[package]=TimeWarp.Amuru",
          "-f", $"client_payload[version]={coreVersion}")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation();

      if (!string.IsNullOrEmpty(rebuildToken))
      {
        dispatch = dispatch.WithEnvironmentVariable("GH_TOKEN", rebuildToken);
      }

      int exitCode;
      try
      {
        exitCode = await dispatch.RunAsync();
      }
      catch (Win32Exception)
      {
        exitCode = -1;
      }

      if (exitCode == 0)
      {
        Terminal.WriteLine("timewarp-software rebuild dispatched".Green());
      }
      else
      {
        Terminal.WriteLine("Could not dispatch timewarp-software rebuild (non-fatal; the site rebuilds nightly)".Yellow());
      }
    }

    // Each packable project's effective <Version> via real MSBuild evaluation — the
    // same mechanism IPackableProjectService uses for IsPackable/PackageId — so a
    // stray csproj override or conditional import is detected by the lockstep gate
    // rather than assumed away.
    private static async Task<string?> ResolveProjectVersionAsync(string repoRoot, string projectPath, CancellationToken ct)
    {
      CommandOutput result = await Shell.Builder("dotnet")
        .WithArguments("msbuild", projectPath, "-getProperty:Version", "-nologo")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);

      if (result.ExitCode != 0)
      {
        return null;
      }

      string version = result.Stdout.Trim();
      return string.IsNullOrWhiteSpace(version) ? null : version;
    }

    private static string? ReadPropsVersion(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");

      if (!File.Exists(propsPath))
      {
        return null;
      }

      XDocument doc = XDocument.Load(propsPath);
      string? value = doc.Descendants("Version").FirstOrDefault()?.Value.Trim();
      return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string ShortSha(string? sha) =>
      string.IsNullOrEmpty(sha) ? "(unknown)" : sha.Length > 7 ? sha[..7] : sha;

    // ─────────────────────────────────────────────────────────────────────────
    // shared output helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void WriteStepBanner(string title)
    {
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  {title}");
      Terminal.WriteLine("===============================================================================");
    }

    private void AbortPipeline(string reason)
    {
      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  Pipeline ABORTED — {reason}");
      Terminal.WriteLine("===============================================================================");
      Environment.ExitCode = 1;
    }

    private bool StopOnFailure(string stepName)
    {
      if (Environment.ExitCode == 0)
      {
        return false;
      }

      Terminal.WriteErrorLine("");
      Terminal.WriteErrorLine("===============================================================================");
      Terminal.WriteErrorLine($"  Pipeline FAILED - {stepName} failed");
      Terminal.WriteErrorLine("===============================================================================");
      return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // outcome types — distinct verdicts are never collapsed into one message
    // ─────────────────────────────────────────────────────────────────────────

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
