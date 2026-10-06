# Upgrade TimeWarp.Nuru to 3.0.0-beta.79 and migrate dev-cli endpoints

## Description

`TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` `3.0.0-beta.79` was published 2026-10-06 as the dogfood release before 3.0. The ganda audit check `nuru` ("TimeWarp.Nuru package is latest") has default severity **Error**, so from now on every `ganda repo attest` (post-commit hook) fails to sign and every `ganda task work` walk's audit auto-fix bumps the pins to beta.79. Task 082's walk did exactly that and CI went red: `tools/dev-cli` no longer compiles because beta.79 moved the message/handler contracts out of Nuru. The bump was reverted on 082 to keep that PR a pure rename; this task does the upgrade properly.

Only `tools/dev-cli/**` consumes Nuru in this repo (`source/`, `tests/`, `samples/`, `.githooks/` have no reference). `bin/dev` is the AOT publish of that dev-cli (`self-install`), so the migration must stay AOT-clean.

## Migration (from the Nuru changelog, 3.0.0-beta.79, "BREAKING: message and handler contracts moved to TimeWarp.Mediator")

- Nuru no longer defines `IMessage`, `IQuery<T>`, `ICommand<T>`, `IIdempotentCommand<T>`, `IIdempotent`, `IQueryHandler<,>`, `ICommandHandler<,>`, `IIdempotentCommandHandler<,>`, or `Unit`. They come from `TimeWarp.Mediator` 14.0.0-beta.4 (`TimeWarp.Mediator.Contracts` + `TimeWarp.Mediator.Generators`, transitive dependencies of Nuru beta.79).
- Add `global using TimeWarp.Mediator;` next to `global using TimeWarp.Nuru;` in `tools/dev-cli/global-usings.cs`.
- **Drop** `global using static TimeWarp.Nuru.Unit;` (it no longer exists, and a static using of `TimeWarp.Mediator.Unit` would hide `System.Threading.Tasks.Task`). Use `Unit.Value` / `Unit.Task` explicitly.
- Handlers: `ValueTask<Unit> Handle(...)` → `Task<Unit> Handle(...)`. In non-async handlers replace `return default;` with `return Unit.Task;` and `new ValueTask<T>(v)` with `Task.FromResult(v)`. Async handlers that `return default;` / `return Unit.Value;` keep `Unit.Value`.
- `[NuruRoute]` endpoint classes must be `public` (the Mediator generator emits public `Send(TRequest)` overloads; `internal` requests fail CS0051). Amuru's endpoints are already `public sealed`; confirm each.
- Apps keep a single `AddGeneratedMediator()` in their own compilation; with `.UseMicrosoftDependencyInjection()` Nuru calls it automatically. Check `tools/dev-cli/dev.cs` host setup matches what `timewarp-nuru/master/tools/dev-cli/dev.cs` does at beta.79 and mirror it.
- Reference implementation: `timewarp-nuru/master/tools/dev-cli/` (endpoints + `global-usings.cs`) is already on beta.79. Diff its shape against amuru's and apply the same changes; do not copy unrelated content.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` → `3.0.0-beta.79`. Add a CPM `PackageVersion` for `TimeWarp.Mediator.Contracts` / `TimeWarp.Mediator.Generators` **only if** restore reports NU1008/NU1010 (CPM wants explicit versions for direct references); if they flow transitively, do not pin them.
- All `tools/dev-cli/endpoints/*.cs` compile under beta.79 with the handler signature changes above; no `#pragma` or `NoWarn` additions.
- `dotnet run --file tools/dev-cli/dev.cs -- --capabilities` works; `dotnet run --file tools/dev-cli/dev.cs -- self-install` produces `bin/dev`; `./bin/dev --capabilities` lists the same 7+ commands the audit `dev-cli-capabilities` check expects; `./bin/dev build`, `./bin/dev test`, `./bin/dev check-version`, `./bin/dev workflow --mode merge` all run.
- AOT: self-install publish emits no IL2xxx/IL3xxx trim/AOT warnings beyond those already present on master (compare counts).
- `ganda repo audit` passes with the `nuru` check green and no advisory about Nuru.
- `public-api/` untouched (dev-cli is not a package). `source/` untouched unless the build proves otherwise; explain any source change in Results.
- Commit hook: the first commit after the pin bump should be **attested** (bin/dev present, audit green). Paste the "Attested tree" line into Results as proof the hook is healthy again.

## Checklist

- [x] Pins bumped to 3.0.0-beta.79 (Nuru + DevCli); Mediator pins only if restore demands them
- [x] `global-usings.cs`: add `TimeWarp.Mediator`, drop `static TimeWarp.Nuru.Unit`
- [x] Every endpoint handler migrated `ValueTask<Unit>` → `Task<Unit>`; `return default` → `Unit.Task` / `Unit.Value` as appropriate; endpoints `public`
- [x] `dev.cs` host setup matches Nuru's beta.79 dev-cli (mediator registration)
- [x] `dotnet run --file tools/dev-cli/dev.cs -- --capabilities` ok; `self-install` ok; `./bin/dev --capabilities` ok
- [x] `./bin/dev build` and `workflow --mode merge` exit 0; `test` 620 passed / 1 skipped; `check-version` runs and reports 2.0.0-beta.1 already released (exit 1)
- [x] AOT publish warning count unchanged vs master (record both numbers)
- [x] `ganda repo audit` green including `nuru`
- [x] Post-commit hook attests (paste the Attested line)
- [x] Results: per-file change summary, any CPM pins added and why, AOT warning counts, audit output

## Notes

- Origin: task 082 walk (2026-10-06) — audit auto-fix bumped Nuru to beta.79, CI red with CS0234/CS0246 on `Unit`, `ICommand<>`, `ICommandHandler<,>` in `tools/dev-cli`. Reverted on 082 (`build: keep TimeWarp.Nuru at 3.0.0-beta.76`).
- Until this merges, attestation fails on every commit in every fresh worktree (audit `nuru` = Error). Land this first; other tasks wait.
- Nuru 3.0 migration guide: `timewarp-nuru/master/documentation/user/guides/migrating-to-3.0.md`.
- `TimeWarp.Jaribu 1.0.0-beta.15` and `TimeWarp.Terminal 1.0.2` are out of scope; bump only if beta.79 forces it (explain in Results).

## Results

Upgraded `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from `3.0.0-beta.76` to `3.0.0-beta.79` and migrated `tools/dev-cli` onto the Mediator handler contracts. `source/` and `public-api/` are unchanged. `TimeWarp.Jaribu` and `TimeWarp.Terminal` stayed at `1.0.0-beta.15` and `1.0.2`; beta.79 did not force a bump.

CPM pins added: none. Restore of the dev-cli runfile succeeded with no NU1008/NU1010. `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Generators` `14.0.0-beta.4` flow transitively from Nuru (seen in the runfile `project.assets.json`). They are not direct `PackageReference`s, so they are not pinned.

`tools/dev-cli/dev.cs` is unchanged. Nuru's beta.79 `tools/dev-cli/dev.cs` uses the same host shape: `ConfigureServices` for the DevCli services, then `DiscoverEndpoints()`, with no explicit `AddGeneratedMediator()` and no `UseMicrosoftDependencyInjection()`. Source-generated DI still registers those singletons. Capabilities, self-install, build, test, and workflow all ran on that host.

### Per-file

- `Directory.Packages.props`: Nuru and Nuru.DevCli `3.0.0-beta.76` → `3.0.0-beta.79`.
- `tools/dev-cli/global-usings.cs`: added `global using TimeWarp.Mediator;`, removed `global using static TimeWarp.Nuru.Unit;`.
- `tools/dev-cli/endpoints/build-command.cs`: `ValueTask<Unit>` → `Task<Unit>`. Already returned `Unit.Value`. Already `public`.
- `tools/dev-cli/endpoints/test-command.cs`: same signature change. Already `public`.
- `tools/dev-cli/endpoints/clean-command.cs`: `Task<Unit>`, `return Value` → `return Unit.Value`. Already `public`.
- `tools/dev-cli/endpoints/verify-samples-command.cs`: endpoint and handler `internal` → `public` (CS0051 on generated `Send`). `Task<Unit>`. `return Value` → `return Unit.Value`. Parameter renamed `ct` → `cancellationToken` because Mediator's `IRequestHandler.Handle` names it that way (CA1725, warnings-as-errors).
- `tools/dev-cli/endpoints/workflow-command.cs`: same public + `Task<Unit>` + `Unit.Value` migration. `cancellationToken` rename, and `ArgumentNullException.ThrowIfNull(command)` because the now-public handler tripped CA1062.

No `#pragma` or `NoWarn` additions.

### Validation run

- `dotnet run --file tools/dev-cli/dev.cs -- --capabilities`: exit 0. Eight commands: build, clean, test, verify-samples, workflow, check-version, release, self-install.
- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: installed `bin/dev`. `./bin/dev --capabilities` lists the same eight commands.
- `./bin/dev build`: exit 0, 0 warnings.
- `./bin/dev test`: exit 0. Total 621, Passed 620, Skipped 1.
- `./bin/dev check-version`: exit 1 with "Version 2.0.0-beta.1 was already released" for TimeWarp.Amuru and TimeWarp.Amuru.Tools. That is the release guard for the current props version, which is already on NuGet. This task does not bump the package version. The command ran and reported that state.
- `./bin/dev workflow --mode merge`: exit 0. Clean, build, both samples, and tests (620 passed / 1 skipped). Pipeline SUCCEEDED.

### AOT warning counts

`dotnet publish tools/dev-cli/dev.cs` (the command `self-install` runs), Release, compared to the master worktree at `75c8489`:

| Tree | Publish exit | `warning IL2xxx` / `warning IL3xxx` lines | Any `warning` lines |
| --- | --- | --- | --- |
| master (beta.76) | 0 | 0 | 0 |
| this task (beta.79) | 0 | 0 | 0 |

IL2026, IL2104, IL3050, and IL3053 stay in the existing dev-cli `NoWarn`. No new trim/AOT warnings were emitted.

### Audit

`ganda repo audit` exit 0. Passed 31, Failed 1, Skipped 0. Summary: "Repository passes — 1 advisory (non-blocking) warning(s)."

- `nuru`: PASS, "TimeWarp.Nuru is up to date". No Nuru advisory.
- `dev-cli-capabilities`: PASS. `bin-dev`: PASS.
- The one advisory is pre-existing `memsearch-scaffold` (Warning): `.githooks/post-commit.cs`, `post-merge.cs`, and `post-checkout.cs` are outdated relative to the memsearch scaffold. Those hooks intentionally do not index memsearch (comment cites task 339). Left unchanged.

### Attestation

The pin-bump commit is `4544352f15bea156a4ac9fb9ee3e90e0ceb42cb5`. The post-commit hook skipped signing because the folderized kitchen was still dirty. `ganda repo attest` on the clean committed tree then printed:

```
Attested tree 7670da8cbcea (key_id=tw-audit-1)
```

Notes were pushed. The status post warned: "Failed to post ganda/attestation commit status" (GitHub response parse error). The attestation note itself was pushed.

### How to validate

Smoke:

- `dotnet run --file tools/dev-cli/dev.cs -- --capabilities`
- `./bin/dev --capabilities`
- `./bin/dev build`
- `./bin/dev test`
- `./bin/dev workflow --mode merge`
- `ganda repo audit`

Expect:

- Capabilities JSON lists build, clean, test, verify-samples, workflow, check-version, release, and self-install.
- Build exits 0 with 0 warnings.
- Tests report Passed 620, Skipped 1.
- `workflow --mode merge` prints `Pipeline SUCCEEDED`.
- Audit exits 0 and the `nuru` row is PASS ("TimeWarp.Nuru is up to date").

### Review

- Rounds: 1. Effort 1, roster: general.
- Final counts: bug 0, suggestion 0, nit 0 (all statuses 0).
- Disposition: **clean** (no findings raised).
- Artifacts: `review/review-framework.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Session

- Created: 522eb63d (2026-10-06)
- Implementation: grok 01a1104c (2026-10-06)
- Review: claude review oracle (2026-10-06), disposition clean
