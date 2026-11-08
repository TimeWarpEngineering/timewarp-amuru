# Enable dev CLI telemetry with UseTelemetry

## Goal (implementer restatement)

Every `dev` command in timewarp-amuru should export one OpenTelemetry command span and a duration metric, including the shared DevCli routes. The dev CLI opts in at compile time by calling `UseTelemetry` with the literal service name `timewarp-amuru-dev`. Export stays off unless `OTEL_EXPORTER_OTLP_ENDPOINT` is set. A caller's `TRACEPARENT` is restored before `RunAsync` so those spans nest under ganda or CI. This repo has no ActivitySource of its own, so none is registered. `dev --capabilities`, `dev build`, and an AOT publish of the dev CLI still succeed.

Acceptance criteria:

- `tools/dev-cli/dev.cs` calls `.UseTelemetry(options => options.ServiceName = "timewarp-amuru-dev")` on the builder chain.
- `IncomingTraceContext.Restore()` wraps `app.RunAsync(args)`.
- `tools/dev-cli/telemetry.md` names the export and trace-context environment variables.
- A built dev CLI run with an OTLP endpoint set delivers a command span.
- `dev --capabilities` and `dev build` work, and AOT publish of the dev CLI succeeds.

## Description

Every `dev` command in this repo should emit OpenTelemetry spans and metrics so we can see
which dev commands are slow across the fleet. Telemetry in TimeWarp.Nuru is a compile-time
opt-in: the `TimeWarp.Nuru.DevCli` package ships no telemetry code, so each repo's
`tools/dev-cli/dev.cs` must call `UseTelemetry` on the builder. Without it the source
generator emits nothing.

Reference implementation: timewarp-nuru `tools/dev-cli/dev.cs` (kanban 556) and
`tools/dev-cli/telemetry.md`. Nuru docs: `documentation/user/features/telemetry.md`.

Export is gated at runtime on `OTEL_EXPORTER_OTLP_ENDPOINT` (set in the developer shell to
`https://otlp.timewarp.work` with `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`). No endpoint
means no export, so the change is safe in CI even without a collector.

## Requirements

- `tools/dev-cli/dev.cs` calls `.UseTelemetry(options => options.ServiceName = "timewarp-amuru-dev")`
  on the `NuruApp.CreateBuilder()` chain. The service name must be a string literal.
- Every route, including the DevCli-provided ones (clean, check-version, release, self-install),
  gets the automatic `TimeWarp.Nuru.Behavior` command span and duration metric.
- Copy `IncomingTraceContext` from timewarp-nuru `tools/dev-cli/endpoints/incoming-trace-context.cs`
  and wrap `app.RunAsync(args)` in `IncomingTraceContext.Restore()` so command spans nest under
  the caller's `TRACEPARENT` (ganda, CI).
- If the repo has its own `ActivitySource`, register it with `options.ActivitySources.Add("<literal>")`.
- `dev --capabilities` and `dev build` still work; AOT publish of the dev CLI still succeeds.

## Checklist

- [x] Add `UseTelemetry` call to `tools/dev-cli/dev.cs`
- [x] Add `IncomingTraceContext` and wrap `RunAsync`
- [x] Register repo-specific ActivitySources if any (none in this repo)
- [x] Build dev CLI (`bin/dev build`) and run one command with `OTEL_EXPORTER_OTLP_ENDPOINT` set; confirm a span arrives
- [x] Add a short `tools/dev-cli/telemetry.md` naming the env vars (copy from timewarp-nuru)

## Results

`tools/dev-cli/dev.cs` calls `UseTelemetry` with the literal service name `timewarp-amuru-dev`. `IncomingTraceContext.Restore()` wraps `RunAsync`. A search of `*.cs` found no `ActivitySource`, so none is registered. `tools/dev-cli/telemetry.md` names `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME`, `TRACEPARENT`, `TRACESTATE`, and `OTEL_EXPORTER_OTLP_PROTOCOL`.

`dotnet build tools/dev-cli/dev.cs` succeeded. `self-install` AOT-published `bin/dev` (ELF x86-64). `./bin/dev --capabilities` exited 0 and listed `build`, `clean`, `test`, `verify-samples`, `workflow`, `check-version`, `release`, and `self-install`. `./bin/dev build` exited 0 (Release, 0 warnings).

A local OTLP HTTP listener on `127.0.0.1:14318` received exports from that AOT binary with `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` set. The `check-version` span (DevCli route) and the `build` span both used service `timewarp-amuru-dev` and source `TimeWarp.Nuru.Behavior`, and each had a `nuru.behavior.commands.duration` metric. Both trace payloads contained trace id `0af7651916cd43dd8448eb211c80319c` and parent span id `b7ad6b7169203331` from `TRACEPARENT`. `check-version` exited 1 because version 3.0.0-beta.1 is already published; the span still arrived.

`ganda repo audit` reported one advisory (`vscode-window-icon`). `ganda repo audit --fix` wrote a gitignored `.vscode/settings.json` window title. The follow-up audit passed 38 checks, failed 0, skipped 2.

### Review

The review ran 1 round with one general reviewer (effort 2, by-diff budget, 209 lines). No findings were raised: 0 bug, 0 suggestion, and 0 nit (0 open, 0 fixed, 0 wontfix). The disposition is **clean**.

The reviewer confirmed that `incoming-trace-context.cs` is byte-identical to the timewarp-nuru reference. `dotnet build tools/dev-cli/dev.cs` succeeded. `./bin/dev --capabilities` exited 0, including with an invalid `TRACEPARENT`. The goal restatement matches the task, with no misread.

Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke: from the repo root, point `OTEL_EXPORTER_OTLP_ENDPOINT` at an HTTP protobuf collector (for example `http://127.0.0.1:14318`), set `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` and `TRACEPARENT=00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01`, then run `./bin/dev build`.

Expect: the command exits 0. The collector receives a `/v1/traces` payload whose service name is `timewarp-amuru-dev`, whose span name is `build`, whose activity source is `TimeWarp.Nuru.Behavior`, and whose bytes include that trace id and parent span id. A `nuru.behavior.commands.duration` metric for `build` arrives on `/v1/metrics`. With `OTEL_EXPORTER_OTLP_ENDPOINT` unset, the same command exports nothing.

## Session

- Created: b3be0249-1852-425a-9e90-5a7e56049503 (2026-11-08)
- Implementation: 01a1bb54-19e4-7781-bd05-c788438499f8 (2026-11-08). Audit --fix wrote the gitignored VS Code window title. No repo ActivitySource to register.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1bb54-19e4-7781-bd05-c788438499f8, max-turns 500, Done — 2026-11-08T13:33:18Z
- Review oracle: claude-opus-5-5 (anthropic), tw-implementation-review, effort 2, roster general, turn cap 120. Round 1 disposition clean (0 findings) — 2026-11-08
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session ea617b36-8bff-42b0-a9be-684437d24e00, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-08T13:35:16Z

## Review

- Round 1: Done

## Notes

Fleet rollout: Steve asked on 2026-11-07 for telemetry on all dev CLI commands in every live
reposet repo. The same task exists in each live repo that has a `tools/dev-cli`. Only
timewarp-nuru had it before this rollout.
