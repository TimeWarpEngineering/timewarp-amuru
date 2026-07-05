# Split TimeWarp.Amuru.Tools package

## Description

Split the repo into two packages so the process-automation core can go 1.0 stable without freezing the tool-wrapper surface (~4x larger, tracks external CLIs that change under us, holds the 087/099/100 work):

- **TimeWarp.Amuru** (core, → 1.0 stable): `core/`, `testing/`, `interfaces/`, `native/` (file-system, aliases, PathResolver), `ScriptContext`, `CliConfiguration`, `AppContextExtensions`
- **TimeWarp.Amuru.Tools** (→ stays beta until 087/099/100 land): `dot-net-commands/`, `git-commands/`, `fzf-command/`, `repo/`, `nu-get/`

Root namespace stays `TimeWarp.Amuru` for both — consumers only add a package reference, no code edits. `repo/`/`nu-get/` stay PUBLIC in Tools because every TimeWarp repo's dev-cli consumes them and public repos cannot depend on private packages (Zana) — see parent 094 decision record.

## Checklist

- [ ] Create `source/timewarp-amuru-tools/` project; move `dot-net-commands/`, `git-commands/`, `fzf-command/`, `repo/`, `nu-get/` (namespaces unchanged)
- [ ] Tools references core; core drops the `NuGet.Versioning` dependency (only repo/nu-get use it); check where `TimeWarp.Terminal` lands after 092 decides `ExecutionResult.WriteToConsole`'s fate
- [ ] Package metadata for Tools (id, description, tags, readme section, icon)
- [ ] Versioning: core and Tools get independent `<Version>` properties. NOTE: deviates from the owner's single-version-per-repo convention — add/confirm a `ganda repo audit` exception for this repo, and document the deviation in the repo readme
- [ ] Update `timewarp-amuru.slnx`, dev-cli build/test/pack endpoints, and CI workflow to build+push both packages
- [ ] Split test tree accordingly (`tests/timewarp-amuru/` vs `tests/timewarp-amuru-tools/`) or tag runner groups; keep aggregate runner covering both
- [ ] Readme: installation section shows both packages; wrapper examples note the Tools package
- [ ] Update consuming repos' dev-clis to add the Tools reference (one PackageReference each, no code changes)
- [ ] Re-verify nuspec of BOTH packages: stable deps only for core

## Notes

Decision 2026-07-05 (parent 094): two packages, not per-tool — subdividing a beta Tools package later is cheap and breaks no promises. Gates: this task blocks core 1.0 (093's doc scope and 096's AOT scope depend on where types land). Tasks 087/099/100 become Tools-stability gates, not core-1.0 gates.

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
