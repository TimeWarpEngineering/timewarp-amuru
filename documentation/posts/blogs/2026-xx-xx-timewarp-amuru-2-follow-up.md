# Replace the grep and sed shell-out

This is the concrete 2.0.0 example. Post it 2–5 days after the launch post. The launch post is the hub:

https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-amuru-2-release

The upgrade guide, if a call used to compile and no longer does:

https://github.com/TimeWarpEngineering/timewarp-amuru/releases/tag/v2.0.0#upgrade-guide

## Before

A script that searches, then edits, starts two programs. `grep` and `sed` have to be on `PATH`. Quoting belongs to the caller. On a machine without those tools, the script fails before it looks at a file.

```csharp
using TimeWarp.Amuru;

CommandOutput found = await Shell.Builder("grep")
  .WithArguments("-n", "alpha", "notes.txt")
  .CaptureAsync();

await Shell.Builder("sed")
  .WithArguments("-i", "s/alpha/beta/", "notes.txt")
  .RunAsync();
```

`sed -i` starts the edit as soon as the process runs. A preview is a separate command.

## After

`TimeWarp.Amuru` 2.0.0 does the search and the edit in process. Install without `--prerelease`:

```bash
dotnet add package TimeWarp.Amuru --version 2.0.0
```

```csharp
using TimeWarp.Amuru.Native.Text;

CommandOutput found = Commands.SelectString("alpha", "notes.txt");
```

Exit 0 means a line matched. Exit 1 means none matched. Exit 2 is an error. Stdout is `path:line:text`. On a file with `alpha`, then `beta`, then `alpha`, the two matching lines come back and the exit code is 0.

Look at the edit before you write it. `DryRun` fills `Preview` and leaves the file alone. Setting `Backup` on that same call does not create a `.bak`.

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

A one-line file whose text is `alpha` plus a newline produces this preview. The `---` and `+++` lines are the full path of the file.

```text
--- /full/path/notes.txt
+++ /full/path/notes.txt
@@ -1,1 +1,1 @@
-alpha
+beta
```

The file still says `alpha`. Apply it when the diff looks right:

```csharp
CommandOutput edited = Commands.ReplaceInFiles(
  "alpha",
  "beta",
  "notes.txt",
  new ReplaceInFilesOptions { Backup = true });
```

Stdout is one line, `<path>: N replacement(s)`, and only for a file whose text changed. On a separate three-line file (`alpha`, `beta`, `alpha`), the same call writes one line ending in `: 2 replacement(s)`, and the `.bak` is the original text. A later call whose pattern matches nothing still exits 0, writes no stdout line, and leaves the bytes and the mtime alone.

`Bash.Grep` and `Bash.Sed` are the same two `Commands` methods under the names a shell script already uses.

The After samples were run on Linux against nuget.org `TimeWarp.Amuru` 2.0.0. The Before block is the shell-out those samples replace. It was not executed for this draft.
