# Analyzer: Shell.Builder on a tool with a typed Amuru command

## Description

`Shell.Builder("dotnet")` should never appear in a TimeWarp repo. Amuru ships typed builders
(`DotNet.Run()`, `DotNet.Build()`, `DotNet.Test()`, ...) and the point of having them is that
every consumer goes through one place: telemetry tags, `TRACEPARENT` propagation (task 149),
argument validation, and one fix when the dotnet CLI changes. A raw `Shell.Builder` bypasses
all of it and nothing today tells the author.

Ship a Roslyn analyzer inside the `TimeWarp.Amuru` package (`analyzers/dotnet/cs`, the same
packaging timewarp-jaribu uses for `timewarp-jaribu-analyzers`) so the diagnostic shows up in
every consumer at compile time, with a message that names the typed builder to use instead.

Org-wide count on 2026-11-18 across the master worktrees under
`~/worktrees/github.com/TimeWarpEngineering/*` (`Shell.Builder("<tool>")`, `.cs` files):

| Tool | Call sites | Typed builders exist today |
|------|-----------:|----------------------------|
| git | 772 | partial (worktree, fetch, clone-bare, remote-head, ...) |
| dotnet | 162 | yes, for most verbs |
| ganda | 98 | no |
| gh | 51 | no |
| herdr | 15 | no |

dotnet verbs used through `Shell.Builder`: run 21, build 13, tool 10, fixie 10, test 6, new 6,
publish 5, msbuild 5, nuget 3, user-secrets 1, restore 1, pack 1, clean 1, add 1, `--version` 1.
`msbuild`, `nuget`, and `--version` have no typed builder; task 151 adds them. `fixie` is a
third-party tool, out of scope.

## Requirements

- New project `source/timewarp-amuru-analyzers` (netstandard2.0, `Microsoft.CodeAnalysis.CSharp`),
  packed into `TimeWarp.Amuru` under `analyzers/dotnet/cs`, with `AnalyzerReleases.Shipped.md` /
  `Unshipped.md`. Copy the layout from timewarp-jaribu `source/timewarp-jaribu-analyzers`.
- Trigger: an invocation of `Shell.Builder(...)` or `Shell.Run(...)` (any overload that takes the
  executable name) whose first argument is a string literal or a constant. A non-constant
  executable is not reported.
- Three diagnostics, each with a stable id and a message that names the replacement:
  - `AMURU0001` (default **error**): the executable is `dotnet` and the first `WithArguments`
    literal, when present, is a verb that has a typed builder. Message:
    `Use DotNet.Run() instead of Shell.Builder("dotnet") with "run"`. When the verb cannot be
    read, the message lists the entry point: `Use the DotNet.* builders instead of Shell.Builder("dotnet")`.
  - `AMURU0002` (default **warning**): the executable is `dotnet` and the verb has no typed
    builder. Message names the verb and says to file a task in timewarp-amuru for a typed
    builder. The known-gap verb list ships in the analyzer so it is reviewable and shrinks as
    builders land.
  - `AMURU0003` (default **info**): the executable is `git` and the invocation matches a
    typed `Git.*` helper that exists. Git stays info because 772 call sites and a thin builder
    set would make a warning pure noise; ganda alone has 203. Raise later per repo via
    `.editorconfig`.
- The verb-to-builder table lives in one place and is generated or asserted from the public
  `DotNet` surface (a test that fails when a `DotNet.X()` builder exists with no table row, or a
  row has no builder), so the analyzer cannot drift from the library.
- Suppression: `.editorconfig` severity per id, and `#pragma warning disable AMURU0001` with a
  comment for the few places a raw shell is the point (Amuru's own round-trip tests, a
  deliberately nested `dotnet tool run X -- dotnet run` until the tool builder covers it).
- Tests: Jaribu runfiles with the standard analyzer test harness: literal `dotnet`, `const`
  executable, verb with builder, verb without builder, non-constant executable not reported,
  `Shell.Run` overloads, `git` info case, editorconfig severity override.
- Amuru's own dev CLI and tests compile clean under the analyzer, with the deliberate
  exceptions pragma-suppressed and explained.
- Readme section listing the ids, defaults, and how to suppress.
- Version bump.

## Out of scope

- Fixing the 162 dotnet call sites in other repos. Each repo picks up the diagnostic on its next
  Amuru bump and fixes its own under its own task. The error default is intentional so the bump
  cannot be taken silently.
- Typed builders for `gh`, `ganda`, and `herdr`. Separate decision; nothing to point at yet.

## Checklist

- [ ] Analyzer project packed under analyzers/dotnet/cs
- [ ] AMURU0001 dotnet verb with builder (error)
- [ ] AMURU0002 dotnet verb without builder (warning)
- [ ] AMURU0003 git with existing helper (info)
- [ ] Verb table asserted against the public DotNet surface
- [ ] Tests
- [ ] Amuru itself compiles clean, exceptions explained
- [ ] Readme and version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session

## Notes

Origin: task 149. The ganda `dev test` command launches the Jaribu suite with
`Shell.Builder("dotnet")`, which is why it had no `ganda.dotnet` span and no trace propagation.
Steve: "This should never be, we should always be using the .NET command. That's the whole
point in having it in Amuru."

Related: 151 (typed builders for msbuild, nuget, version) closes the AMURU0002 gaps found above.
