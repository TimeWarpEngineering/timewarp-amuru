# Fix JsonDocument disposed before JsonElement is read in NuGetPackageService

## Description

`ganda nuget outdated` crashes with `System.ObjectDisposedException: Cannot access a disposed object. Object name: 'JsonDocument'` when checking packages whose NuGet registration index uses external page documents (no inline leaf `items`).

The crash occurs in `AddLeafVersions` because `AddPageVersionsAsync` extracts a `JsonElement` from a `JsonDocument` and then disposes that document before `AddLeafVersions` can enumerate it. `JsonElement` is a view over its owning `JsonDocument` and becomes invalid once the document is disposed.

## Checklist

- [ ] Fix `AddPageVersionsAsync` so `itemsElement` from an external page fetch remains valid when passed to `AddLeafVersions`
- [ ] Verify `AddLeafVersions` no longer receives disposed `JsonElement` for any NuGet registration page shape
- [ ] Reproduce the fix: `ganda nuget outdated --package NuGet.Versioning` succeeds without crash
- [ ] Verify no regression: `ganda nuget outdated --package ModelContextProtocol.Core` still works
- [ ] Run full `ganda nuget outdated` without crash

## Notes

### Root Cause

In `source/timewarp-amuru/nu-get/nuget-package-service.cs`:

- Line 211: `using JsonDocument pageDocument = await ReadJsonDocumentAsync(...)` — page document is fetched and parsed
- Line 213: `itemsElement` is assigned from `pageDocument.RootElement`
- Line 217: the block containing the `using` declaration ends, disposing `pageDocument`
- Line 219: `AddLeafVersions(itemsElement, versions)` receives a `JsonElement` backed by the now-disposed document
- Lines 222-243: `AddLeafVersions` reads/enumerates the element and throws `ObjectDisposedException`

### Why CliWrap and ModelContextProtocol.Core don't trigger it

Those packages have inline `items` arrays in their NuGet registration index — the `TryGetProperty("items", ...)` check at line 194 succeeds, so the external page-fetch branch (lines 196-217) is never entered for them. `NuGet.Versioning` has 3 root pages, all lacking inline `items`, forcing the external fetch path on every page.

### Reproduction

```text
ganda nuget outdated --package NuGet.Versioning   # crashes
ganda nuget outdated --package ModelContextProtocol.Core  # succeeds
```

Full diagnosis: `.agent/workspace/2026-05-13T00-00-00_diagnosis-ganda-nuget-outdated-jsondocument-disposed.md`

### History

- Commit `79b4b3c0` — `fix: remove NuGet.Protocol dependency chain`; owns lines 154-222 of `nuget-package-service.cs`
- Commit `c7f1a18` — `style: rename nu-get folder files to kebab-case`
