# Round 1 — general
**Date:** 2026-11-08
**Scope reviewed:** branch vs `master`. Product files: `tools/dev-cli/dev.cs`, `tools/dev-cli/endpoints/incoming-trace-context.cs`, `tools/dev-cli/telemetry.md`. Also read `tools/dev-cli/Directory.Build.props` and `tools/dev-cli/global-usings.cs`.

## Summary

The change adds `.UseTelemetry(options => options.ServiceName = "timewarp-amuru-dev")` to the `NuruApp.CreateBuilder()` chain. The service name is a string literal, as the task requires. It wraps `app.RunAsync(args)` in `using IDisposable? incomingTrace = IncomingTraceContext.Restore();`. It also adds `tools/dev-cli/telemetry.md`.

`incoming-trace-context.cs` matches the timewarp-nuru reference byte for byte (`diff` printed nothing). `endpoints/**/*.cs` compiles it, and `global-usings.cs` provides `System.Diagnostics` and `System.Runtime.CompilerServices`. The task's ActivitySource requirement is conditional, and the repo has none.

`telemetry.md` is the nuru doc with the service name changed and nuru's workflow-stage-span section removed, which is correct because amuru has no stage spans. It names `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME`, `TRACEPARENT`, `TRACESTATE`, and `OTEL_EXPORTER_OTLP_PROTOCOL`.

Risk is low. The change is opt-in at runtime and inert without an endpoint.

Re-verified in this review:
- `dotnet build tools/dev-cli/dev.cs` succeeded.
- `./bin/dev --capabilities` exited 0.
- With `TRACEPARENT=garbage`, `./bin/dev --capabilities` still exited 0, so an invalid header is ignored.

I did not re-run the implementer's OTLP listener check. Their Results record the trace id and parent span id arriving on both a DevCli route (`check-version`) and a local route (`build`).

Goal restatement vs task: the restatement covers every requirement and checklist item and adds nothing outside the task. No misread.

## Issues

<!-- None. -->
