# Enable XML documentation generation and document public API

## Description

`GenerateDocumentationFile` is set nowhere (`Directory.Build.props`, `msbuild/*.props`, `timewarp-amuru.csproj`) — verified: the packed nupkg contains no `lib/net10.0/timewarp-amuru.xml`. Consumers of a stable 1.0 library get zero IntelliSense docs despite extensive `///` comments in the source, and CS1591 enforcement is off despite `AnalysisMode=All`. Measured with docs force-enabled: **391 public members missing XML docs**.

**BLOCKER for 1.0.**

## Checklist

- [ ] Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `source/Directory.Build.props`
- [ ] Burn down CS1591 (build is `TreatWarningsAsErrors`, so this lands as one deliberate task). Top offenders: `DotNet.Workload.cs` (77), `DotNet.NuGet.cs` (70), `DotNet.New.cs` (36), `DotNet.UserSecrets.cs` (35), `DotNet.Sln.cs` (30), `execution-result.cs` (10), `CommandResult` class and `Pipe` method (`core/command-result.cs:42,261`), all 6 public members of `ScriptContext`
- [ ] Verify the `.xml` ships in the nupkg after the change

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers (API-surface and packaging). Sequence after task 094 (surface scoping) so effort isn't spent documenting members that get internalized or cut.
