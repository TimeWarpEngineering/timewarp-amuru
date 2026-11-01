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

## Checklist

- [ ] Share one registration read per package id
- [ ] `GetLatestVersionsAsync` derives from that list and stays listed-only
- [ ] `SearchAsync` derives from that list and still includes unlisted versions
- [ ] A repeat call does not hit the network
- [ ] Tests for one download, the listed filter, and not-found

## Session

- Created: 1664621 (2026-11-01)

## Notes

Filed from timewarp-ganda task 537. Do not fix this inside ganda. Ganda task 538 is the disk cache, ETag, and flat-container follow-up on that side.
