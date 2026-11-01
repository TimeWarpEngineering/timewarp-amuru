# Round 1 — merged findings
**Date:** 2026-10-31
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/native/file-system/file-system-walk.cs:53. Also at `source/timewarp-amuru-tools/nu-get/nuget-package-service.cs:124`, `source/timewarp-amuru/native/file-system/glob-matcher.cs:35`, and `source/timewarp-amuru/native/text/text-diff.cs:259` and `:279`.
- Description: Five `#pragma warning disable CA1307` blocks silence analyzer rule CA1307. Each one rests on a false claim, "net11.0 has no StringComparison overload". On SDK `11.0.100-rc.3.26522.148` with NetAnalyzers `11.0.100-rc.3.26522.148`, `AnalysisMode=All`, and warnings as errors, the `StartsWith(char, StringComparison)`, `EndsWith(char, StringComparison)`, and `LastIndexOf(char, int, StringComparison)` overloads compile clean. Neither CA1307 nor CA1865 fires. The CA1307 message itself names the overload to use.
- Suggestion: Pass `StringComparison.Ordinal` to the char overload at all five sites. Remove the pragmas, their comments, and the locals that only hold a pragma span. Correct the CA1307 claim in `task.md` (Notes "CA1307 and CA1865", the checklist line in section 1, and `## Results`). Run the Release build and `dev test` again.
- Source: general
- Disposition notes:

### M2 — Severity: nit — Status: open
- File: tools/dev-cli/Directory.Build.props:28
- Description: CA1812 is suppressed project-wide for the dev CLI, but only `WorkflowCommand.Handler`, which the mediator activates, needs it. Dead internal classes elsewhere in the dev CLI would no longer be reported.
- Suggestion: Put `[SuppressMessage("Performance", "CA1812", Justification = "Activated by the Nuru mediator")]` on `WorkflowCommand.Handler` and drop CA1812 from `NoWarn`. Or mark wontfix with a reason.
- Source: general
- Disposition notes:

### M3 — Severity: nit — Status: open
- File: documentation/user/installation.md:139 (and :81)
- Description: "`dnx` is a .NET 11 command" and "`#:package` is native to .NET 11" state the wrong history, because both arrived with the .NET 10 SDK. The requirement lines at :554 and :560 are correct.
- Suggestion: Use version-neutral or open-ended wording, such as ".NET SDK (.NET 10 and later)".
- Source: general
- Disposition notes:

## Duplicates / conflicts

- The five CA1307 sites are one finding (M1) because they share one root cause and one fix.
