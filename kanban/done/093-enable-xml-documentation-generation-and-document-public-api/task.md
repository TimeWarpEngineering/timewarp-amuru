# Enable XML documentation generation and document public API

## Description

`GenerateDocumentationFile` is set nowhere (`Directory.Build.props`, `msbuild/*.props`, `timewarp-amuru.csproj`) — verified: the packed nupkg contains no `lib/net10.0/timewarp-amuru.xml`. Consumers of a stable 1.0 library get zero IntelliSense docs despite extensive `///` comments in the source, and CS1591 enforcement is off despite `AnalysisMode=All`. Measured with docs force-enabled: **391 public members missing XML docs**.

**Status 2026-10-06:** the core half is done (see checked items). What remains is the **Tools package**: `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` does not set `GenerateDocumentationFile`, so `TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg` ships no XML docs and consumers get blank IntelliSense for every dotnet, git, and fzf builder. Both packages are stable and released in lockstep (1.1.1, then 2.0.0-beta.1 on 2026-10-05), so the old "Tools stays beta" framing no longer applies. Finish this before 2.0.0 GA so the stable release ships documented.

**Decision:** enable `GenerateDocumentationFile` on the Tools csproj exactly as core does (`timewarp-amuru.csproj:9`) and burn down every CS1591 under warnings-as-errors. Do not suppress CS1591 for Tools. Documentation does not change the public surface, so `public-api/PublicAPI.Unshipped.txt` must stay untouched; if the build reports RS0016/RS0017 you changed a signature by mistake.

**Doc quality bar** (tw-csharp XML docs): one-sentence `<summary>` stating what the member does, not "Gets or sets X". For builder methods that map to a `dotnet` CLI option, name the flag and summarize its CLI meaning (e.g. "Adds `--configfile` so NuGet reads the given config file instead of the default hierarchy."). For `Git.*` members describe the git behavior and the result record fields. `<param>` for every parameter; `<returns>` where non-obvious. No placeholder text, no copied-from-neighbor summaries that do not match the member.

## Checklist

- [x] `GenerateDocumentationFile` enabled on the CORE csproj (2026-07-05; Tools enables when its burn-down happens)
- [x] **Core burn-down COMPLETE (2026-07-05)**: after the 094 deletes/split only 5 members were missing (CommandResult, Pipe, AppContextExtensions x3) — all documented; ScriptContext documented in 097; ExecutionResult deleted in 092; also fixed malformed XML in PathResolver docs. CS1591 now enforced on core (warnings-as-errors)
- [x] Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj`
- [x] Measure first: run `./bin/dev build` and count CS1591 per file (the July figures of ~250 across Workload 77, NuGet 70, New 36, UserSecrets 35, Sln 30 are stale; task 100 added ~130 members since). Paste the per-file count into Results before starting the burn-down
- [x] Burn down every CS1591 in Tools: `dot-net-commands/*.cs`, `dot-net-commands/tool/*.cs`, `git-commands/*.cs`, `fzf-command/*.cs`, `DotNet.cs` entry points. Meet the doc quality bar above
- [x] Fix any malformed XML the compiler flags (CS1570/CS1572/CS1573/CS1587) rather than suppressing it
- [x] `./bin/dev build` 0 warnings / 0 errors; `public-api/PublicAPI.Unshipped.txt` unchanged for both packages (`git diff --stat` shows no public-api change)
- [x] Pack and verify: `dotnet pack source/timewarp-amuru-tools/timewarp-amuru-tools.csproj -c Release -o /tmp/amuru-pack` then `unzip -l` the nupkg shows `lib/net10.0/timewarp-amuru-tools.xml`; record its size in Results
- [x] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`; `ganda repo audit` clean
- [x] Core verified: `lib/net10.0/timewarp-amuru.xml` (66KB) present in the nupkg
- [x] Tools verified: `lib/net10.0/timewarp-amuru-tools.xml` present in the nupkg

## Results

`GenerateDocumentationFile` is on for `TimeWarp.Amuru.Tools`, matching core. The first `./bin/dev build` after that property reported **371** CS1591 errors and no other diagnostics. The July counts for the five large files were unchanged. `git-commands/*.cs` and the `DotNet` entry points were already documented (0 CS1591). No CS1570, CS1572, CS1573, or CS1587. Nothing was suppressed.

Measurement before the burn-down, paths under `source/timewarp-amuru-tools/`:

| Count | File |
|------:|------|
| 77 | dot-net-commands/DotNet.Workload.cs |
| 70 | dot-net-commands/DotNet.NuGet.cs |
| 36 | dot-net-commands/DotNet.New.cs |
| 35 | dot-net-commands/DotNet.UserSecrets.cs |
| 30 | dot-net-commands/DotNet.Sln.cs |
| 23 | dot-net-commands/DotNet.Reference.cs |
| 21 | dot-net-commands/DotNet.Watch.cs |
| 7 | dot-net-commands/DotNet.DevCerts.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Install.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.List.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Restore.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Run.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Search.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Uninstall.cs |
| 7 | dot-net-commands/tool/DotNet.Tool.Update.cs |
| 7 | nu-get/nuget-package-service.cs |
| 4 | fzf-command/Fzf.cs |
| 3 | repo/RepoCheckVersionService.cs |
| 3 | repo/RepoCleanService.cs |
| 1 | dot-net-commands/DotNet.AddPackage.cs |
| 1 | dot-net-commands/DotNet.ListPackages.cs |
| 1 | dot-net-commands/DotNet.Pack.cs |
| 1 | dot-net-commands/DotNet.PackageSearch.cs |
| 1 | dot-net-commands/DotNet.Publish.cs |
| 1 | dot-net-commands/DotNet.RemovePackage.cs |

The missing members were builder constructors plus `Build`, `RunAsync`, `CaptureAsync`, `PassthroughAsync`, `TtyPassthroughAsync`, and `SelectAsync` (and `RunAndCaptureAsync` on fzf), plus the NuGet and repo service members. Each summary names that builder's command. `public-api/` has no diff.

After the burn-down, `./bin/dev build` succeeded with 0 warnings and 0 errors. `ganda repo audit` passed 32 checks. `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs` finished with 620 passed, 1 skipped, 0 failed (621 total).

`lib/net10.0/timewarp-amuru-tools.xml` in `TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg` is **528118 bytes**.

### How to validate

Smoke: `dotnet pack source/timewarp-amuru-tools/timewarp-amuru-tools.csproj -c Release -o /tmp/amuru-pack` and `unzip -l /tmp/amuru-pack/TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg | grep timewarp-amuru-tools.xml`. Then `./bin/dev build`.

Expect: the listing contains `lib/net10.0/timewarp-amuru-tools.xml` at 528118 bytes, and the build reports 0 warnings and 0 errors.

### Review disposition

- Rounds: 1. Effort 3, roster: general (Sonnet subagent a641b0de6267ecda2) plus an orchestrator spot-check.
- Final counts: bug 0, suggestion 0, nit 0. Nothing is open, fixed, or wontfix.
- Disposition: **clean**.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers (API-surface and packaging). Sequence after 094-001 (deletes) and 094-003 (package split) — the split cuts the core-1.0 doc burden from 391 members to roughly the core types' share, and every deleted type is doc work avoided. Build is `TreatWarningsAsErrors`, so enabling the property forces the burn-down per project. Paths relative to `source/timewarp-amuru-tools/`. Large mechanical diff expected (hundreds of `///` blocks); split commits by file family (dotnet, tool, git, fzf) so review can read them.

## Session

- Kitchen refresh: 522eb63d (2026-10-06)
- Implementation: Tools XML documentation burn-down (2026-10-06)
- Review: round 1 clean, effort 3 general (2026-10-06)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-06T03:59:38Z
