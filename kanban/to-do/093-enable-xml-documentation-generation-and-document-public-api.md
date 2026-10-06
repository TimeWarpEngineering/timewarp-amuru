# Enable XML documentation generation and document public API

## Description

`GenerateDocumentationFile` is set nowhere (`Directory.Build.props`, `msbuild/*.props`, `timewarp-amuru.csproj`) — verified: the packed nupkg contains no `lib/net10.0/timewarp-amuru.xml`. Consumers of a stable 1.0 library get zero IntelliSense docs despite extensive `///` comments in the source, and CS1591 enforcement is off despite `AnalysisMode=All`. Measured with docs force-enabled: **391 public members missing XML docs**.

**Status 2026-10-06:** the core half is done (see checked items). What remains is the **Tools package**: `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` does not set `GenerateDocumentationFile`, so `TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg` ships no XML docs and consumers get blank IntelliSense for every dotnet, git, and fzf builder. Both packages are stable and released in lockstep (1.1.1, then 2.0.0-beta.1 on 2026-10-05), so the old "Tools stays beta" framing no longer applies. Finish this before 2.0.0 GA so the stable release ships documented.

**Decision:** enable `GenerateDocumentationFile` on the Tools csproj exactly as core does (`timewarp-amuru.csproj:9`) and burn down every CS1591 under warnings-as-errors. Do not suppress CS1591 for Tools. Documentation does not change the public surface, so `public-api/PublicAPI.Unshipped.txt` must stay untouched; if the build reports RS0016/RS0017 you changed a signature by mistake.

**Doc quality bar** (tw-csharp XML docs): one-sentence `<summary>` stating what the member does, not "Gets or sets X". For builder methods that map to a `dotnet` CLI option, name the flag and summarize its CLI meaning (e.g. "Adds `--configfile` so NuGet reads the given config file instead of the default hierarchy."). For `Git.*` members describe the git behavior and the result record fields. `<param>` for every parameter; `<returns>` where non-obvious. No placeholder text, no copied-from-neighbor summaries that do not match the member.

## Checklist

- [x] `GenerateDocumentationFile` enabled on the CORE csproj (2026-07-05; Tools enables when its burn-down happens)
- [x] **Core burn-down COMPLETE (2026-07-05)**: after the 094 deletes/split only 5 members were missing (CommandResult, Pipe, AppContextExtensions x3) — all documented; ScriptContext documented in 097; ExecutionResult deleted in 092; also fixed malformed XML in PathResolver docs. CS1591 now enforced on core (warnings-as-errors)
- [ ] Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj`
- [ ] Measure first: run `./bin/dev build` and count CS1591 per file (the July figures of ~250 across Workload 77, NuGet 70, New 36, UserSecrets 35, Sln 30 are stale; task 100 added ~130 members since). Paste the per-file count into Results before starting the burn-down
- [ ] Burn down every CS1591 in Tools: `dot-net-commands/*.cs`, `dot-net-commands/tool/*.cs`, `git-commands/*.cs`, `fzf-command/*.cs`, `DotNet.cs` entry points. Meet the doc quality bar above
- [ ] Fix any malformed XML the compiler flags (CS1570/CS1572/CS1573/CS1587) rather than suppressing it
- [ ] `./bin/dev build` 0 warnings / 0 errors; `public-api/PublicAPI.Unshipped.txt` unchanged for both packages (`git diff --stat` shows no public-api change)
- [ ] Pack and verify: `dotnet pack source/timewarp-amuru-tools/timewarp-amuru-tools.csproj -c Release -o /tmp/amuru-pack` then `unzip -l` the nupkg shows `lib/net10.0/timewarp-amuru-tools.xml`; record its size in Results
- [ ] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`; `ganda repo audit` clean
- [x] Core verified: `lib/net10.0/timewarp-amuru.xml` (66KB) present in the nupkg
- [ ] Tools verified: `lib/net10.0/timewarp-amuru-tools.xml` present in the nupkg

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers (API-surface and packaging). Sequence after 094-001 (deletes) and 094-003 (package split) — the split cuts the core-1.0 doc burden from 391 members to roughly the core types' share, and every deleted type is doc work avoided. Build is `TreatWarningsAsErrors`, so enabling the property forces the burn-down per project. Paths relative to `source/timewarp-amuru-tools/`. Large mechanical diff expected (hundreds of `///` blocks); split commits by file family (dotnet, tool, git, fzf) so review can read them.

## Session

- Kitchen refresh: 522eb63d (2026-10-06)
