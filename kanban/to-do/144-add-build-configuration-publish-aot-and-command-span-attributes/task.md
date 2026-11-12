# Add build.configuration, build.publish_aot, and build.command span attributes

## Description

Add span attributes `build.configuration`, `build.publish_aot`, and `build.command` (`build` / `publish` / `pack` / `test`) on `Activity.Current` in Amuru's `DotNet.Build()`, `DotNet.Publish()`, and related calls (`DotNet.Pack()`, `DotNet.Test()`).

One change here covers every repo's `dev` CLI, because those commands go through TimeWarp.Amuru. Nuru `TelemetryBehavior` cannot infer AOT or configuration by itself. Dev-cli docs already say `dotnet build` and `dotnet test` are not instrumented (`timewarp-nuru/tools/dev-cli/telemetry.md`).

## Requirements

- Set tags on `Activity.Current` (do not start a new ActivitySource unless one already exists for this API). Prefer not to export argv, secrets, or exception messages.
- `build.configuration`: the configuration passed to the command (for example `Release` or `Debug`), when known.
- `build.publish_aot`: whether this invocation is a Native AOT publish (`PublishAot=true` or equivalent publish flags), not the `cli.runtime=aot|managed` attribute on official `dotnet-cli` spans (that is the CLI host flavor; 7-day publish p50 was ~15s for both).
- `build.command`: `build`, `publish`, `pack`, or `test` to match the Amuru API used.
- Apply in `DotNet.Build()`, `DotNet.Publish()`, and the related pack/test entry points under `source/timewarp-amuru-tools/dot-net-commands/`.

## Checklist

- [ ] Set the three attributes from `DotNet.Build()`, `DotNet.Publish()`, `DotNet.Pack()`, and `DotNet.Test()`
- [ ] Cover the fluent `WithConfiguration` / publish-AOT path so a `dev self-install` or `dev build-ganda` is visibly AOT
- [ ] Tests for the new tags (no secrets in asserted attributes)
- [ ] After a dev-cli run with `OTEL_EXPORTER_OTLP_ENDPOINT` set, the attributes appear on the command or child span in SigNoz

## Acceptance criteria

- A `dev build` (Release, not publish) and a `dev self-install` / `DotNet.Publish` of a `PublishAot=true` runfile are distinguishable in SigNoz via `build.publish_aot` and `build.command`.
- Existing command spans (`command.name`, status, `error.type`) stay as they are.

## Notes

- Why Amuru, not Nuru or each repo's `build-command.cs`: shared `TimeWarp.Nuru.DevCli` does not ship `build`/`test`/`publish`; those are per-repo copies. Hooking Amuru is the one-change path.
- Baseline (SigNoz, 7 days ending 2026-11-12 Asia/Bangkok): `dotnet publish` p50 15.4s / p95 273s vs `dotnet build` p50 4.4s / p95 33s. There is currently no project-AOT tag to explain that tail.
- Related: timewarp-ganda 616 (parent raw gates), timewarp-jenzi 011 (umbrella).
- Filed by Jenzi from cross-repo build-telemetry audit 2026-11-12.
