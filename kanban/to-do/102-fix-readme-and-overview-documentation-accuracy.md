# Fix readme and overview documentation accuracy

## Description

The readme ships INSIDE the nupkg, so its non-compiling samples land on the nuget.org package page. Spot-check against `source/timewarp-amuru/core/` found the flagship examples use APIs that don't exist.

## Checklist

### readme.md (MAJOR — ships in the package)
- [ ] Multi-arg `Shell.Builder("npm", "install")` used throughout (lines 72, 75, 82, 88, 94, 265, 270, 280, 295, 319, 326, 375, 378, 384, 428, 439) — actual API is `Shell.Builder(string executable)` + `.WithArguments(...)` (`core/shell.cs:26`, `core/shell-builder.cs:31`)
- [ ] `:385` `.WithValidation(CommandResultValidation.None)` → real method is `WithNoValidation()` (align with task 090's outcome)
- [ ] `:105` `.WithCancellationToken(...)` → doesn't exist; token is passed to `RunAsync`/`CaptureAsync`
- [ ] `:404,409` `.WithTimeout(TimeSpan)` → not on ShellBuilder (only `DotNetNuGetPushBuilder`); task 044 tracks adding real timeout support — sync docs with whatever ships
- [ ] `:91,307` `result.Lines` → real API is `GetLines()`/`GetStdoutLines()`/`OutputLines`
- [ ] `:470-510` broken relative links using dead PascalCase paths (`Documentation/...`, `Analysis/MigrationGuide.md`, `Spikes/CsScripts/`, `Source/TimeWarp.Multiavatar/README.md`, `Kanban/Overview.md`); `:504` labels a link "TimeWarp.Zana" but points at timewarp-ganda

### overview.md (MAJOR)
- [ ] `:30,77-90,118-122,352,448-462` document `.GetStringAsync()`, `.GetLinesAsync()`, `.ExecuteAsync()`, `.SaveAs()` — none exist; large sections (~126-463) are unshipped "vision" content with nothing flagging it. Rewrite against the real API or clearly mark vision sections

### Package split (from 094-003)
- [ ] Installation section shows BOTH packages (TimeWarp.Amuru core, TimeWarp.Amuru.Tools for DotNet/Git/Fzf builders); wrapper examples note the Tools package

### documentation/
- [ ] `documentation/overview.md:10-16,22-23` — dead PascalCase links, wrong sample filenames
- [ ] `documentation/developer/reference/shell-commands.md:265`, `documentation/conceptual/architectural-layers.md:938` — links into the now-separate ganda repo
- [ ] `documentation/conceptual/design/json-rpc-support-design.md` — still documents `AsJsonRpcClient()`/StreamJsonRpc as active; feature is disabled (task 084's open checklist item — mark disabled pending task 083)
- [ ] Consider a doc-sample compile check (samples/ are already referenced via `#:project` and verified current — extend that pattern or extract readme snippets into compiled samples)

## Notes

Found by multi-agent release review (2026-07-04). Verified correct already: `CaptureAsync()`→`CommandOutput` members, `CliConfiguration.*`, `DotNet.*`, `Fzf.*`, `When/Unless/Tap` docs, the ganda rename (task 030) fully applied, samples/ current.
