# Diagnosis: `ganda nuget outdated` crashes with disposed `JsonDocument`

## 1. Symptom

Running `ganda nuget outdated` from the `timewarp-amuru/dev` worktree aborts after printing:

```text
Checking packages in dev...
  [1/15] CliWrap 3.10.1 (up to date)
  [2/15] ModelContextProtocol.Core 1.2.0 -> 1.3.0
Unhandled exception. System.ObjectDisposedException: Cannot access a disposed object.
Object name: 'JsonDocument'.
```

Observed reproduction evidence:

- `ganda nuget outdated` exits with code `134`.
- `ganda nuget outdated --package ModelContextProtocol.Core` exits successfully.
- `ganda nuget outdated --package NuGet.Versioning` exits with the same `ObjectDisposedException`.

## 2. Root Cause

The failing package is `NuGet.Versioning`, not `ModelContextProtocol.Core`.

`TimeWarp.Amuru.NuGetPackageService.AddPageVersionsAsync` stores a `JsonElement` from a fetched NuGet registration page, then exits the block that owns the backing `JsonDocument`, disposing that document before `AddLeafVersions` enumerates the `JsonElement`.

Relevant source: `source/timewarp-amuru/nu-get/nuget-package-service.cs`

- Lines 194-217: external registration page branch fetches and parses `pageDocument`.
- Line 213: `itemsElement` is assigned from `pageDocument.RootElement`.
- Line 217: the block containing `using JsonDocument pageDocument` ends, so `pageDocument` is disposed.
- Line 219: `AddLeafVersions(itemsElement, versions)` receives a `JsonElement` backed by the disposed document.
- Lines 222-243: `AddLeafVersions` reads/enumerates that element and throws `ObjectDisposedException`.

`JsonElement` is a view over its owning `JsonDocument`; it is not valid after that document has been disposed.

## 3. Evidence Chain

1. `Directory.Packages.props` lists packages in this order:
   - line 8: `CliWrap`
   - line 9: `ModelContextProtocol.Core`
   - line 10: `NuGet.Versioning`

2. Ganda's handler increments the package counter, calls `GetLatestVersionsAsync`, and only prints the package status after that call returns:
   - `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-ganda/dev/source/timewarp-ganda/endpoints/nuget/nuget-outdated-command.cs:101-106`
   - output happens later at lines 134 or 149.

3. Therefore the last printed package (`[2/15] ModelContextProtocol.Core`) was completed successfully; the crash occurs while processing package 3, `NuGet.Versioning`, before `[3/15]` can be printed.

4. NuGet registration API shape differs by package:
   - `ModelContextProtocol.Core`: 1 root page, inline `items` present; package-specific command succeeds.
   - `NuGet.Versioning`: 3 root pages, all missing inline `items`; each root page only has an `@id` page URL, forcing the external page-fetch branch.

5. That external page-fetch branch is the only path where `itemsElement` can be assigned from a `JsonDocument` whose lexical scope ends before `AddLeafVersions` runs.

6. Git blame shows the affected `GetVersionsAsync` / `AddPageVersionsAsync` region was introduced by commit `79b4b3c0` (`fix: remove NuGet.Protocol dependency chain`) on 2026-04-26. The working tree was clean during diagnosis.

## 4. Affected Scope

Any caller of `NuGetPackageService` can hit this when checking a package whose NuGet registration index uses external page documents instead of inline leaf items.

Known affected current package:

- `NuGet.Versioning` 7.3.1: all 3 root registration pages require external page fetches.

Known non-triggering packages in the current `Directory.Packages.props` list have inline page `items` and do not exercise the disposed-document path.

## 5. Reproduction Steps

From `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/dev`:

```text
ganda nuget outdated --package NuGet.Versioning
```

Expected observed result: process aborts with `System.ObjectDisposedException: Cannot access a disposed object. Object name: 'JsonDocument'.`

Control reproduction:

```text
ganda nuget outdated --package ModelContextProtocol.Core
```

Expected observed result: command succeeds and reports `1.2.0 -> 1.3.0`.

## 6. Contributing Factors

- NuGet registration responses have two valid shapes: inline leaf `items`, or external page URLs via `@id`.
- The crash only occurs for the external page shape, so packages with inline registration pages mask the defect.
- Ganda prints each package line after the NuGet lookup completes, so the last visible package is not necessarily the failing package.
- The April 2026 replacement of `NuGet.Protocol` with custom registration API traversal introduced direct ownership/lifetime handling for `JsonDocument` and `JsonElement`.

## 7. Related History

- `79b4b3c0` — `fix: remove NuGet.Protocol dependency chain`; owns lines 154-222 of `nuget-package-service.cs`.
- `c7f1a18` — `style: rename nu-get folder files to kebab-case`.
