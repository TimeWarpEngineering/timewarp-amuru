# Round 1 — general
**Date:** 2026-11-07
**Scope reviewed:** same as framework (branch vs merge base `7b5cba1`)

## Summary

`NuGetPackageService` now stores one `Task<RegistrationSnapshot>` per lower-cased package id. The parameterless constructor uses a static process-wide `ConcurrentDictionary`. The new `NuGetPackageService(HttpMessageHandler)` constructor uses a per-instance dictionary. The walk runs on `Task.Run` behind a `TaskCompletionSource`, so the in-flight task is published before the walk can finish. A 404 or a thrown walk completes the shared task and then removes it with a compare-and-remove. `IsReusable` also refuses a finished not-found or faulted entry that is still in the dictionary. Callers wait through `WaitAsync(cancellationToken)`, so one caller's cancellation does not cancel the shared HTTP walk. The walk itself runs with `CancellationToken.None` and relies on the `HttpClient` 100s default timeout.

All five requirements hold:

- One read per id, shared across both methods and in-flight callers.
- Listed-only latest selection, with null when nothing listed.
- `SearchAsync` keeps unlisted versions and returns null on 404 or an empty list.
- 404 and failures are not cached.
- Existing method signatures are unchanged.

The only public addition is the handler constructor, recorded in `PublicAPI.Unshipped.txt`. Version parsing, ordering, the pre-existing `#pragma warning disable IDE0007`, and the tolerance for a page 404 are all carried over from the old code. The two new `[SuppressMessage]` attributes are justified and follow repo practice:

- CA1031: the catch-all forwards the exception into the shared TCS and is effectively a rethrow.
- CA2000: `HttpMessageInvoker` with `disposeHandler: false` holds nothing. The service is not `IDisposable`.

Neither suppression hides work that the rule measures.

The implementer's restatement in `## Goal (implementer restatement)` matches the task. No misread.

Verified locally:

- `nuget-package-service.registration-cache.cs`: 9/9 passed.
- `multi-file-runners/run-tests.cs`: total 742, passed 741, skipped 1 (the existing `GetCommitsAheadOfDefaultBranch_Given_` skip), failed 0.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-amuru-tools/nu-get/nuget-package-service.cs:430
- Description: An external registration page that returns 404 is skipped silently, as before. The walk then completes as `Found` with a partial version list, and that list now stays in the process cache. Before this change, a later call in the same process re-walked and could recover. Now a transient page 404, for example during a nuget.org catalog update, sticks for the process lifetime. The index-level 404 and thrown failures are handled correctly. Only this partial-success path is cached.
- Suggestion: Optionally mark a snapshot as incomplete when a page is skipped, and have `ReadAndPublishAsync` and `IsReusable` treat an incomplete snapshot like not-found. Callers still get the partial list for this call, but the next call walks again.
- Status: open
