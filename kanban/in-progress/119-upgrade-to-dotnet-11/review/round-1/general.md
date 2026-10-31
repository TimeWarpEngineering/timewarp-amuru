# Round 1 — general
**Date:** 2026-10-31
**Scope reviewed:** same as the framework. That is the product diff `53950d7..75b0006` (25 files) plus the claims in `task.md`.

## Summary

The retarget follows the restated goal. `global.json`, the root and `tools/` `Directory.Build.props`, NetAnalyzers, `<Version>` 3.0.0, the release note, and the docs all move to .NET 11. The only `net10.0` strings left are example monikers passed to the dotnet builder APIs, plus history (blips and `.agent/workspace`). Ubuntu CI run 41181587366 is green on `6bdf9c2`. The goal restatement matches the task, including Steve's 2026-10-31 RC go-live overrule, so there is no misread.

There is one bug. The five CA1307 `#pragma` suppressions rest on a false claim, "net11.0 has no StringComparison overload". I disproved that on this host. Two nits concern the scope of a suppression and doc accuracy.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/native/file-system/file-system-walk.cs:53. The same pattern is at `source/timewarp-amuru-tools/nu-get/nuget-package-service.cs:124`, `source/timewarp-amuru/native/file-system/glob-matcher.cs:35`, `source/timewarp-amuru/native/text/text-diff.cs:259`, and `source/timewarp-amuru/native/text/text-diff.cs:279`.
- Description: Five `#pragma warning disable CA1307` blocks silence analyzer rule **CA1307** (Specify StringComparison for clarity). The comment on each block, Notes "CA1307 and CA1865", and `## Results` all say `net11.0` has no `StartsWith(char, StringComparison)`, `EndsWith(char, StringComparison)`, or `LastIndexOf(char, int, StringComparison)`. That is false. Under SDK `11.0.100-rc.3.26522.148`, a file-based app targeting `net11.0` compiles `s.StartsWith('v', StringComparison.Ordinal)`, `s.EndsWith('\n', StringComparison.Ordinal)`, and `s.LastIndexOf('/', 5, StringComparison.Ordinal)` with `TreatWarningsAsErrors`, `AnalysisMode=All`, `AnalysisLevel=latest-all`, and `Microsoft.CodeAnalysis.NetAnalyzers@11.0.100-rc.3.26522.148`. That build has no CA1307 and no CA1865. The `StartsWith('v')` form fails with CA1307, and the message names the overload: "Replace this call … with a call to 'string.StartsWith(char, System.StringComparison)'". Reflection on runtime 11.0.0 lists those overloads, and the Rune variants too. The ref-pack `System.Runtime.xml` has no doc entries for them, which is probably how the false claim got started. The suppressions do nothing except make the analyzer pass. They skip the change the analyzer asks for, so this is a bug under the suppression rule.
- Suggestion: At all five sites, pass `StringComparison.Ordinal` to the char overload: `version.StartsWith('v', StringComparison.Ordinal)`, `name.StartsWith('.', StringComparison.Ordinal)`, `normalized.LastIndexOf('/', firstGlob, StringComparison.Ordinal)`, and `EndsWith('\n', StringComparison.Ordinal)` twice. Delete the pragmas and their comments. The temporary locals that exist only to hold a pragma span (`startsWithDot`, `endsWithLineFeed`, `lineFeedText`) can collapse back to the original expressions. In `task.md`, correct Notes "CA1307 and CA1865", the checklist line "The new CA1307 findings on char overloads are suppressed at the call", and the matching sentence in `## Results`. Run the Release build and `dev test` again.
- Status: open

### Issue 2 — Severity: nit
- File: tools/dev-cli/Directory.Build.props:28
- Description: CA1812 (internal class is never instantiated) goes into the project-wide `NoWarn` for the whole dev CLI. The class that needs it is `WorkflowCommand.Handler`, which became internal so it matches the DevCli 4.0.0-beta.10 internal parameter types, and which the Nuru mediator activates. Silencing the rule for the whole project means a dead internal class anywhere else in `tools/dev-cli` is never reported either. The suppression itself is justified, because CA1812 is a known false positive for types that DI activates.
- Suggestion: Scope it to the type with `[SuppressMessage("Performance", "CA1812", Justification = "Activated by the Nuru mediator")]` on `WorkflowCommand.Handler`, and drop CA1812 and its comment from `NoWarn`. If more mediator-activated internal handlers are expected soon, mark this wontfix and give that reason instead.
- Status: open

### Issue 3 — Severity: nit
- File: documentation/user/installation.md:139 (and :81)
- Description: The sweep changed statements about which .NET version introduced a feature, not only statements about what the package requires. `dnx` and the `#:package` directive arrived with the .NET 10 SDK. "`dnx` is a .NET 11 command" and "`#:package` is native to .NET 11" are now wrong about history. The requirement lines at :554 and :560 are correctly .NET 11.
- Suggestion: Make both lines version-neutral or open-ended. For example: "`dnx` is a .NET SDK command (.NET 10 and later)" and "The `#:package` directive is built into the .NET SDK (.NET 10 and later)".
- Status: open

## Checked, no finding

- `global.json`: with `rollForward: latestFeature` and `allowPrerelease: true`, a later GA `11.0.1xx` SDK will be selected once it is installed. CI installs the exact version from `global-json-file`.
- `.githooks/Directory.Build.props` imports the root props and opts out of packing only for the hooks. The product csprojs still pack, as the CI run shows.
- `WorkflowCommand.Handler` is internal with a public constructor. That is consistent with the internal DevCli service types and avoids CS0051.
- `Directory.Packages.props`: dropping the BOM is harmless.
- `3.0.0` is correct semver for dropping a TFM. `PublicAPI.Unshipped.txt` stays header-only because the symbol surface did not change.
- The pre-GA soak on Windows and macOS is honestly recorded as not run. The task allows that.
