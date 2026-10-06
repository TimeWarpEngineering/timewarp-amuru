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

- [ ] Pins bumped to 3.0.0-beta.79 (Nuru + DevCli); Mediator pins only if restore demands them
- [ ] `global-usings.cs`: add `TimeWarp.Mediator`, drop `static TimeWarp.Nuru.Unit`
- [ ] Every endpoint handler migrated `ValueTask<Unit>` → `Task<Unit>`; `return default` → `Unit.Task` / `Unit.Value` as appropriate; endpoints `public`
- [ ] `dev.cs` host setup matches Nuru's beta.79 dev-cli (mediator registration)
- [ ] `dotnet run --file tools/dev-cli/dev.cs -- --capabilities` ok; `self-install` ok; `./bin/dev --capabilities` ok
- [ ] `./bin/dev build`, `test` (expect 620 passed / 1 skipped), `check-version`, `workflow --mode merge` all green
- [ ] AOT publish warning count unchanged vs master (record both numbers)
- [ ] `ganda repo audit` green including `nuru`
- [ ] Post-commit hook attests (paste the Attested line)
- [ ] Results: per-file change summary, any CPM pins added and why, AOT warning counts, audit output

## Notes

- Origin: task 082 walk (2026-10-06) — audit auto-fix bumped Nuru to beta.79, CI red with CS0234/CS0246 on `Unit`, `ICommand<>`, `ICommandHandler<,>` in `tools/dev-cli`. Reverted on 082 (`build: keep TimeWarp.Nuru at 3.0.0-beta.76`).
- Until this merges, attestation fails on every commit in every fresh worktree (audit `nuru` = Error). Land this first; other tasks wait.
- Nuru 3.0 migration guide: `timewarp-nuru/master/documentation/user/guides/migrating-to-3.0.md`.
- `TimeWarp.Jaribu 1.0.0-beta.15` and `TimeWarp.Terminal 1.0.2` are out of scope; bump only if beta.79 forces it (explain in Results).

## Session

- Created: 522eb63d (2026-10-06)
