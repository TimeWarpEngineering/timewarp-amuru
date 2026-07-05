# Enable XML documentation generation and document public API

## Description

`GenerateDocumentationFile` is set nowhere (`Directory.Build.props`, `msbuild/*.props`, `timewarp-amuru.csproj`) — verified: the packed nupkg contains no `lib/net10.0/timewarp-amuru.xml`. Consumers of a stable 1.0 library get zero IntelliSense docs despite extensive `///` comments in the source, and CS1591 enforcement is off despite `AnalysisMode=All`. Measured with docs force-enabled: **391 public members missing XML docs**.

**BLOCKER for 1.0.**

## Checklist

- [ ] Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `source/Directory.Build.props`
- [ ] **Core package first (gates 1.0)**: burn down CS1591 for the ~31 core types — `CommandResult` class and `Pipe` method (`core/command-result.cs:42,261`), all 6 public members of `ScriptContext`, `execution-result.cs` (10), remaining core/testing/native members
- [ ] **Tools package (gates Tools stable, not core 1.0)**: the bulk — `DotNet.Workload.cs` (77), `DotNet.NuGet.cs` (70), `DotNet.New.cs` (36), `DotNet.UserSecrets.cs` (35), `DotNet.Sln.cs` (30), rest of the wrappers
- [ ] Verify the `.xml` ships in each nupkg after the change

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers (API-surface and packaging). Sequence after 094-001 (deletes) and 094-003 (package split) — the split cuts the core-1.0 doc burden from 391 members to roughly the core types' share, and every deleted type is doc work avoided. Build is `TreatWarningsAsErrors`, so enabling the property forces the burn-down per project.
