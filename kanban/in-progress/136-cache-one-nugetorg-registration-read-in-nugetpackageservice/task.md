# Cache one nuget.org registration read in NuGetPackageService

## Description

`NuGetPackageService.GetLatestVersionsAsync` and `SearchAsync` each call private `GetVersionsAsync`. That method downloads `https://api.nuget.org/v3/registration5-gz-semver2/{id}/index.json` and then every external registration page. `GetLatestVersionsAsync` keeps only the highest listed stable version and the highest listed prerelease, and throws the rest away. A second call for the same package pays the full download again. `SearchAsync` (`includeUnlisted: true`) repeats that download even when `GetLatestVersionsAsync` just finished it.

Measured with TimeWarp.Amuru.Tools 2.0.0 on 2026-11-01 (one process, no cache):

- Newtonsoft.Json `GetLatestVersionsAsync`: 0.49s, stable 13.0.4
- Microsoft.Extensions.DependencyInjection `GetLatestVersionsAsync`: 1.20s, stable 10.0.14
- The same call again: 1.09s
- `SearchAsync` for that package: 1.10s, 184 versions
- SkiaSharp `GetLatestVersionsAsync`: 2.91s
- TimeWarp.Nuru `GetLatestVersionsAsync`: 0.30s

Installed ganda `nuget outdated` on timewarp-ganda (37 packages, serial) took 26.71s. That matches one registration walk per package. Ganda task 537 now calls `SearchAsync` once per package per process and derives the latest versions and the band columns from that list, so it no longer calls `GetLatestVersionsAsync` and a second registration fetch for the same package. The remaining cost is still this walk, once per distinct package.

## Requirements

- One registration read per package id for the process, shared by `SearchAsync` and `GetLatestVersionsAsync`. Store the in-flight task so concurrent callers share one request.
- `GetLatestVersionsAsync` still returns the highest listed stable version and the highest listed prerelease, or null when none are listed.
- `SearchAsync` still returns every version, including unlisted, with `Listed` set the same way, or null when nuget.org has no registration.
- Do not cache a failure or a 404 as a successful empty list. A later call may retry. Concurrent callers of the failed attempt still share that one request.
- Public method shapes stay the same.

## Goal (implementer restatement)

`SearchAsync` and `GetLatestVersionsAsync` both walk nuget.org registration for a package id, then keep different slices of that walk. Within one process, the first walk for an id should be the only walk: a second call, the other method, and any caller that arrives while the walk is still running all share that one read.

The stored result is the full version list, including unlisted versions and the listed flag. `SearchAsync` returns that list (or null when the feed has no registration). `GetLatestVersionsAsync` picks the highest listed stable version and the highest listed prerelease from the same list, and returns null when nothing listed is present. Unlisted versions stay out of the latest-version result.

A successful walk stays stored for the process. A 404 and a failed walk do not stay stored as an empty success. Callers that joined the failed attempt all see that one outcome. A call that starts after the attempt has finished may try again. `SearchAsync` and `GetLatestVersionsAsync` keep the signatures they have now.

## Checklist

- [x] Share one registration read per package id
- [x] `GetLatestVersionsAsync` derives from that list and stays listed-only
- [x] `SearchAsync` derives from that list and still includes unlisted versions
- [x] A repeat call does not hit the network
- [x] Tests for one download, the listed filter, and not-found

## Session

- Created: 1664621 (2026-11-01)
- Implementation: grok task-work (2026-11-07)
- Audit: `ganda repo audit --fix` wrote the gitignored `.vscode/settings.json` window title. No tracked file changed. A follow-up audit passed (38 passed, 2 skipped).
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1b678-ee95-7af2-93ef-376737647773, max-turns 500, Done — 2026-11-07T15:08:03Z

## Notes

Filed from timewarp-ganda task 537. Do not fix this inside ganda. Ganda task 538 is the disk cache, ETag, and flat-container follow-up on that side.

## Results

`NuGetPackageService` keeps one registration walk per package id. The parameterless constructor stores that walk for the process, including the in-flight task, so `SearchAsync` and `GetLatestVersionsAsync` share it. The stored list keeps unlisted versions. Latest selection still uses listed versions only. A 404 and a failed walk are removed when the attempt finishes, so a later call can retry, and callers that already joined that attempt see the same result. A caller's cancellation token cancels only that caller's wait.

The new `NuGetPackageService(HttpMessageHandler)` constructor keeps an isolated cache so tests can count requests without filling the process cache. The caller owns the handler. `SearchAsync` and `GetLatestVersionsAsync` keep their existing signatures.

### How to validate

**Smoke**

```bash
dotnet run --file tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.registration-cache.cs
dotnet run --file tests/timewarp-amuru/single-file-tests/repo-services/nuget-package-service.cs
```

**Expect**

- Registration cache: 9 passed. Repeat `SearchAsync` / `GetLatestVersionsAsync` for one id downloads the index and its external page once. Latest stable and prerelease ignore higher unlisted versions. A 404 and an HTTP failure return control to the caller and the next call hits the network again. Two overlapping callers share one in-flight request. Cancelling one caller leaves the shared walk for the other.
- Live service tests: 31 passed. In one process, the first `SearchAsync("Newtonsoft.Json")` took 0.63s and the next search of that id took 0.00s. `GetLatestVersionsAsync("Newtonsoft.Json")` on a new instance also took 0.00s. A missing package id still took a network round trip (0.66s) and returned null.

**Automated**

```bash
dotnet run --file tests/timewarp-amuru/multi-file-runners/run-tests.cs
# this session: Passed 741, Skipped 1, Failed 0
```

The skip is the existing `GetCommitsAheadOfDefaultBranch_Given_` test. `RegistrationCache_Given_` passed 9 in that run.
