# Passthrough and TtyPassthrough must honor configured standard input

## Description

`CommandResult.PassthroughAsync` ignores any standard input configured on the command. It always rebinds stdin to the console (`source/timewarp-amuru/core/command-result.cs:202-211`: `OpenStandardInput()` then `WithStandardInputPipe(PipeSource.FromStream(stdIn))`), which overwrites the `PipeSource.FromString(standardInput)` that `Shell.Builder` / the run extensions attached in `command-extensions.cs:97-99`. `TtyPassthroughAsync` (`command-result.cs:252+`) starts the process with `RedirectStandardInput = false`, so configured stdin is silently dropped there too.

Effect: any builder that feeds the child over stdin (fzf `FromInput`, `FromFiles`, `FromCommand` after task 088; any caller using `WithStandardInput`) works with `CaptureAsync` / `SelectAsync` / `RunAsync` but silently loses its input in the two passthrough modes. No error, no warning.

Found by the task 088 review (finding M2, pre-existing, out of scope there).

## Requirements

- `PassthroughAsync`: when the command has configured standard input (a string pipe source), use it instead of the console stream. Only fall back to `OpenStandardInput()` when no stdin was configured. Stdout and stderr still go to the console.
- `TtyPassthroughAsync`: decide and document one of:
  - (a) honor configured stdin by redirecting only stdin (`RedirectStandardInput = true`, write the string, close) while stdout/stderr stay inherited; or
  - (b) refuse with a clear `InvalidOperationException` ("TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY") so the drop is never silent.
  Preference: (b) unless (a) is shown to work for real TUI apps without breaking isatty on stdout/stderr. Record the choice in Results.
- Mock path (`CommandMock`) must see the configured stdin the same way the real path does.
- No change to `CaptureAsync`, `StreamAsync`, `SelectAsync`, `RunAsync` semantics. Empty string stdin still means immediate EOF (task 088); null still means caller owns stdin.
- Update the Purpose/Design header comment in `command-result.cs` to state how each execution mode treats configured stdin.
- Tests: real-execution tests for `PassthroughAsync` with configured stdin (e.g. `cat` / `sort` reading from the string) proving the input reaches the child; a test that `TtyPassthroughAsync` with configured stdin behaves per the recorded decision; mock-path parity test.

## Added scope (2026-10-05, from task 100 review finding M1)

`CommandResult.Pipe(string executable, params string[] args)` has no overload that accepts `CommandOptions`, so a piped stage cannot carry validation, working directory, or environment. Visible effect: `Fzf.FromCommand(...)` applies validation and options to the source command only; the fzf stage runs with defaults. Documented today as a limitation in `source/timewarp-amuru-tools/fzf-command/Fzf.cs`.

- Add a `Pipe` overload on `CommandResult` that accepts `CommandOptions` (or the builder-style equivalent used elsewhere in core) and applies it to the piped stage.
- Update `Fzf.FromCommand` / `SelectWithFzf` to pass the fzf builder's options through to the pipe stage, and remove the limitation note.
- Test: a piped stage with `WithZeroExitCodeValidation` throws when the downstream command fails; the same without validation reports the exit code.
- PublicAPI: new core member goes in `source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt`.

## Checklist

- [ ] Read `command-result.cs` PassthroughAsync / TtyPassthroughAsync and `command-extensions.cs` stdin attachment; confirm how CliWrap exposes the configured `PipeSource` (may need to track "stdin configured" on CommandResult rather than inspect CliWrap internals).
- [ ] PassthroughAsync: keep configured stdin; fall back to console stdin only when none configured.
- [ ] TtyPassthroughAsync: implement option (a) or (b); document in Results and in XML docs.
- [ ] CommandMock parity for both modes.
- [ ] Tests under `tests/timewarp-amuru/single-file-tests/core/` (or the existing command-result test files): passthrough honors stdin; tty decision covered; mock parity.
- [ ] Update header Design comment and XML docs on both methods.
- [ ] `Pipe` overload with options (see Added scope); fzf passes options through; limitation note removed
- [ ] PublicAPI Unshipped updated for every new core member (analyzer is live; RS0016 fails the build otherwise)
- [ ] `dev build` warnings-as-errors clean; `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs` green.

## Notes

- Origin: task 088 review `kanban/done/088-.../review/round-1/merged.md` M2 (2026-10-04). The old echo-based fzf input had the same defect, so this is not a regression from 088.
- fzf's primary path is `SelectAsync`, which is unaffected. `PassthroughAsync` is the documented mode for "interactive stream-based tools like fzf", so the gap is user-visible.
- Behavior change in `PassthroughAsync` is additive for callers who never configured stdin. Note it in the release notes alongside the 088 empty-string change.

## Session

- Created: 522eb63d (2026-10-04)
- Kitchen refresh (Pipe options scope): 522eb63d (2026-10-05)
