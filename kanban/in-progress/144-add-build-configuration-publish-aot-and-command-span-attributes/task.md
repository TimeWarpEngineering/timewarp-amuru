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

## Goal (implementer restatement)

When a caller runs `DotNet.Build()`, `DotNet.Publish()`, `DotNet.Pack()`, or `DotNet.Test()` from TimeWarp.Amuru.Tools, the ambient `Activity.Current` (for a `dev` CLI this is the Nuru command span) gets three tags:

- `build.command`: `build`, `publish`, `pack`, or `test`, matching the builder.
- `build.configuration`: the configuration passed with `WithConfiguration` (or a `Configuration` MSBuild property). Not set when no configuration is known.
- `build.publish_aot`: `true` only for a Native AOT publish. That is a `PublishAot=true` property on the builder, a file-based app (`.cs` runfile, where the SDK defaults `PublishAot` to true) without a `#:property PublishAot=false` directive, or a project file that sets `<PublishAot>true</PublishAot>`. `false` for build, pack, test, and non-AOT publishes.

No new ActivitySource. No argv, paths, secrets, or exception text go into tags. With no `Activity.Current`, nothing happens.

Acceptance:
- `dev build` (Release build) and `dev self-install` (`DotNet.Publish("dev.cs")`, AOT runfile) are distinguishable by `build.command` and `build.publish_aot`.
- Existing command span tags (`command.name`, status, `error.type`) are untouched (we only add tags).
- Tests assert the three tags for each builder, including the runfile and `PublishAot` property paths, and that no other `build.*` tag carries a path or argv.

## Checklist

- [x] Set the three attributes from `DotNet.Build()`, `DotNet.Publish()`, `DotNet.Pack()`, and `DotNet.Test()`
- [x] Cover the fluent `WithConfiguration` / publish-AOT path so a `dev self-install` or `dev build-ganda` is visibly AOT
- [x] Tests for the new tags (no secrets in asserted attributes)
- [x] After a dev-cli run with `OTEL_EXPORTER_OTLP_ENDPOINT` set, the attributes appear on the command or child span in SigNoz

## Acceptance criteria

- A `dev build` (Release, not publish) and a `dev self-install` / `DotNet.Publish` of a `PublishAot=true` runfile are distinguishable in SigNoz via `build.publish_aot` and `build.command`.
- Existing command spans (`command.name`, status, `error.type`) stay as they are.

## Notes

- Why Amuru, not Nuru or each repo's `build-command.cs`: shared `TimeWarp.Nuru.DevCli` does not ship `build`/`test`/`publish`; those are per-repo copies. Hooking Amuru is the one-change path.
- Baseline (SigNoz, 7 days ending 2026-11-12 Asia/Bangkok): `dotnet publish` p50 15.4s / p95 273s vs `dotnet build` p50 4.4s / p95 33s. There is currently no project-AOT tag to explain that tail.
- Related: timewarp-ganda 616 (parent raw gates), timewarp-jenzi 011 (umbrella).
- Filed by Jenzi from cross-repo build-telemetry audit 2026-11-12.

## Results

### What changed

- `source/timewarp-amuru-tools/dot-net-commands/dot-net.build-telemetry.cs` (new, internal `DotNetBuildTelemetry`): tags `Activity.Current` with `build.command`, `build.configuration` (only when known), and `build.publish_aot`. Does nothing when there is no current activity. No new ActivitySource. Only the command word, the configuration name, and a bool are exported.
- `build.publish_aot` is true only for a publish: an explicit `PublishAot` property wins; otherwise a `.cs` runfile is AOT (SDK default) unless it has `#:property PublishAot=false`; otherwise a project file whose text sets `<PublishAot>true</PublishAot>`. Directory.Build.props and directory project paths are not inspected (documented in the file's Design region).
- `DotNetBuildBuilder`, `DotNetPublishBuilder`, `DotNetPackBuilder`, and `DotNetTestBuilder` call it from `Build()`, so every terminal (`RunAsync`, `CaptureAsync`, passthrough, and others) is covered. To stay inside the TWA0038 file-length ratchet, the redundant `// Add boolean flags` comment was dropped from `dot-net.publish.cs` and `dot-net.test.cs`.
- `tools/dev-cli/endpoints/build-command.cs`: `dev build` in this repo now runs through `DotNet.Build(...).WithConfiguration("Release")` instead of a raw `dotnet build` process, so its own span carries the tags. Output handling is the same: capture, then print stdout and stderr.
- `tools/dev-cli/telemetry.md`: documents the three tags.
- Tests: `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build-telemetry.cs` has 14 tests. They cover build, test, and pack; configuration from the method or the property; an absent configuration; PublishAot as a property (true and false); build ignoring PublishAot; a runfile as AOT by default and with the directive off; a project file with PublishAot; a missing project; no current activity; and a check that only the three `build.*` keys are added, with no path, property value, or source credentials in any of them.
- Audit boy-scout: `ganda repo audit --fix` set the VS Code window title in `.vscode/settings.json`, which is gitignored and local only.

### SigNoz verification

The run used `OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.timewarp.work` with protocol `http/protobuf`, for the service `timewarp-amuru-dev`, trace `fdf68d446c565b49d2c6b04501342101` (2026-11-15):
- span `build` (`command.name=build`, status Ok): `build.command=build`, `build.configuration=Release`, `build.publish_aot=false`.
- span `self-install` (`command.name=self-install`, status Ok): `build.command=publish`, `build.publish_aot=true`, and no configuration tag, because DevCli self-install passes none.
Existing `command.name` and status are unchanged.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | PASS. The first run failed TWA0038, because `dot-net.publish.cs` and `dot-net.test.cs` were 2 lines over the ratchet. They were trimmed and the rerun passed. |
| Test | `./bin/dev test` | PASS: 853 total, 852 passed, 1 skipped, 0 failed |
| Audit | `ganda repo audit` | PASS with 1 advisory warning (`vscode-window-icon`: `.vscode/settings.json` missing). After `ganda repo audit --fix`, all checks pass. |

### How to validate

Smoke:
1. `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.build-telemetry.cs`
2. With `OTEL_EXPORTER_OTLP_ENDPOINT` set, run `./bin/dev build` and then `./bin/dev self-install`. In SigNoz, query service `timewarp-amuru-dev` with select fields `build.command`, `build.configuration`, and `build.publish_aot`.

Expect:
1. 14/14 pass.
2. The `build` span shows `build.command=build`, `build.configuration=Release`, and `build.publish_aot=false`. The `self-install` span shows `build.command=publish` and `build.publish_aot=true`.

## Session

- 2026-11-15 implementer (claude-opus-5-5, ganda task work implement oracle): implemented the tags, switched this repo's `dev build` to Amuru, ran the gates, and verified the result in SigNoz.
