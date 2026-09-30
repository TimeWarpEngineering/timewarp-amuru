# Round 1 — general
**Date:** 2026-09-30
**Scope reviewed:** commit `b775361` (skill, DotNet reference, workflow path filters, aggregate Compile glob, `TestsDirectory`, test `Directory.Build.props`) checked against current builders

## Summary

M21’s named contradictions are gone: the skill no longer claims authority, default validation is `None`, and `ExecutionResult` / `AsJsonRpcClient` are absent. M22–M26 match the brief (pull_request paths only; push to master stays unfiltered on purpose; `**/*.cs` covers the same tree the old folder list named, with no extra folders today; `tests/` casing; no `Console.` left under the test tree). The rewritten samples still teach calls that do not compile.

## Issues

### Issue 1 — Severity: bug
- File: skills/amuru/SKILL.md:187
- Description: Fzf samples call `WithInputItems` and `WithInputCommand`. Neither method exists. Input is `FromInput` and `FromCommand` (`source/timewarp-amuru-tools/fzf-command/Fzf.InputMethods.cs`). `WithHeader`, `WithPreview`, and `SelectAsync` are real. These lines were not in the hunk, but this commit rewrote the skill to match current types and left a non-compiling sample in the same file.
- Suggestion: Use `FromInput(...)` and `FromCommand(...)`.
- Status: open

### Issue 2 — Severity: bug
- File: skills/amuru/SKILL.md:17
- Description: The only package directive is `TimeWarp.Amuru`. `DotNet`, `Git`, and `Fzf` (including the `WithSingleFile` / `WithTrimmed` samples this commit corrected) ship in `TimeWarp.Amuru.Tools`. A runfile that follows the Package section cannot compile those samples.
- Suggestion: Document `#:package TimeWarp.Amuru.Tools` next to core, and say which types live in which package. Namespace stays `TimeWarp.Amuru`.
- Status: open

### Issue 3 — Severity: bug
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md:178
- Description: “These exist on the builders above” includes `WithProperty`. Build, Clean, Restore, Run, Test, Publish, and Pack have it. `DotNet.ListPackages`, `DotNet.AddPackage`, and `DotNet.RemovePackage` do not. `WithWorkingDirectory`, `WithEnvironmentVariable`, and `WithNoValidation` do exist on every builder in the reference. The claim that builders do not expose `WithZeroExitCodeValidation()` holds.
- Suggestion: Say which builders have `WithProperty`. Keep the sample on `DotNet.Build()`, which does.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-amuru-tools/dot-net-commands/dot-net.md:25
- Description: `RunAndCaptureAsync` is documented only on build, clean, and `DotNet.WithVersion()` / `WithListSdks()` / `WithInfo()`. `DotNetPackBuilder` also defines it (`DotNet.Pack.cs`). “Other builders expose `RunAsync` and `CaptureAsync`” is false for Pack. Test, Run, Publish, Restore, ListPackages, AddPackage, and RemovePackage really do not have `RunAndCaptureAsync`.
- Suggestion: Include pack in the `RunAndCaptureAsync` sentence.
- Status: open
