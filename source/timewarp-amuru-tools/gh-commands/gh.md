# Gh fluent API

Typed builders for the GitHub CLI (`gh`). Task 152.

`gh` emits no OpenTelemetry. Each execution of a `Gh` builder opens a span on the
`ActivitySource` named `Gh.ActivitySourceName` (`TimeWarp.Amuru.Gh`), so the call appears as a
child of the caller's span. `TRACEPARENT` reaches `gh` from `CommandResult` (task 149) and names
that span.

## Builders

| Entry | Command | Base |
|-------|---------|------|
| `Gh.Run().List()` | `gh run list` | list (`--repo`, `--json`, `--jq`, `--limit`) |
| `Gh.Run().Download()` / `Download(runId)` | `gh run download` | repo (`--repo`) |
| `Gh.Api(endpoint)` | `gh api` | command |
| `Gh.Auth().Token()` | `gh auth token` | command |
| `Gh.Auth().Status()` | `gh auth status` | command |
| `Gh.Issue().List()` | `gh issue list` | list |
| `Gh.Issue().Create()` | `gh issue create` | repo |
| `Gh.Pr().View()` / `View(selector)` | `gh pr view` | json (`--repo`, `--json`, `--jq`) |
| `Gh.Pr().Checks()` / `Checks(selector)` | `gh pr checks` | json |
| `Gh.Pr().Create()` | `gh pr create` | repo |
| `Gh.Pr().Merge()` / `Merge(selector)` | `gh pr merge` | repo |
| `Gh.Release().List()` | `gh release list` | list |
| `Gh.Release().Create(tag)` | `gh release create` | repo |

Every builder has `WithWorkingDirectory`, `WithEnvironmentVariable`, `WithNoValidation`,
`WithZeroExitCodeValidation`, `WithTimeout`, `WithArgument` / `WithArguments` (appended after the
modeled arguments), `ToArguments()`, `Build()`, and the span-wrapped `RunAsync`, `CaptureAsync`,
`RunAndCaptureAsync`, and `PassthroughAsync`. `Build()` opens no span.

## Typed results

| Method | Runs | Returns |
|--------|------|---------|
| `GhRunListBuilder.ListRunsAsync()` | `gh run list --json <GhWorkflowRun.JsonFields>` | `GhRunListResult` (`Success`, `Runs`, `Output`) |
| `GhPrViewBuilder.ViewPullRequestAsync()` | `gh pr view --json <GhPullRequest.JsonFields>` | `GhPullRequestViewResult` (`Success`, `PullRequest`, `Output`) |
| `GhAuthStatusBuilder.GetStatusAsync()` | `gh auth status --json hosts` | `GhAuthStatus` (`SignedIn`, `Host`, `Login`, `Output`) |

The typed methods replace any `--json` fields and `--jq` set earlier. A non-zero exit returns
`Success` false (or `SignedIn` false); it does not throw unless strict validation is set.

## Span

| Item | Value |
|------|-------|
| Name | `gh <verb> <sub-verb>`, for example `gh run list`, `gh api` |
| Kind | Client |
| `gh.command` | `run list`, `api`, ... |
| `gh.repo` | the `WithRepo` value, when set |
| `gh.api.endpoint` | `gh api` only: the endpoint path. Query string, fragment, scheme, host, and user info are cut off |
| `gh.exit_code` | the exit code |
| `gh.elapsed_ms` | elapsed milliseconds |
| Status | Error with `exit code N` on a non-zero exit; Error with the exception type name when the run threw |

Arguments, `-f` / `-F` values, headers, bodies, notes, stdout (including the `gh auth token`
output), stderr, and exception messages never reach the span.

## Analyzer

`Shell.Builder("gh")` or `Shell.Run("gh", ...)` with a command these builders cover is AMURU0001
(error), naming the builder. The table is `TypedBuilderTable.GhRows`.
