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

## Checklist

- [ ] Propagation in `CommandOptions.ApplyTo`
- [ ] Propagation in the TTY passthrough path
- [ ] Explicit caller value and explicit null win
- [ ] Tests listed above
- [ ] Doc paragraph on automatic propagation and opt-out
- [ ] Version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session; see `## Notes`

## Notes

Origin: a conversation on test selection for ganda's suite. Measuring compile versus execution
needed the dotnet-cli spans to sit under the `test` span; they did not, which surfaced this.
The measurement itself is already answered from the trace above: Jaribu suite build about 63 s,
execution about 12.4 min under dotnet-coverage, AOT publish 4.9 min.
