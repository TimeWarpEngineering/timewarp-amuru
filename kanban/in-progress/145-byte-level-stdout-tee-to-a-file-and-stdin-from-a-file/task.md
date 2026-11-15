# Byte-level stdout tee to a file, and stdin from a file

## Description

Give a caller an in-process way to (1) copy a child's stdout to a file byte for byte as
the bytes arrive, while the normal capture still works, and (2) use a file as the child's
stdin. ganda needs both to stop wrapping agent launches in `bash -c '… | tee -- out'`
(ganda task 648).

Why Amuru cannot do this today:

- Every streaming path is line based. `StreamStdoutAsync` and `StreamToFileAsync` on
  `ShellBuilder` (`source/timewarp-amuru/core/shell-builder.cs`), their `CommandResult`
  counterparts (`source/timewarp-amuru/core/command-result.cs`), and the echo-and-capture merge all use
  `PipeTarget.ToDelegate`, which delivers a line only after its newline. Grok ends its
  answer with no trailing newline, so the last line (the one ganda needs) never arrives
  while the process is alive. That is the reason ganda shells out to `tee` and polls the
  file.
- `ShellBuilder.WithStandardInput(string)` (`source/timewarp-amuru/core/shell-builder.cs`)
  takes text only. ganda's prompts live in a temp file and can be over 128 KiB.

CliWrap already has the pieces: `PipeTarget.ToStream` / `PipeTarget.ToFile` pass raw bytes,
`PipeTarget.Merge` fans one stream out to several targets, and `PipeSource.FromFile` reads
stdin from a file.

## Checklist

- [x] Stdout tee: a builder option or method that writes stdout bytes to a file as they
      arrive (flushed per write, readable by another reader while the child runs) and
      still returns the captured stdout, stderr, and exit code unchanged.
- [x] The tee file holds an unterminated final line while the child is still running
      (test: a child that prints text with no newline and then sleeps; the file has the
      text before the child exits).
- [x] Stdin from a file: an overload beside `WithStandardInput(string)` that takes a file
      path. `AsJsonRpcClient` refuses it the same way it refuses the string form.
- [x] Both work with the mock path (`TryExecuteMockAsync`) and with cancellation: a
      cancelled command leaves the bytes written so far in the tee file.
- [x] stderr is not written to the tee file.
- [x] Docs for both, and a release so ganda can take the package.
      (Docs and the `3.0.0-beta.5` version bump are done here. Cutting the release with
      `dev release` waits for an explicit ask after merge.)

## Goal (implementer restatement)

ganda must stop running agents through `bash -c '… | tee -- out'`. Amuru needs two
in-process features on `ShellBuilder`:

1. **Stdout tee.** A builder option that copies the child's stdout bytes into a file as
   they arrive. No line splitting: an unterminated last line is in the file while the
   child still runs. Every write is flushed, and another reader can open the file during
   the run. The normal result (stdout, stderr, exit code) is unchanged. stderr never goes
   to the tee file.
2. **Stdin from a file.** A builder option beside `WithStandardInput(string)` that takes
   a file path. `AsJsonRpcClient` refuses it like the string form.

Acceptance:

- Tee works in the CliWrap execution modes and survives cancellation (bytes so far stay
  in the file).
- A test child prints text with no newline and sleeps; the tee file holds that text
  before the child exits.
- Both options work on the mock path (`TryExecuteMockAsync`).
- Docs for both, and the version is bumped so a release can ship (the release itself is a
  later explicit step).

## Session

- Created: b3be0249-1852-425a-9e90-5a7e56049503 (2026-11-15)
- Implemented: ganda task work, implement oracle (2026-11-15)

## Notes

Filed only; do not start until Steven says so.

Steven 2026-11-15: an Amuru fix is in process instead of out of process; "no bash at all
is good".

The API shape is the implementer's call. The names above are not a requirement.

ganda task 648 depends on this task's release.

## Results

API (the task left the shape open):

- `ShellBuilder.WithStandardOutputTee(string filePath)`: byte-level copy of stdout to a
  file. Every CliWrap mode (`RunAsync`, `CaptureAsync`, `RunAndCaptureAsync`,
  `PassthroughAsync`, `SelectAsync`, the three `Stream*Async`, `StreamToFileAsync`) puts a
  tee `PipeTarget` in front of its own stdout target. The tee is first in the merge, so it
  gets each chunk before a line target that waits for a newline. Unbuffered `FileStream`,
  flushed per chunk, `FileShare.ReadWrite | Delete`. Chunks are written with
  `CancellationToken.None`, so cancel or timeout keeps what was read. stderr is never teed.
  `Pipe` keeps the tee on the last stage. `TtyPassthroughAsync` and `AsJsonRpcClient` refuse it.
- `ShellBuilder.WithStandardInputFile(string filePath)`: `PipeSource.FromFile`. The later
  of the text and file calls wins. It counts as configured stdin (Passthrough keeps it,
  TTY and `AsJsonRpcClient` refuse it). A missing file throws `FileNotFoundException`.
- Both paths are made absolute against the current directory at configure time.
- Mock path (`TryExecuteMockAsync`): the tee file is created on match and gets the setup's
  stdout after the delay or exception prelude. A matched call records the stdin file's text
  for `CommandMock.GetConfiguredStandardInput`.

Files:

- New `source/timewarp-amuru/core/command-result.standard-streams.cs`: the constructors move
  here, plus the two fields, the tee target, and the mock hooks. That keeps
  `command-result.cs` under its line cap. It shrank from 1339 to 1291 lines, and the
  `.editorconfig` TWA0038 entry and `.timewarp/line-cap-baseline.json` ratchet go down to match.
- `shell-builder.cs`, `command-extensions.cs`, `command-result.cs`, `command-result.timeout.cs`.
- Tests: `tests/timewarp-amuru/single-file-tests/core/shell-builder.standard-output-tee.cs`
  (13) and `shell-builder.standard-input-file.cs` (9).
- Docs: `documentation/developer/reference/command-execution.md` (new section),
  `readme.md`, `skills/tw-amuru/SKILL.md`, `AGENTS.md`, and
  `documentation/release-notes/3.0.0-beta.5.md`.
- Version `3.0.0-beta.4` -> `3.0.0-beta.5`. The two API lines are in `PublicAPI.Shipped.txt`,
  and Unshipped is header-only again (the AGENTS.md rule for a version-bump PR).

Gates (run once each, in order, after the last product edit):

- `./bin/dev build`: succeeded, 0 warnings.
- `./bin/dev test`: exit 0. Aggregate: 875 total, 874 passed, 1 skipped.
- `ganda repo audit`: passed with one advisory, `vscode-window-icon` (the ignored
  `.vscode/settings.json` was missing). `ganda repo audit --fix` fixed it, and the audit
  then reported "Repository passes all audit checks". That file is gitignored and changes
  no tracked file.
- The first commit's post-commit attest refused on two checks the pre-commit audit did not
  show. `runfile-executable` failed because the two new test runfiles lacked the exec bit.
  `package-pin-currency` failed because the githook self-pins `TimeWarp.Amuru[.Tools]` were
  `3.0.0-beta.3` and `3.0.0-beta.4` had been published that day. `ganda repo audit --fix`
  set the exec bits and moved the pins to `3.0.0-beta.4`, recorded in the beta.5 release
  note. Only the `.githooks` runfiles consume those pins, so build and test do not change.
  The rerun `ganda repo audit` passed all checks.

Follow-up: cut the `3.0.0-beta.5` release (`dev release`) after merge, on an explicit ask.
ganda task 648 then takes the package.

### How to validate

Smoke:

```bash
cd tests/timewarp-amuru/single-file-tests/core
dotnet run shell-builder.standard-output-tee.cs
dotnet run shell-builder.standard-input-file.cs
```

Expect: 13/13 and 9/9 pass. `UnterminatedLine_Should_BeInFileWhileChildRuns` reads
`final answer` from the tee file while `sh -c "printf 'final answer'; sleep 3"` is still
running. `Cancellation_Should_KeepBytesWritten` and `Timeout_Should_KeepBytesWritten` keep
the bytes written before the stop. `CaptureAsync_Should_CopyStdoutAndKeepResult` shows
stderr out of the file and the capture unchanged. `BinaryOutput_Should_CopyBytesUnchanged`
shows raw bytes (`00 FF 0D 0A 80`) unchanged. `LargeFile_Should_StreamEveryByte` streams
300 KiB through stdin.
