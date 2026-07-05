# Release stable TimeWarp.Terminal and update dependency

## Description

`Directory.Packages.props` pinned `TimeWarp.Terminal 1.0.0-beta.12`, a direct SHIPPED dependency. A stable `TimeWarp.Amuru 1.0.0` cannot ship depending on a prerelease package (NU5104). This was the external gating item for the release.

## Checklist

- [x] Release TimeWarp.Terminal 1.0.0 stable (done in Terminal repo)
- [x] Bump `Directory.Packages.props` to `TimeWarp.Terminal 1.0.0`
- [x] Re-pack and verify the nuspec dependency group shows only stable dependencies (CliWrap 3.10.1, NuGet.Versioning 7.3.1, TimeWarp.Terminal 1.0.0)
- [x] Prune stale pins: `StreamJsonRpc 2.24.84` and `ModelContextProtocol.Core 1.2.0` removed (084 leftover; all code references were commented out)

## Results

Completed 2026-07-05. Library builds clean (0 warnings/errors) and the full suite passes (357/358, 1 known env skip) against Terminal 1.0.0. Nuspec verified all-stable — the NU5104 blocker is cleared. A `global.json` pinning SDK 10.0.301 was added in the same session (task 103 item) since the build is only clean on SDK 10.
