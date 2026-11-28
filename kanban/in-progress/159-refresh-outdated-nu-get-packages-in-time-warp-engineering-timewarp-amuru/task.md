# Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru

## Description

Refresh outdated NuGet packages in TimeWarpEngineering/timewarp-amuru.
Filter: (all)
Outdated:
  TimeWarp.Ramani 1.0.0-beta.25 -> 1.0.0-beta.26 (library: source/timewarp-amuru/timewarp-amuru.csproj has no PrivateAssets=all)
Edit Directory.Packages.props in this repo only.

Version: set <Version> in source/Directory.Build.props from 3.0.0-beta.10 to 3.0.0-beta.11 in this pull request. The prerelease suffix is -beta.N, never +beta.N. The wave releases it after merge.

## Goal (implementer restatement)

Move the one outdated library pin onto the published package, and cut the next prerelease so the wave can release what is already on master.

Acceptance:

- `Directory.Packages.props` pins `TimeWarp.Ramani` at `1.0.0-beta.26`. No other package pin moves, and the self pins stay on the published `3.0.0-beta.10` packages the git hooks restore.
- `<Version>` in `source/Directory.Build.props` is `3.0.0-beta.11`.
- This is the version-bump pull request, so each packable project's `PublicAPI.Unshipped.txt` entries move into `PublicAPI.Shipped.txt` and Unshipped is left as only `#nullable enable`.
- The library still builds against Ramani, the session-machine and command-completion runfiles still pass, and `ganda repo audit` passes.

## Checklist

- [x] `TimeWarp.Ramani` is `1.0.0-beta.26` in `Directory.Packages.props`
- [x] `<Version>` is `3.0.0-beta.11`
- [x] Public API unshipped lines are promoted on both packable projects
- [x] Build, covering runfiles, and `ganda repo audit` pass

## Results

`Directory.Packages.props` pins `TimeWarp.Ramani` at `1.0.0-beta.26` (was `1.0.0-beta.25`). That release adds the use-case package and demo diagrams. The session machine and the command-completion oracle were not edited. Self pins stay at the published `3.0.0-beta.10` packages the git hooks restore.

`<Version>` in `source/Directory.Build.props` is `3.0.0-beta.11`. Unshipped public API lines moved into `PublicAPI.Shipped.txt`: 109 lines on `TimeWarp.Amuru` (native Tail) and 321 lines on `TimeWarp.Amuru.Tools` (Gh builders). Both `PublicAPI.Unshipped.txt` files are only `#nullable enable`.

### How to validate

Smoke: `grep -n 'TimeWarp.Ramani' Directory.Packages.props && grep -n '<Version>' source/Directory.Build.props && ./bin/dev check-version`

Expect: Ramani `1.0.0-beta.26`, `<Version>3.0.0-beta.11</Version>`, and "Version in source is new — safe to release."

Gates:

- `./bin/dev build` — succeeded, 0 warnings, 0 errors; packages built as `TimeWarp.Amuru.3.0.0-beta.11.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.11.nupkg`.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` — 19 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs` — 31 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.guard.cs` — 14 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs` — 20 passed.
- `./bin/dev check-version` — source `3.0.0-beta.11`, latest NuGet `3.0.0-beta.10`, "Version in source is new — safe to release."
- `ganda repo audit` — pass, 42 passed, 0 failed, 1 warning: `vscode-window-icon` (pre-existing advisory). Exit 0.

## Session

- Minted by ganda task wave run 20261126T021837Z-0cb631
- Implement oracle: implemented by implementer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 500), Done — 2026-11-28T14:42:19Z
