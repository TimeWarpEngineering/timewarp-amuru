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
`nuget` (`DotNet.NuGet()`) and `--version` (`DotNet.WithVersion()`) already have typed builders.
`msbuild` and `nuget verify` do not; task 151 adds them and runs before this task. `fixie` is a
third-party tool, out of scope.

## Requirements

- New project `source/timewarp-amuru-analyzers` (netstandard2.0, `Microsoft.CodeAnalysis.CSharp`),
  packed into `TimeWarp.Amuru` under `analyzers/dotnet/cs`, with `AnalyzerReleases.Shipped.md` /
  `Unshipped.md`. Copy the layout from timewarp-jaribu `source/timewarp-jaribu-analyzers`.
- The analyzer ships in `TimeWarp.Amuru` because `Shell` lives there. The `DotNet.*` and `Git.*`
  builders live in `TimeWarp.Amuru.Tools`, so every message names that package, for example
  `Use DotNet.Run() (TimeWarp.Amuru.Tools) instead of Shell.Builder("dotnet") with "run"`. A
  consumer that references only `TimeWarp.Amuru` adds the Tools package to fix the diagnostic.
  The analyzer does not check whether Tools is referenced.
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
    builders land. With task 151 merged, `msbuild` and `nuget` are not on it. `--version` and
    `--info` map to `DotNet.WithVersion()` / `DotNet.WithInfo()` under `AMURU0001`.
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

## Depends on

- 151

## Goal (implementer restatement)

Ship a Roslyn analyzer inside the `TimeWarp.Amuru` package that flags `Shell.Builder(...)` and
`Shell.Run(...)` when the executable is a compile-time constant `dotnet` or `git` and a typed
Amuru builder exists, so consumers move to `DotNet.*` / `Git.*` (TimeWarp.Amuru.Tools).

Acceptance:
- `source/timewarp-amuru-analyzers` (netstandard2.0) with release-tracking files, packed into the
  `TimeWarp.Amuru` nupkg under `analyzers/dotnet/cs`.
- AMURU0001 (error): `dotnet` + a verb with a typed builder, or an unreadable verb (generic message).
- AMURU0002 (warning): `dotnet` + a verb with no typed builder (known-gap list in the analyzer).
- AMURU0003 (info): `git` + a call that matches an existing `Git.*` helper.
- Messages name the replacement and the TimeWarp.Amuru.Tools package. Non-constant executables
  are not reported.
- One verb table, asserted by a test against the public `DotNet` surface (both directions), and
  the git helper names asserted against `Git`.
- Jaribu runfile tests: literal, const, verb with/without builder, non-constant, `Shell.Run`, git
  info, editorconfig severity override.
- Amuru's tests and dev CLI build under the analyzer; deliberate raw shells are pragma-suppressed
  with a comment.
- Readme section (ids, defaults, suppression) and a version bump.

Round 2 fix pass (M2, M3, M4): AMURU0003 must name a Git helper only for the argv that helper
builds. `GetDefaultBranchAsync` matches `symbolic-ref refs/remotes/origin/HEAD --short` only.
`SetUpstreamAsync` needs the third argument to be `<remote>/<fourth>` with a non-empty remote
(constant pair, or an interpolation ending `/{x}` where the fourth argument is that same `x`).
`GetCommitsAheadAsync` needs a two-dot `..HEAD` range, never `...HEAD`. Each finding's
counterexamples get a no-report test case; the existing true positives keep reporting.

## Checklist

- [x] Analyzer project packed under analyzers/dotnet/cs
- [x] AMURU0001 dotnet verb with builder (error)
- [x] AMURU0002 dotnet verb without builder (warning)
- [x] AMURU0003 git with existing helper (info)
- [x] Verb table asserted against the public DotNet surface
- [x] Tests
- [x] Amuru itself compiles clean, exceptions explained
- [x] Readme and version bump

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session
- Groomed: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18), existing nuget/version builders, Tools package in messages, depends on 151
- Implemented: ganda task work implement oracle (2026-11-18), Claude Opus 5.5
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 939359cc-7424-4d0f-980c-364b061ae12d, max-turns 500, Done — 2026-11-18T02:52:25Z
- Review oracle: Grok, effort 3, roster general, round 1, 2026-11-18. Open finding M1. Disposition not written.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ec92-02b0-76a0-af9f-6e71341180db, max-turns 200, artifacts general.md, merged.md — 2026-11-18T03:06:15Z
- Fix pass for round 1 (implement-fix-2 oracle, Claude Opus 5.5, 2026-11-18): M1 fixed. AMURU0003 now matches whole git argument shapes.
- Review round 2: reviewer-grok (grok, vendor xai, model grok-4.7), session 01a1eca3-78dc-71d0-9c94-9992104206d8, effort 2, roster general, max-turns 120. M1 stays fixed. Open findings M2, M3, M4. Disposition not written. Artifacts `review/round-2/general.md`, `review/round-2/merged.md`.
- Fix pass for round 2 (implement-fix-3 oracle, Claude Opus 5.5, 2026-11-18): M2, M3, M4 fixed in `git-helper-table.cs` and `shell-builder-analyzer.cs`.

## Results

What shipped:

- `source/timewarp-amuru-analyzers` (netstandard2.0, `Microsoft.CodeAnalysis.CSharp` 5.12.0, same
  pin as timewarp-jaribu-analyzers) with `AnalyzerReleases.Shipped.md` / `Unshipped.md`.
  `timewarp-amuru.csproj` builds it through an `Analyzer` ProjectReference and the
  `PackAmuruAnalyzer` target packs `analyzers/dotnet/cs/timewarp-amuru-analyzers.dll` into
  `TimeWarp.Amuru.3.0.0-beta.8.nupkg` (checked with `unzip -l`). Added to `timewarp-amuru.slnx`.
- `ShellBuilderAnalyzer`: an operation analyzer on `TimeWarp.Amuru.Shell.Builder` / `Shell.Run`
  with a constant executable. It reads the leading constant arguments of the first chained
  `WithArguments` call, or of the `Shell.Run` arguments array (`new[]`, `new string[]`, a
  collection expression).
  - AMURU0001 (error): table row → `Use DotNet.Run() (TimeWarp.Amuru.Tools) instead of
    Shell.Builder("dotnet") with "run"`. When there are no readable arguments, or the verb is
    known but its subcommand is not → `Use the DotNet.* builders (TimeWarp.Amuru.Tools) instead
    of Shell.Builder("dotnet")`. A `*.cs` first argument → `DotNet.Run().WithFile()`.
  - AMURU0002 (warning): known-gap SDK verb (`format`, `exec`, `vstest`, ... in
    `TypedBuilderTable.DotNetGapVerbs`) → "file a task in timewarp-amuru for a DotNet builder".
    An unknown verb (a local tool such as `fixie`) also gets AMURU0002, and the message points
    at `DotNet.Tool().Run("fixie")`. `msbuild` and `nuget` are table rows (task 151).
  - AMURU0003 (info): a git call with the exact argument shape of a `Git.*` helper
    (`GitHelperTable`; reworked in the round 1 fix pass below).
- `TypedBuilderTable` is the one table. The `VerbTable_Given_` tests reflect over `DotNet` and
  `Git`. They fail when a public static DotNet builder has no row (only `DotNet.Builder()` is
  exempt), when a row names a missing method, or when a gap verb overlaps a row.
- The analyzer runs on Amuru's own code: the core project, `tests/timewarp-amuru` (Directory.Build.props) and
  `tools/dev-cli` (Directory.Build.props). Raw `dotnet` calls moved to typed builders instead of
  pragmas. Those calls were: dev-cli release `nuget push` → `DotNet.NuGet().Push()`, version
  probe `msbuild -getProperty` → `DotNet.MsBuild().WithGetProperty()`, `dev test` →
  `DotNet.Run().WithFile()` (task 149's origin), `dev verify-samples` → `DotNet.Build()`, and
  the two test child builds (argv-echo, mcp-stdio-server) → `DotNet.Build()`. `RunProcessAsync`
  became `WriteOutputAsync`. No `#pragma warning disable AMURU*` was needed in this repo.
- One exception outside the repo: `TimeWarp.Nuru.DevCli` 4.0.0-beta.15 compiles
  `content/any/services/packable-project-service.cs` from the NuGet cache, and that file calls
  `Shell.Builder("dotnet")` with `msbuild`. No pragma or repo `.editorconfig` can reach it.
  `tools/dev-cli/Directory.Build.targets` (`ScopeAmuruAnalyzerForDevCliContent`) generates a
  global analyzer config that lowers AMURU0001 to suggestion for that one package file only.
  The comment there says to remove the target once DevCli moves to `DotNet.MsBuild()`.
- Readme `## Analyzers` section (ids, defaults, editorconfig and pragma suppression; the
  diagnostics' help link points at it). AGENTS.md package-layout line. Version
  3.0.0-beta.7 → 3.0.0-beta.8.

Audit (boy-scout): the first `ganda repo audit` failed `analyzer-suppressions`. The pragma test case's
sample source contained a literal `#pragma warning disable AMURU0001` string. That text is now
built from a `PragmaWarning` constant, with a comment. `vscode-window-icon` (warning) was fixed by
`ganda repo audit --fix`, which created the git-ignored `.vscode/settings.json`.
The first commit's post-commit attest then failed two checks that run on the committed tree:
`runfile-executable` (the new analyzer test had no exec bit) and `package-pin-currency`
(TimeWarp.Jaribu 1.0.0-beta.17 was published 2026-11-18). `ganda repo audit --fix` set the bit and
bumped Jaribu 1.0.0-beta.16 → 1.0.0-beta.17. The aggregate runner compile and the three test files
below were rerun on beta.17.

Fix pass, review round 1 M1 (AMURU0003 named helpers for operations they do not run):

- git rows moved out of `TypedBuilderTable` into `GitHelperTable` (`source/timewarp-amuru-analyzers/git-helper-table.cs`).
  Each row lists the full argument shapes its helper runs. A call matches only when it has exactly
  that many arguments and each fits its slot: `Literal` (that constant), `Value` (a constant or
  non-constant that does not start with `-`), `Shaped(prefix, suffix)` (known literal start and
  end, for `refs/heads/{branch}` and `{branch}..HEAD`). One helper per row; `worktree add` and
  `worktree add -b` are separate rows.
- Dropped rows: `rev-parse --show-toplevel` (`Git.FindRoot()` is a directory walk, no git) and
  `remote get-url origin` (`Git.GetRepositoryNameAsync()` returns a name, not the URL).
- The analyzer now reads every argument, not only the leading constants: every chained
  `WithArguments` call is appended (it appends at run time), interpolated strings and `+`
  concatenations keep their literal start and end, and a spread, an array variable, or a chain that
  still yields a `ShellBuilder` (kept for later) marks the list incomplete. git reports only on a
  complete list. dotnet still reads the leading constants, so its behavior is unchanged except that
  a second chained `WithArguments` now extends the read instead of being ignored.
- The diagnostic label is the helper's command line, for example
  `with "worktree add <path> <branch>"`. Readme AMURU0003 row says exact match.
- Tests: `GitHelperShapes_Should_EachNameTheirHelper` (a true positive per row, plus `Shell.Run`,
  and asserts every row is covered), `GitOtherOperations_Should_NotReport` (every M1
  counterexample), `GitIncompleteArguments_Should_NotReport` (spread, array variable, kept builder,
  extra chained `WithArguments`). `GitWithHelper_Should_ReportInfo` message updated.

Follow-up (not filed from this walk): a timewarp-nuru task to move TimeWarp.Nuru.DevCli
`PackableProjectService.EvaluateWithMsBuildAsync` to `DotNet.MsBuild()` once an Amuru release
carries task 151. Every repo that uses DevCli hits AMURU0001 on its next Amuru bump until then.

### Gates

- `./bin/dev build` → ✅ Build completed successfully (after the version bump; nupkg contains the analyzer)
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs` → 20/20 passed (new)
- `dotnet run --file tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs` → 4/4 passed (edited; rerun with the temp argv-echo build deleted so `DotNet.Build()` ran)
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs` → 20/20 passed (edited; rerun with the temp mcp-stdio-server build deleted)
- `dotnet build tools/dev-cli/dev.cs` → exit 0 under the analyzer (dev-cli has no test file)
- `dotnet build tests/timewarp-amuru/multi-file-runners/run-tests.cs` → exit 0 (compile only; CI runs the suite)
- `ganda repo audit` → failed analyzer-suppressions (fixed above) → `ganda repo audit --fix` → exit 0
- post-commit attest → failed runfile-executable + package-pin-currency → `ganda repo audit --fix` → exit 0; test files above rerun on Jaribu beta.17 with the same counts, aggregate compile exit 0

Fix pass (round 1 M1):

- `./bin/dev build` → ✅ Build completed successfully, 0 warnings
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs` → 23/23 passed (edited; 3 new git cases)
- `ganda repo audit` → exit 0, all checks pass

Fix pass (round 2 M2, M3, M4):

- M2: `GetDefaultBranchAsync` row keeps one shape, `symbolic-ref refs/remotes/origin/HEAD --short` (label updated).
- M3: `GitHelperRow` takes an optional argument check. `SetUpstreamAsync` requires the third argument to be `<remote>/<fourth>` with a non-empty remote: two constants, or an interpolation ending in a literal `/` and a final hole naming the same local, parameter, field or property as the fourth argument (`ShellArgument.Reference` / `ShellArgument.SlashTail`, read by the analyzer).
- M4: `ArgumentSlot.Shaped` takes an excluded suffix; `GetCommitsAheadAsync` uses `..HEAD` excluding `...HEAD`.
- Tests: new `GitNearHelperShapes_Should_NotReport` (both M2 shapes, M3 constant and interpolated counterexamples, M4 constant and interpolated three-dot). `GitHelperShapes_Should_EachNameTheirHelper` gained the constant `origin/main main`, `$"origin/{branch}" branch`, and `main..HEAD` true positives (folded there to stay under the 500-line TWA0038 cap).
- `./bin/dev build` → ✅ Build completed successfully, 0 warnings, 0 errors
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs` → 24/24 passed (edited; first run hit TWA0038 at 503 lines, folded the positive cases into the existing shapes test, rerun green)
- `ganda repo audit` → exit 0, all checks pass

### How to validate

Smoke:

```bash
./bin/dev build
unzip -l artifacts/packages/TimeWarp.Amuru.3.0.0-beta.8.nupkg | grep analyzers
dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs
dotnet build tools/dev-cli/dev.cs
```

Expect: build succeeds. The nupkg lists `analyzers/dotnet/cs/timewarp-amuru-analyzers.dll`. The
analyzer test reports 24 passed, 0 failed. The dev-cli build exits 0 with no AMURU0001 error.
Adding `Shell.Builder("dotnet").WithArguments("run")` to any dev-cli or test file fails the
build with `error AMURU0001: Use DotNet.Run() (TimeWarp.Amuru.Tools) instead of
Shell.Builder("dotnet") with "run"`.

### Review

Round 1 (effort 3, roster general): one bug, M1: AMURU0003 named Git helpers for command lines those helpers do not run. See `review/round-1/merged.md`. Fixed in the fix pass above; gates rerun (build, analyzer runfile 23/23, audit).

Round 2 (effort 2, roster general): re-checked the fix delta. M1's listed counterexamples stay silent. Three open bugs remain on the new matcher: M2 `GetDefaultBranchAsync` matches two shapes the helper does not run, M3 `SetUpstreamAsync` does not require `{remote}/{branchName}` plus that same branch, M4 `GetCommitsAheadAsync` matches a three-dot `main...HEAD` range. See `review/round-2/merged.md`. Disposition not written. Gates were not re-run; the fix-pass record above covers this delta.

## Review

- Round 1: M1 fixed in the fix pass (2026-11-18). Round 2 confirmed the listed counterexamples stay silent.
- Round 2: M2, M3, M4 fixed in the round 2 fix pass (2026-11-18); round 3 to confirm.

## Notes

Origin: task 149. The ganda `dev test` command launches the Jaribu suite with
`Shell.Builder("dotnet")`, which is why it had no `ganda.dotnet` span and no trace propagation.
Steve: "This should never be, we should always be using the .NET command. That's the whole
point in having it in Amuru."

Related: 151 (`DotNet.MsBuild()` and `DotNet.NuGet().Verify()`) closes the AMURU0002 gaps found
above and lands first.

## Learnings

Misread: the opening sentence of `## Goal (implementer restatement)` says the analyzer flags a constant `dotnet` or `git` executable only when a typed builder exists. The task also requires AMURU0002 when the dotnet verb has no typed builder, and AMURU0001 when the verb cannot be read. The acceptance list under that same restatement includes both, and the implementation includes both. The lead sentence is the mismatch.
