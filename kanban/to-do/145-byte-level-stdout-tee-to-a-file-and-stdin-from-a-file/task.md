# Byte-level stdout tee to a file, and stdin from a file

## Description

Give a caller an in-process way to (1) copy a child's stdout to a file byte for byte as
the bytes arrive, while the normal capture still works, and (2) use a file as the child's
stdin. ganda needs both to stop wrapping agent launches in `bash -c '… | tee -- out'`
(ganda task 648).

Why Amuru cannot do this today:

- Every streaming path is line based. `StreamStdoutAsync`, `StreamToFileAsync`
  (`source/timewarp-amuru/core/command-result.cs`), and the echo-and-capture merge all use
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

- [ ] Stdout tee: a builder option or method that writes stdout bytes to a file as they
      arrive (flushed per write, readable by another reader while the child runs) and
      still returns the captured stdout, stderr, and exit code unchanged.
- [ ] The tee file holds an unterminated final line while the child is still running
      (test: a child that prints text with no newline and then sleeps; the file has the
      text before the child exits).
- [ ] Stdin from a file: an overload beside `WithStandardInput(string)` that takes a file
      path. `AsJsonRpcClient` refuses it the same way it refuses the string form.
- [ ] Both work with the mock path (`TryExecuteMockAsync`) and with cancellation: a
      cancelled command leaves the bytes written so far in the tee file.
- [ ] stderr is not written to the tee file.
- [ ] Docs for both, and a release so ganda can take the package.

## Session

- Created: b3be0249-1852-425a-9e90-5a7e56049503 (2026-11-15)

## Notes

Filed only; do not start until Steven says so.

Steven 2026-11-15: an Amuru fix is in process instead of out of process; "no bash at all
is good".

The API shape is the implementer's call. The names above are not a requirement.

ganda task 648 depends on this task's release.
