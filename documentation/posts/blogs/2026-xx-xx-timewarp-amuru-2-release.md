# TimeWarp.Amuru 2.0: Shell scripting in C#, without shelling out

2.0.0 is the first stable 2.x of TimeWarp.Amuru. The previous stable is 1.1.1. `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` ship together at `2.0.0`. Install either package without `--prerelease`.

The new text commands stay in the process. `SelectString` searches. `ReplaceInFiles` edits. Neither one starts `grep` or `sed`. A command that might hang takes a timeout and comes back with exit 124.

2.0.0 breaks callers. Read the upgrade guide before you bump a 1.1.1 project:

https://github.com/TimeWarpEngineering/timewarp-amuru/releases/tag/v2.0.0#upgrade-guide

## Install

```bash
dotnet add package TimeWarp.Amuru --version 2.0.0
dotnet add package TimeWarp.Amuru.Tools --version 2.0.0
```

A file-based app pins the same version:

```csharp
#:package TimeWarp.Amuru@2.0.0
```

NuGet: https://www.nuget.org/packages/TimeWarp.Amuru/2.0.0 and https://www.nuget.org/packages/TimeWarp.Amuru.Tools/2.0.0

The samples below were run on Linux against the `TimeWarp.Amuru` 2.0.0 package from nuget.org.

## Search

`Commands.SelectString` writes `path:line:text` and uses grep's exit codes: 0 when a line matched, 1 when none matched, 2 on error.

```csharp
using TimeWarp.Amuru.Native.Text;

CommandOutput found = Commands.SelectString("alpha", "notes.txt");
// found.ExitCode is 0, 1, or 2
// found.Stdout is one "path:line:text" line per matching line
```

On a file whose text is `alpha`, `beta`, `alpha`, a search for `alpha` exits 0 and writes the two matching lines. A search for `missing` exits 1, `Success` is false, and stdout is empty.

`Direct.SelectString` streams `TextMatch` (`Path`, `LineNumber`, `Line`, `Match`, `ContextBefore`, `ContextAfter`). Context lines live on that value. They are not written to the `Commands` stdout.

A file whose first 8000 bytes contain a NUL and have no UTF-16 or UTF-32 BOM is treated as binary and yields no matches.

`Bash.Grep` is the same call:

```csharp
using static TimeWarp.Amuru.Native.Aliases.Bash;

CommandOutput found = Grep("alpha", "notes.txt");
```

## Replace

`Direct.ReplaceInFiles` with `DryRun` fills `Preview` with unified-diff lines and writes nothing. `Backup` is ignored on a dry run, so no `.bak` file appears.

```csharp
await foreach (ReplaceResult file in Direct.ReplaceInFiles(
  "alpha",
  "beta",
  "notes.txt",
  new ReplaceInFilesOptions { DryRun = true }))
{
  if (file.Changed)
  {
    Console.WriteLine(string.Join("\n", file.Preview));
  }
}
```

For a one-line file `alpha` plus a newline, `Changed` is true, `BackupPath` is null, the file bytes stay `alpha\n`, and `Preview` is the full path on the `---` and `+++` lines:

```text
--- /full/path/notes.txt
+++ /full/path/notes.txt
@@ -1,1 +1,1 @@
-alpha
+beta
```

The real edit is `Commands.ReplaceInFiles`. It writes `<path>: N replacement(s)` for each file whose text changed. Exit 0 means the walk finished, including when nothing matched. Exit 1 is a regex or I/O error.

```csharp
CommandOutput edited = Commands.ReplaceInFiles(
  "alpha",
  "beta",
  "notes.txt",
  new ReplaceInFilesOptions { Backup = true });
```

On a file whose lines are `alpha`, `beta`, `alpha`, that call writes one stdout line, the path plus `: 2 replacement(s)`, and the file becomes three `beta` lines. The `.bak` beside the path is the original text. It is written only after the temp file is ready and only when the text changes. A later call whose pattern matches nothing still exits 0, writes no stdout line, and leaves the bytes and the mtime alone.

A file whose text does not change is not rewritten. Its bytes and mtime stay. The write is a temp file in the target directory, then a move on that volume. A BOM is kept. The first newline style is kept. A symlink is followed and the target is rewritten, so the link stays a link.

`$1` and `${name}` expand to captures. `Bash.Sed` is the `Commands` call.

## A command that cannot hang forever

There is no process-wide default timeout. `CommandOptions.Timeout` null means no limit. `TimeoutGracePeriod` defaults to 5 seconds.

```csharp
using TimeWarp.Amuru;

CommandOutput output = await Shell.Builder("sleep")
  .WithArguments("30")
  .WithTimeout(TimeSpan.FromSeconds(2))
  .CaptureAsync();
```

On expiry the graceful signal is sent (SIGINT on Linux and macOS, Ctrl+C on Windows). The process tree is killed after the grace period. Under default validation you get `TimedOut` true, exit code 124 (`CommandResult.TimeoutExitCode`), and `Success` false. The child's own exit status is not preserved. `WithZeroExitCodeValidation()` throws `TimeoutException` instead. `TtyPassthroughAsync` still returns exit 124, including under strict validation.

Checked on nuget.org `2.0.0`: `sleep 30` with a 1 second limit and `WithTimeoutGracePeriod(TimeSpan.Zero)` returned `TimedOut` true, exit 124, and `Success` false. A call that omits the grace override uses the 5 second default.

`MockSetup.TimesOut()` is the test double.

## Also in 2.0.0

- Execution methods may run concurrently on one `CommandResult`. Each call returns its own `CommandOutput`. `LastOutput` is the last writer. Read the value the call returned.
- `Pipe` has an overload that takes `CommandOptions`, so the next stage can set working directory, environment, and validation.
- `CommandMock.GetConfiguredStandardInput` returns the standard-input string recorded for the latest matching call. Null means the caller owns stdin.

## What broke

The full list is one line per change in the upgrade guide linked above. The ones that change an existing call:

- `WithStandardInput("")` is immediate EOF. `null` still means the caller owns stdin.
- `TtyPassthroughAsync` throws if standard input was configured, including the empty string. It also throws when the command is a pipeline. Use `PassthroughAsync` when an upstream stage must feed the child.
- `Git.GetMasterWorktreePathAsync` and `Git.UpdateMasterWorktreeAsync` are gone. Use `Git.GetDefaultWorktreePathAsync` and `Git.UpdateDefaultWorktreeAsync`. `Git.GetWorktreePathAsync`, `Git.UpdateBranchAsync`, and `Git.UpdateWorktreeAsync` no longer default the branch to `"master"`.
- `Git.FetchAsync`, `Git.BranchExistsAsync`, `Git.ConfigureFetchRefspecAsync`, `Git.SetRemoteHeadAutoAsync`, and `Git.WorktreeListPorcelainAsync` return typed result records. They no longer return `bool` or `string`.
- On `TimeWarp.Amuru.Tools`, several `dotnet` builder methods were removed or renamed. `DotNetPackBuilder.WithFramework` is gone. `DotNetListPackagesBuilder.WithConfig` is `WithConfigFile`. `DotNetWatchBuilder.WithTargetFramework` is `WithFramework`. The upgrade guide has the rest.

`TimeWarp.Amuru.Tools` is the fluent builders for `dotnet`, `git`, and `fzf`. It is the same version, `2.0.0`, and it installs without `--prerelease`.

Repository: https://github.com/TimeWarpEngineering/timewarp-amuru
