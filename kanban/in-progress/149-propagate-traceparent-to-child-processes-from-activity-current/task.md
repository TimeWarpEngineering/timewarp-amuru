# Propagate TRACEPARENT to child processes from Activity.Current

## Description

Every TimeWarp dev CLI, ganda, and Jaribu's tooling launch child processes through Amuru.
When the launching process has an active `Activity.Current`, Amuru should set `TRACEPARENT`
(and `TRACESTATE` when present) on the child's environment so the child's spans join the
caller's trace. Today each consumer has to remember to do this itself, and most do not.

Evidence (SigNoz, timewarp-ganda-dev `dev test` trace `122d56dda05d165ddf3f8a0ecf2b7ead`,
2026-11-17 16:06 UTC): the `test` command span has one direct child (`ganda.dotnet`). The
dotnet-cli spans for the skills compile, the VSTest run, the 806 s Jaribu `dotnet run`, and
the 295 s AOT `dotnet publish` all parent to the workflow root instead, because the children
inherit the `TRACEPARENT` the dev process itself was started with. Jaribu's per-test spans
(Jaribu task 071) will land as orphans in every consumer for the same reason.

Three repos already carry their own copy of this idea, each incomplete:

- timewarp-jaribu `tools/dev-cli/endpoints/trace-parent-propagation.cs`: `TraceParentPropagation.Apply`, per call site.
- timewarp-ganda `source/timewarp-ganda/services/dotnet-invocation-trace.cs`: `ganda.dotnet` span plus `TRACEPARENT` for one dotnet child.
- timewarp-amuru `source/timewarp-amuru-tools/dot-net-commands/dot-net.build-telemetry.cs`: tags `Activity.Current` for build commands; propagates nothing to the child.

Putting it in Amuru makes it automatic for every consumer on the next package bump. No
repo audit is needed afterwards because there is no helper call left to forget.

## Requirements

- In the normal launch path (`CommandOptions.ApplyTo`, around the `WithEnvironmentVariables`
  call in `source/timewarp-amuru/core/command-options.cs`) and in the TTY passthrough path
  (`source/timewarp-amuru/core/command-result.cs`, where `StartInfo.EnvironmentVariables` is
  filled), when `Activity.Current` is non-null with a non-default `TraceId` and `SpanId`:
  - set `TRACEPARENT` to `00-{traceId}-{spanId}-{01|00}` (flags from `ActivityTraceFlags.Recorded`);
  - set `TRACESTATE` to `Activity.Current.TraceStateString` when it is non-empty;
  - do not overwrite a `TRACEPARENT` or `TRACESTATE` the caller set explicitly through
    `WithEnvironmentVariable(s)`. An explicit `null` value (remove) also wins, so a caller can opt
    a child out.
- No `Activity.Current` leaves the environment alone, so an inherited `TRACEPARENT` from ganda
  or CI still applies to the child.
- Never log the header value.
- The resolved value is computed at start time, not at builder-construction time, so a builder
  created before a span opens still picks up the span active when `ExecuteAsync`/`CaptureAsync`
  runs. Note which choice the implementation made in `## Results`.
- Jaribu task 071's telemetry doc (`documentation/telemetry.md`, "Joining `dev test`") describes
  the consumer-side contract. Amuru's readme or the relevant XML doc gets a short paragraph
  saying propagation is automatic and how to opt out.
- Tests (Jaribu runfiles under `tests/`): child sees `TRACEPARENT` matching the current span;
  no activity leaves env untouched; explicit caller value wins; explicit null removes;
  `TRACESTATE` set only when non-empty; TTY passthrough path covered where testable.
- Version bump per the repo's release convention so consumers can pin it.

## Out of scope (follow-ups in other repos, create after this merges)

- timewarp-jaribu: delete `TraceParentPropagation` and its two call sites in `ci-test-suite.cs`
  once the dev CLI is on the new Amuru.
- timewarp-ganda: drop the `TRACEPARENT` hand-off in `dotnet-invocation-trace.cs` (keep the
  `ganda.dotnet` span), pass nothing extra in `test-command.cs`, bump Jaribu to the 071 release.
- Optional SigNoz saved query or alert: per `*-dev` service, share of `jaribu.class` spans whose
  parent is a `test` span should be 100 percent. This is the behavioral guard that replaces a
  repo audit.

## Goal (implementer restatement)

When code running inside an `Activity` starts a child process through Amuru, the child should
join that trace without the caller doing anything. Amuru writes `TRACEPARENT` (W3C
`00-{traceId}-{spanId}-{flags}`) and, when the activity has one, `TRACESTATE` into the child's
environment, for every launch path (CliWrap paths and the raw-process TTY passthrough path).

Acceptance criteria:

- Child sees `TRACEPARENT` built from `Activity.Current` at the moment the command starts (not
  when the builder or `CommandResult` was built). Flags are `01` when recorded, else `00`.
- `TRACESTATE` is set only when `Activity.Current.TraceStateString` is non-empty.
- No current activity (or default trace/span id): the environment is untouched, so an inherited
  `TRACEPARENT` still flows through.
- A caller value set with `WithEnvironmentVariable(s)` wins, including `null` (remove), which opts
  the child out.
- The header value is never logged.
- Covered by Jaribu runfile tests; TTY passthrough covered where testable.
- Readme/XML doc paragraph on automatic propagation and opt-out; version bumped to the next beta.

## Checklist

- [x] Propagation in the normal launch path (at start time in `CommandResult`, not in `CommandOptions.ApplyTo`; see Results)
- [x] Propagation in the TTY passthrough path
- [x] Explicit caller value and explicit null win
- [x] Tests listed above
- [x] Doc paragraph on automatic propagation and opt-out
- [x] Version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session; see `- 2026-11-18 implement oracle (claude-opus-5-5): start-time propagation in all launch paths, tests, docs, bump to 3.0.0-beta.6.
  Boy-scout: `ganda repo audit --fix` created the git-ignored `.vscode/settings.json` (vscode-window-icon warning); no tracked change.

## Results

### What changed

- New `source/timewarp-amuru/core/trace-context-propagation.cs`: internal `TraceContextPropagation`
  (`Capture` reads `Activity.Current`; `VariablesToAdd` / `Apply` add `TRACEPARENT` and, when non-empty,
  `TRACESTATE`, skipping any key the caller set, null included; key match ignores case on Windows).
- New partial `source/timewarp-amuru/core/command-result.trace-context.cs`: `StartCommand()` applies the
  context to `InternalCommand` inside each execution mode, right before the CliWrap call; `FillTtyEnvironment`
  for `TtyPassthroughAsync`. Pipe compositions now carry `PipeStages` so every stage (not only the first)
  gets the variables; `StartCommand` rebuilds the same left fold `PipeCore` builds.
- Every `InternalCommand.With...` execution site and the tee/listen paths (`StartTeeCommand`) now start from
  `StartCommand()`. The JSON-RPC child (`json-rpc-child-process.cs`) gets the same variables.
- Docs: readme "Trace Context Propagation" section, `documentation/developer/reference/command-execution.md`
  "Trace context", `CommandOptions.EnvironmentVariables` XML remarks, release note `3.0.0-beta.6.md`.
- Version `3.0.0-beta.5` → `3.0.0-beta.6` in `source/Directory.Build.props`. No public API change.

### Design choices

- **Start time, not construction time.** The value is resolved when `RunAsync`/`CaptureAsync`/etc. starts the
  process. A `CommandResult` built before a span opens hands the child the span active at execution
  (test `ResultBuiltBeforeSpan_Should_UseSpanActiveAtStart`). That is why it is not in `CommandOptions.ApplyTo`
  (which runs at `Build()`): `ApplyTo` keeps the CliWrap command's environment equal to the caller's explicit
  set, which is how an explicit value or null removal is recognized.
- No current activity (or default trace/span id) leaves the environment alone; an inherited `TRACEPARENT`
  flows through.
- `TRACESTATE` is only written when non-empty; an inherited `TRACESTATE` is not removed when the activity has none.
- Header values are never logged.
- Mocked commands start no process, so nothing is propagated there.

### Gates

| Gate | Command | Result |
|---|---|---|
| Build | `./bin/dev build` | Pass (0 warnings, 0 errors) |
| Test | `./bin/dev test` | Pass: 890 total, 889 passed, 1 skipped (pre-existing); `TraceContext_Given_` 11/11 |
| Audit | `ganda repo audit` | First run: pass with 1 advisory warning (vscode-window-icon). After `ganda repo audit --fix`: "Repository passes all audit checks." |

### How to validate

Smoke:

```bash
./bin/dev build
(cd tests/timewarp-amuru/single-file-tests/core && dotnet run shell-builder.trace-context.cs)
```

Expect: 11 tests pass — child `TRACEPARENT` equals `00-{traceId}-{spanId}-01` of the current activity (`-00`
when not recorded); no activity leaves `TRACEPARENT`/`TRACESTATE` equal to the inherited values; explicit
caller values win; explicit null leaves both `<unset>`; `TRACESTATE` set only when non-empty; a result built
before the span opens uses the span active at start; the downstream `Pipe` stage gets it; `TtyPassthroughAsync`
writes the activity's values (and honors explicit null).

Not covered by a test: the JSON-RPC child path (same helper, needs a JSON-RPC server child).

### Follow-ups (other repos, after merge and release)

- timewarp-jaribu: delete `TraceParentPropagation` and its call sites once on Amuru 3.0.0-beta.6.
- timewarp-ganda: drop the `TRACEPARENT` hand-off in `dotnet-invocation-trace.cs`.
- Optional SigNoz guard query (see Out of scope).

## Notes`

## Notes

Origin: a conversation on test selection for ganda's suite. Measuring compile versus execution
needed the dotnet-cli spans to sit under the `test` span; they did not, which surfaced this.
The measurement itself is already answered from the trace above: Jaribu suite build about 63 s,
execution about 12.4 min under dotnet-coverage, AOT publish 4.9 min.
