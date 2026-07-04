# Release stable TimeWarp.Terminal and update dependency

## Description

`Directory.Packages.props:16` pins `TimeWarp.Terminal 1.0.0-beta.12`, a direct SHIPPED dependency (confirmed present in the produced nuspec dependency group). A stable `TimeWarp.Amuru 1.0.0` cannot ship depending on a prerelease package (NU5104).

**BLOCKER — this is the external gating item for the whole release.**

## Checklist

- [ ] Release TimeWarp.Terminal 1.0.0 stable (separate repo)
- [ ] Bump `Directory.Packages.props:16` to the stable version
- [ ] Re-pack and verify the nuspec dependency group shows only stable dependencies
- [ ] While in the file: prune stale pins — `StreamJsonRpc 2.24.84` (:12) and `ModelContextProtocol.Core 1.2.0` (:9) are unused by the library (StreamJsonRpc pin is a leftover from task 084 and invites accidental re-reference)

## Notes

Found by multi-agent release review (2026-07-04). Not blockers: `TimeWarp.Jaribu` / `TimeWarp.Nuru` prerelease pins are dev-only (tests/ and tools/), not in the nuspec. Shipped deps CliWrap 3.10.1 and NuGet.Versioning 7.3.1 are stable. `dotnet list package --vulnerable --include-transitive` is clean.
