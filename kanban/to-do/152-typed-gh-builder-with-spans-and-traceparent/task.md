# Typed Gh builder with spans and TRACEPARENT

## Description

The GitHub CLI emits no OpenTelemetry of its own, so a `gh` call made through
`Shell.Builder("gh")` is invisible in SigNoz: no span, no duration, no parent. A typed
`Gh` builder in Amuru gives every consumer one place to open a span per call, tag it with the
subcommand and repo, propagate `TRACEPARENT` (task 149), and shape the arguments. It also
gives the task 150 analyzer a replacement to name for `Shell.Builder("gh")`.

Org-wide scan on 2026-11-18 of the master worktrees under
`~/worktrees/github.com/TimeWarpEngineering/*`: 51 call sites in 12 repos (timewarp-ganda 8
files, timewarp-nuru 4, the rest one or two each). Subcommands seen:

| Subcommand | Call sites | Notes |
|------------|-----------:|-------|
| run list | 12 | workflow runs, usually `--json` + `--jq` or `--limit` |
| api | 12 | `repos/{owner}/{repo}/actions/runs/{id}/artifacts`, `repos/.../dispatches` |
| run download | 11 | artifacts by run id, `--name`, `--dir` |
| auth token | 5 | token for a NuGet or API call |
| auth status | 2 | preflight |
| issue list / issue create | 2 | |
| other | 7 | argument lists built at runtime, not readable by the scan |

## Requirements

- New `source/timewarp-amuru-tools/gh-commands` following the `dot-net-commands` layout: one
  file per verb, `Options` plumbing, `WithWorkingDirectory`, `WithEnvironmentVariable`,
  `WithNoValidation`, `CaptureAsync` / `RunAsync`, public API files updated.
- Builders for the surface above: `Gh.Run().List()`, `Gh.Run().Download()`, `Gh.Api()`,
  `Gh.Auth().Token()`, `Gh.Auth().Status()`, `Gh.Issue().List()`, `Gh.Issue().Create()`,
  plus `Gh.Pr()` with `View`, `Checks`, `Create`, and `Merge` and `Gh.Release()` with
  `List` and `Create`, because ganda and the dev CLIs reach those today through other paths
  and should land on the same builder. Common flags: `--repo`, `--json` + `--jq`,
  `--limit`. A `WithArgument` escape hatch for a flag the builder does not model.
- Telemetry: each execution opens a span `gh <verb> <subverb>` with tags `gh.command`,
  `gh.repo` when known, `gh.api.endpoint` for `api` (path only, no query values), the exit
  code, and the elapsed time. Follow `dot-net.build-telemetry.cs` for where tags land.
  Never tag or log a token, the `auth token` stdout, request bodies, or `-f` field values.
- `TRACEPARENT` propagation comes from task 149 for free; nothing gh-specific needed beyond
  not overwriting it.
- Typed results where the JSON shape is stable: `run list --json` to a record list, `auth
  status` to a signed-in flag plus host, `pr view --json` to a record. Everything else returns
  the raw `CommandOutput`.
- Tests: Jaribu runfiles asserting the argument string per builder, that the span name and
  tags are set, and that the token never appears in tags. No live GitHub calls.
- Readme section and version bump.
- Add `gh` to the task 150 analyzer table once this lands (AMURU0001 error for the verbs
  covered here), in the same change or a one-line follow-up.

## Out of scope

- Moving the 51 call sites. Each repo does that on its Amuru bump under its own task.
- A GitHub REST client that bypasses `gh`. The point is to wrap the CLI the repos already rely
  on for auth.

## Checklist

- [ ] gh-commands project layout and public API files
- [ ] Run, Api, Auth, Issue, Pr, Release builders
- [ ] Span per call with safe tags
- [ ] Typed results for stable JSON shapes
- [ ] Tests
- [ ] Readme and version bump
- [ ] Analyzer table entry for gh

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session

## Notes

Related: 149 (TRACEPARENT from Amuru), 150 (analyzer), 151 (missing dotnet builders).
Steve: "I don't know if the GitHub command itself has telemetry, but if we make an explicit
typed builder for it, then we can add whatever we want."
