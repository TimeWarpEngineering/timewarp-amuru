# Research: head and tail commands that save the full stream for agents

## Description

Research only. Do not build the commands in this task. The result is a recommendation
Steven can say yes or no to.

**The problem.** Agents pipe long output through `tail -40` or `head -20` by reflex. The
part that mattered is cut off, and they run the whole command again to see it. Measured on
timewarp-ganda task 681 (2026-11-19): a 136s `ganda repo audit` went through `tail -40`,
the failures were hidden, and a second full audit ran only to grep for them. The same
walk piped a 63s and a 149s test run through `tail`.

**The idea** (Steven, 2026-11-19). Write our own `head` and `tail`, put them ahead of the
system ones on the agent's PATH, and have them save the whole stream to a file. The agent
gets the slice it asked for plus one line that says where the rest is. Agents change
nothing.

**Why it is research and not a build task.** It replaces two core tools for every command
an agent runs. The questions below decide whether that is safe, and none of them has an
answer yet. Steven: "just an idea … file it as a research task, not a do".

**Why this repo.** Amuru already holds in-process replacements for shell tools under
`source/timewarp-amuru/native/`: `GetContent` (cat), `SelectString` (grep) and
`ReplaceInFiles` (sed), with bash aliases in `native/aliases/bash.cs`. Head and tail are
the same family. Ganda's private library, Zana (`source/timewarp-zana/` in timewarp-ganda,
not packable), holds ganda's own git, kanban, repo and nuget helpers. It has no shell-tool
replacements. This readme already points to Ganda for the CLI tools, so the likely split
is: the stream logic here, the executables and the PATH setup in timewarp-ganda. Confirm
or correct that split in the answer.

## Goal (implementer restatement)

Decide, with measured evidence, whether we should ship our own `head` and `tail` that sit
first on the agent's PATH, print exactly the slice the real tool would print, and save the
whole stream to a file so an agent never re-runs a long command to see what the slice cut.
No commands get built here.

Acceptance:

- Each of the nine questions has an answer backed by a measurement, a transcript count, or
  a source reference (stdout identity and `2>&1`, early-exit cost and the cap, which forms
  are ours, finding the real binary per OS, file location and cleanup, whose PATH per
  harness, AOT startup against `/usr/bin/head`, how often a cut caused a re-run, and what
  ganda 683 and Claude Code's own large-output save already cover).
- One recommendation: build, build smaller, or drop.
- If build: the Amuru/timewarp-ganda split, as task titles.
- All of it written under `## Results` in this file. No product code changes.

## Questions to answer

1. **Does stdout stay identical?** Proposal: the slice goes to stdout exactly as the real
   tool prints it, and the "saved to" line goes to stderr. Check that a script such as
   `x=$(git log | head -1)` gets the same value. Find the cases where a caller merges
   stderr into stdout (`2>&1`) and say what breaks.
2. **Early exit.** The real `head` exits after N lines, and that stops the command
   upstream. Ours has to keep reading to save everything. What does that cost on
   `git log | head -5` in a large repo? What happens on a stream that never ends? Propose
   the cap (bytes, lines, or time) after which it stops saving and exits like the real one.
3. **Which invocations are ours?** Proposal: only the pipe case with `-n N` or `-N`. File
   arguments, `-f`, `-c`, `-n +N` and anything else go to the real binary. List the forms
   agents use. Count them from real transcripts under the Claude projects folders and the
   walk oracle logs.
4. **How does it find the real binary** without calling itself, on Linux, macOS and Windows?
5. **Where does the saved file go, and when is it deleted?** Walks run at the same time in
   many worktrees.
6. **Whose PATH?** Agent shells and walk workers only. Not Steven's terminal and not CI.
   Name the place that sets it for each harness (Claude Code, Grok, OpenCode) and for the
   headless walk workers.
7. **Startup cost.** Measure an AOT build of a do-nothing Nuru app against `/usr/bin/head`.
   Agents run these thousands of times a day.
8. **Does it pay?** Count how often an agent re-ran a command after a `head` or `tail`
   cut. One measured case is not enough to justify replacing core tools.
9. **What already covers this?** timewarp-ganda task 683 makes `repo audit` print a short
   summary and save its report. Claude Code saves very large tool output to a file by
   itself. Say what is left for this idea after those two.

## Checklist

- [x] Each question above has an answer with its evidence
- [x] A recommendation: build it, build a smaller version, or drop it
- [x] If build: which part goes in Amuru and which in timewarp-ganda, as task titles
- [x] The answer is written into `## Results` in this file

## Results

No product change from the research itself: this is a research task, and the answer is below. The only
product diff is the audit boy-scout fixes listed under Gates (package pins, line-cap baseline). The scan scripts and the
throwaway prototype are under `research/` in this folder (`research/readme.md`).

### Recommendation

**Build a smaller version: `tail` only. Drop `head`.**

- Ship a POSIX `sh` shim named `tail`, not a .NET program. It catches one form: reading a pipe
  with `-n N`, `-N`, or no arguments. It saves the stream with `tee`, prints the slice with the
  real `tail` on the saved file, and writes one stderr line only when lines were cut. Every
  other form `exec`s the real `tail`.
- Put it on PATH through the agent `BASH_ENV` that ganda already sets for every harness child.
  It never reaches Steven's terminal or CI.
- Drop `head`. Replacing it changes pipeline behaviour (upstream keeps running, `pipefail`
  status changes), and the re-run data is mostly about `tail` (139 of 169 cases).

Why build anything: re-runs after a cut are real but small. They cost about 119 minutes of
re-run time in 79 days, 148 of the 169 cases in November. Task 683 already removes the
`ganda repo audit` share. The tail shim is cheap, has no measurable startup cost, and keeps
stdout byte-identical by construction. So it pays for itself, but only in this small form.

### Split (corrects the assumption in the description)

Nothing goes in Amuru. The stream logic is about 15 lines of `sh` (`tee`, real `tail`,
`wc -l`). A .NET version would need an `execv` P/Invoke on Unix and a stdio-proxy child on
Windows for the passthrough case, and it would buy nothing measurable (see Q7). Amuru's
`GetContent`, `SelectString`, and `ReplaceInFiles` are in-process C# APIs, not PATH
executables, so they are not the same family as a PATH shim.

Task titles:

1. timewarp-ganda: **Agent shells get a tail shim that saves the full stream when it cuts**.
   It ships `agent-bin/tail` (sh) and installs it under `~/.timewarp/agent-bin/`. The save
   folder, cap, and sweep are as in Q5. It has a test that `x=$(seq 100 | tail -1)` is `100`
   and that file-argument forms are passed through.
2. timewarp-ganda: **Agent BASH_ENV prepends the agent-bin folder to PATH**.
   `AgentShellProfile.Effective` points at a ganda-written wrapper file. The wrapper sources the
   existing `BASH_ENV` or `~/.bashrc`, then prepends `~/.timewarp/agent-bin`. `ganda doctor`
   probes that `command -v tail` in an agent shell resolves to the shim.
3. timewarp-flow: **Hand-launched Claude and OpenCode sessions use the ganda agent BASH_ENV**.
   Claude: a `SessionStart` hook in `shared/claude-settings.json` writes the export to
   `$CLAUDE_ENV_FILE`. OpenCode: `opencode/plugins/bash-env.ts` points at the same wrapper.

### Evidence base

- Claude transcripts: the `.jsonl` files under `$CLAUDE_CONFIG_DIR/projects`, Bash calls from
  2026-09-09 to 2026-11-26 (79 days). Both scripts take `--until 2026-11-27`, so later
  transcripts do not move the numbers. Headless walk workers run Claude in print mode and
  write their transcripts there too, so walks are included. There are 71,281 unique Bash
  calls, deduped by tool-use id (the raw files repeat about 30% of them). 41,466 calls (58%)
  use `head` or `tail`.
- Walk oracle logs: `find ~/.timewarp/ganda/walks -name 'oracle-*.log' | wc -l` printed 522 on
  2026-11-27. These files hold the oracle result JSON plus `ORACLE_EXIT`, not the workers'
  Bash commands. Those commands are in the Claude transcripts above, so they were not counted
  again. Grok and OpenCode transcripts were not scanned.
- On this machine `/usr/bin/head` and `/usr/bin/tail` are **uutils coreutils 0.10.0** (Rust),
  not GNU. The "real tool" differs from machine to machine, which is one more reason never to
  reimplement the slice.

### Q1. Does stdout stay identical?

Yes, by construction, if the shim prints the slice by running the real `tail -n N` on the
saved file. Checked with `research/proto-tail.sh` symlinked onto `PATH` as `tail`, with
`TMPDIR` set to an empty folder:

- `x=$(seq 100 | tail -1)` gives `100`. The note is on stderr, so a capture does not see it.
  One `stream.*` file stays in `TMPDIR`.
- If nothing was cut (`seq 3 | tail -5`), the prototype prints all three lines, writes no
  note, and removes its `stream.*` file before it exits. `TMPDIR` is empty afterwards.
- File arguments (`tail -3 in.txt`) go to the real binary unchanged.
- A last line with no newline (`printf 'a\nb\nc' | tail -2`) is counted as a line, as the
  real `tail` does (the prototype counts with `awk`, not `wc -l`).

What breaks is any place where the shim's own stderr is merged into captured stdout:

- `x=$(cmd | tail -1 2>&1)` and `x=$( { cmd | tail -1; } 2>&1 )`: the value gets a second
  line.
- `cmd | tail -2 > out.txt 2>&1`: the note lands in the file.
- A caller that treats any stderr as failure.

In the transcripts this is rare (`slice-forms-scan.py`, the `Q1 …` counters, head and tail
together). 13 calls put `2>&1` on the slice itself, 1 wraps the slice in a group or subshell
and then uses `2>&1`, and 1 capture has `2>&1` inside. Out of 38,956 calls with a piped slice,
2,088 capture a slice with `$( … )`. Only that one capture with `2>&1` inside is affected. `cmd 2>&1 |
tail -40` (the common agent form) is fine: that `2>&1` is on `cmd`. The harness merges stdout
and stderr in the tool result, so the agent sees the note.

`pipefail`: the real `head` makes upstream die with SIGPIPE. With `set -o pipefail`,
`yes | head -1` and `git log | head -1` (ganda, 8,680 commits) both return 141. A head that
drains changes that to 0. The real `tail` reads to the end anyway, so a tail shim does not
change it. 94 calls set `pipefail`.

### Q2. Early exit

This applies to `head` only. `tail` must read the whole stream already, so a saving tail
costs one `tee` write and nothing else. Measured with hyperfine (10 runs) on timewarp-ganda
master:

| Command | Real `head` | Save the whole stream, then head |
|---|---|---|
| `git log \| head -5` (2.76 MB) | 11.2 ms | 79 ms |
| `git log -p \| head -5` (122 MB) | 16.0 ms | 2.35 s |

A stream that never ends (`yes | head -5`, `--watch | head -1`) never finishes without a cap.
If `head` is ever built, use this cap: print the N lines as soon as they arrive, keep saving
until 4 MiB **or** 1 s after the slice is complete (whichever comes first), then exit so
upstream gets SIGPIPE like with the real one. Even capped, upstream does up to 1 s more work,
and that matters for side-effecting upstreams (`find … -delete | head`). That is the main
reason to drop `head`.

### Q3. Which invocations are ours?

Counted in the transcripts (`research/slice-forms-scan.py`, the `-- forms (Q3)` lines), all
piped or unpiped uses:

| Form | Count | Ours? |
|---|---|---|
| `\| head -N` | 30,855 | head (dropped) |
| `\| tail -N` | 14,821 | **yes** |
| `\| head` (no args, 10 lines) | 13,333 | head (dropped) |
| `tail … FILE` (no pipe) | 3,543 | no, real |
| `head … FILE` (no pipe) | 1,781 | no, real |
| `-c` (head and tail, all) | 700 | no, real |
| `\| tail -n N` | 210 | **yes** |
| `\| head -n N` | 49 | head (dropped) |
| `-n +N` / `+N` | 62 | no, real |
| `\| tail` (no args) | 33 | **yes** |
| `-f`, `-q`, `-v`, `-z`, file after a pipe, no-pipe `-N` on a redirect, other | 338 | no, real |

The rows sum to the script's `total head/tail uses`, 65,725. The pipe forms with `-N`, `-n N`,
or no args are 59,301 of them (90%), which is the script's `piped numeric slices` counter.
The tail share is 15,064. The rule "stdin is not a terminal, no file operand, only `-n N`, `-N`, or nothing" is
easy to check in `sh` before anything else runs.

### Q4. How does it find the real binary?

- **Linux and macOS:** the shim strips its own folder (`~/.timewarp/agent-bin`) from `PATH`
  and runs `command -v tail`, then `exec`s it, so it can never call itself. `TW_REAL_TAIL`
  overrides this for tests. macOS gets the BSD `tail`. The `-n N` stdin output is the same,
  and every other form keeps BSD semantics because it is passed through.
- **Windows:** Claude Code on Windows runs tool commands in Git Bash, where `tail` is
  `/usr/bin/tail` from Git. A sh shim works unchanged there. PowerShell has no `tail`, so
  there is nothing to catch. A .NET shim would have no `exec` on Windows and would have to
  proxy stdio through a child, which is another reason for `sh`.

### Q5. Where does the saved file go, and when is it deleted?

- Folder: `${TMPDIR:-/tmp}/tw-agent-streams/<uid>/`, mode 0700. Not under
  `$GANDA_HOME/walks` (host-owned) and not in the worktree (it would dirty git).
- Name: `mktemp tail-<UTC yyyymmddThhmmss>-<pid>-XXXXXX.log`. The pid and random suffix keep
  concurrent walks in many worktrees from colliding, so no per-walk folder is needed.
- Written only when lines were cut. Otherwise the file is removed before the shim exits. In
  the transcripts 11,694 of the 15,064 piped numeric `tail` slices had a tool result of at
  least N lines (`tail piped numeric cut (lines >= N)` / `tail piped numeric slices`; 43,436
  of 59,301 for head and tail together). A line count counts a trailing newline as the end
  of the last line. The tool result holds all of the call's output, not only the slice, so
  this is an upper bound. Expect at most about 150 small files a day.
- Cap: stop writing at 64 MiB and say so in the note.
- Deletion: files older than 24 h are removed by the shim at most once an hour (a stamp-file
  mtime check, then `find -mmin +1440 -delete` in the background). `/tmp` is tmpfs here
  (48 GB), so a reboot clears it too.
- Note text: `tail: showed last N of M lines; full output: <path>`.

### Q6. Whose PATH?

The common lever is `BASH_ENV`. All three harnesses run tool commands as `bash -c`, which
reads only `$BASH_ENV`. Interactive terminals never read it, and CI never gets ganda's value.

- **Headless walk workers and Herdr tabs launched by ganda (Claude, Grok, OpenCode):**
  `AgentChildEnvironment.Variables` → `AgentShellProfile.Effective`
  (`timewarp-ganda/source/timewarp-ganda/agents/agent-child-environment.cs`,
  `agent-shell-profile.cs`) already sets `BASH_ENV` for every child, including the Herdr pane
  script (`services/oracle-launch/herdr-pane-command-launch.cs`). Its design note says Claude,
  Grok, and OpenCode headless all pass `BASH_ENV` to their shell tool (checked 2026-11-19).
  Pointing it at a wrapper that sources the old value and then prepends `agent-bin` covers
  every walk with one change.
- **Hand-launched Claude Code:** Claude Code 2.1.332 supports `CLAUDE_ENV_FILE` ("write bash
  exports there to apply env to subsequent BashTool commands"). A `SessionStart` hook in
  `timewarp-flow/shared/claude-settings.json` appends the export. That file already has a
  `SessionStart` hook (`ganda herdr claude-hook`).
- **Hand-launched OpenCode:** `timewarp-flow/opencode/plugins/bash-env.ts` already sets
  `BASH_ENV` on `shell.hook("create.before")`. Point it at the wrapper.
- **Hand-launched Grok:** `grok/hooks/herdr.json` has `SessionStart`, but I found no hook that
  sets the shell environment. Grok is covered only when ganda launches it. That is acceptable,
  since walks are ganda-launched.
- **Steven's terminal and CI:** untouched. Neither sets the agent `BASH_ENV`, and interactive
  bash ignores it.

### Q7. Startup cost

hyperfine, 300 runs each, stdin is a 100-line file, AOT published (`dotnet publish`, .NET 11):

| Binary | Mean | Size |
|---|---|---|
| `/usr/bin/tail -n 5` (uutils) | 2.1 ms | |
| `/usr/bin/head -n 5` (uutils) | 2.3 ms | |
| bare AOT C# (`return 0;`) | 1.8 ms | 0.9 MB |
| AOT Nuru 4.0.0-beta.18, one route `tail -n {count:int}` | 2.7 ms | 4.7 MB |
| `sh` shim that `exec`s `/usr/bin/tail` | 2.5 ms | |

Startup is not a concern either way: +0.4 ms for the sh shim and +0.6 ms for Nuru. At about
190 piped tails a day that is well under a second a day. The Nuru app was:

```csharp
#:package TimeWarp.Nuru@4.0.0-beta.18
#:property PublishAot=true
#:property InvariantGlobalization=true
using TimeWarp.Nuru;
NuruApp app = NuruApp.CreateBuilder()
  .Map("tail -n {count:int}").WithHandler((int count) => 0).AsQuery().Done()
  .Build();
return await app.RunAsync(args);
```

### Q8. Does it pay?

`research/rerun-after-cut-scan.py` looks for a piped slice that returned at least N lines,
then the same upstream command re-run within the next 4 Bash calls, with no
Edit/Write/MultiEdit and no mutating bash (`sed -i`, `git commit`, `--fix`, …) in between.

A cut is a piped `-N`, `-n N`, or no-argument slice whose tool result has at least N lines.

- Loose count (the `-- loose` block): 880 re-runs (`cheap loose rerun` 684, commands such as
  `git`, `grep`, `ls`; `exp loose rerun` 196). Of these, 136 re-runs took at least 30 s
  (15,328 s in total). A hand check of 30 of them, on the first run of the scan, found about a
  third were real "re-run to see what was cut". The rest were `git status` repeats or chains
  where another command took the time.
- Strict count (the `-- strict` block): the upstream matches one of the table rows below, and
  one statement of the next command (split on `&&`, `;`, and newlines) starts with that exact
  upstream. **169 cases in 79 days, 148 of them in November.** 139 followed a `tail`, 30 a
  `head`. The first run's duration, which approximates the cost of re-running it, adds up to
  **7,169 s (119 min)**:

| Upstream | Cases | Seconds |
|---|---|---|
| `ganda repo audit` (or `dev audit`) | 39 | 730 |
| `dev test` | 18 | 2,286 |
| `dev build` | 26 | 1,224 |
| `dotnet test/run` | 35 | 1,375 |
| `dev deps` | 14 | 760 |
| `dotnet build/publish` | 18 | 592 |
| other `bin/dev` verbs or `run-tests` | 19 | 202 |

- The strict check is a text match. A spot check of 12 strict rows (`--rows`) found 11 re-runs
  of the same command with a different filter or slice. One followed a file edit made through a
  python heredoc, which the mutation filter does not see.
- In November that is 6,436 s, about 4 minutes of re-run time a day across all sessions, plus
  the tokens of the second output. The worst single case was 434 s
  (`./bin/dev test 2>&1 | tail -25`, then re-run with a grep).
- Not measurable: 3,037 slices of a strict upstream were cut and **not** re-run
  (`strict-upstream cut, not rerun`). Some of those agents
  went on without the hidden part. A saved file helps there as well, but the transcripts
  cannot show how often it mattered.

It pays only because the smaller version is cheap and low risk. It would not justify a .NET
`head` replacement.

### Q9. What already covers this?

- **timewarp-ganda 683** (done, landed 2026-11-27): `ganda repo audit` prints a short summary
  plus `Report saved: …`, and `--last` reprints it. That removes the audit row above (39 cases,
  730 s) and the measured task-681 case. Seven audit re-runs still happened 2026-11-24..26,
  before it landed.
- **Claude Code's own save:** output over about 30 KB is replaced with `Output too large
  (NN KB). Full output saved to: …`, seen about 1,000 times in the transcripts. It only acts
  on what reaches the harness. `| tail -40` cuts the stream before Claude Code sees it, so this
  never covers the re-run cases.
- **What is left:** `dev test`, `dev build`, `dev deps`, and `dotnet test/run` piped through
  `tail` or `head`: 93 strict cases, 5,645 s (94 min). A 683-style summary-and-saved-log in the dev CLI would
  cover the biggest rows too, but that is a per-tool fix. The tail shim covers every command
  at once, including repos without a dev CLI.

### How to validate

Smoke: from this folder, run
`python3 -I research/slice-forms-scan.py "$CLAUDE_CONFIG_DIR/projects" --until 2026-11-27` and
`python3 -I research/rerun-after-cut-scan.py "$CLAUDE_CONFIG_DIR/projects" --until 2026-11-27`.
Neither writes a file unless `--rows FILE` is passed. Then symlink `research/proto-tail.sh`
as `tail` into an empty folder `D`, set `TMPDIR` to another empty folder, and run
`PATH=D:$PATH bash -c 'x=$(seq 100 | tail -1); echo "[$x]"'` and
`PATH=D:$PATH bash -c 'seq 3 | tail -5'`.

Expect: `slice-forms-scan.py` prints `65725 total head/tail uses`, `59301 piped numeric
slices`, the Q3 rows, and the Q1 counters as written above. `rerun-after-cut-scan.py` prints
`169 total 7169` in the `-- strict` block, `139 after tail`, `30 after head`, the Q8 rows, and
`880 loose rerun`. These hold while the transcripts for that window are on disk. The first
prototype run prints `[100]` on stdout, one `tail: showed last 1 of 100 lines; full output: …`
line on stderr, and leaves one `stream.*` file. The second prints `1 2 3`, no note, and leaves
no new file.

### Gates

The research changes only this task folder. The first audit failed on two existing checks,
which were fixed on this branch:

- `package-pin-currency`: `ganda repo audit --fix` bumped `TimeWarp.Ramani` from
  1.0.0-beta.24 to beta.25, and `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` from 4.0.0-beta.16
  to beta.18, in `Directory.Packages.props`.
- `source-file-line-cap`: the check now counts markdown. Five existing docs over 500 lines
  (`readme.md` 509, `documentation/user/installation.md` 593,
  `documentation/conceptual/architectural-layers.md` 976,
  `documentation/developer/progressive-enhancement-pattern.md` 1414, and
  `.agent/workspace/2025-12-11T16-45-00_issue-39-tty-passthrough-analysis.md` 526) were
  recorded in `.timewarp/line-cap-baseline.json` at their current length, as the check says.
- `vscode-window-icon` (warning): `--fix` wrote the gitignored `.vscode/settings.json`.

Gate results:

- `./bin/dev build`: exit 0, 0 warnings, 0 errors. Run before and after the package bump.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs`: exit 0, 31 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.guard.cs`: exit 0, 14 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs`: exit 0, 20 passed.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs`: exit 0, 19 passed.
- `tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.fake-seam.cs` is
  not a standalone runfile (no `#if !JARIBU_MULTI` guard). CI runs it.
- `dotnet run --file tools/dev-cli/dev.cs -- self-install`: exit 0. This recompiled the dev
  CLI against Nuru beta.18.
- `ganda repo audit`: first run exit 1 (the two failures above). Then `--fix` plus the baseline
  entries. The final run is recorded under `## Session`.

Fix pass for review round 1 (M1–M6). It changes only this task folder: the two scan scripts,
the prototype, `research/readme.md`, and the numbers above. No C# changed, so no test file
covers it.

- M1: `proto-tail.sh` now removes its `stream.*` file when nothing was cut. Re-checked the Q1
  commands with the prototype symlinked as `tail` and an empty `TMPDIR`.
- M2: both scripts count lines with `str.splitlines`, so a trailing newline ends the last line.
- M3: `rerun-after-cut-scan.py` encodes the strict predicate and prints the Q8 table, the
  tail/head split, and the month split. `slice-forms-scan.py` prints the Q1 counters.
- M4: the Q5 denominator is the `piped numeric slices` counter from the same run as Q3.
- M5: the Q3 rows sum to the stated total.
- M6: the oracle-log count is the `find` count, 522.
- Both scans take `--until 2026-11-27`, and the scripts write no file unless `--rows` is given.

Fix-pass gate results:

- `./bin/dev build`: exit 0, 0 warnings, 0 errors.
- Test files: none added or edited, and no production code changed.
- `python3 -I research/slice-forms-scan.py "$CLAUDE_CONFIG_DIR/projects" --until 2026-11-27`:
  exit 0, numbers as in Q1, Q3, and Q5.
- `python3 -I research/rerun-after-cut-scan.py "$CLAUDE_CONFIG_DIR/projects" --until 2026-11-27`:
  exit 0, numbers as in Q8 and Q9.
- `ganda repo audit`: recorded under `## Session`.

### Review disposition

Round 2 is clean. M1–M6 are fixed. No new findings.

- Rounds: 2. Roster: general. Effort: 2. Turn cap: uncapped in this session (review budget 120).
- Final counts: bug 0 open, 4 fixed, 0 wontfix. Suggestion 0. Nit 0 open, 2 fixed, 0 wontfix.
- Disposition: clean (`review/disposition.md`).
- Paths: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`.

## Session

- Created: f616d67d-0c1e-4d2d-afe5-e4a3068beb00 (2026-11-19), from a timewarp-ganda session
- 2026-11-27: implement oracle (Claude, `ganda task work 157`). Scanned the Claude transcripts,
  timed the early-exit and startup cases with hyperfine, read the ganda agent launch code and
  the harness configs in timewarp-flow, wrote `## Results`. No product code.
- 2026-11-27: final `ganda repo audit` after the audit fixes: exit 0, 43 passed, 0 failed,
  0 warnings.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 16672e49-ce38-4da3-942e-119c5e4ec6c4, max-turns 500, Done — 2026-11-27T10:33:25Z
- Reviewer: general (cursor, vendor grok, model grok-4.7), session 0c6c085c-dbed-434b-85c1-7c18bacafc0b, effort 2, roster general, turn cap uncapped (review budget 120), round 1 open — 2026-11-27
- Review oracle: review by reviewer-cursor (cursor, vendor xai, model grok-4.7-high), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts general.md, merged.md — 2026-11-27T10:44:50Z
- 2026-11-27: implement-fix-2 (review round 1, M1–M6): fixed the prototype and both scans,
  re-ran them with `--until 2026-11-27`, replaced the Q1, Q3, Q5, Q8, and Q9 numbers.
  `./bin/dev build` exit 0. Final `ganda repo audit`: exit 0, 10 passed, 0 failed, 1 skipped
  (it reused the recorded pass for the unchanged tree and re-ran the checks that read kanban/).
- Reviewer round 2: general (cursor, vendor xai, model grok-4.7), session not reported, effort 2, roster general, turn cap uncapped (review budget 120), round 2 clean — 2026-11-27
- Review oracle round 2: review by reviewer-cursor (cursor, vendor xai, model grok-4.7), session not reported, max-turns uncapped (no --max-turns flag; bound 120), artifacts `review/round-2/general.md`, `review/round-2/merged.md`, `review/disposition.md` — 2026-11-27

## Learnings

The restatement adds "No product code changes." The task asks for the recommendation in `## Results` and says not to build the head and tail commands. The branch also updates `Directory.Packages.props` and `.timewarp/line-cap-baseline.json` so audit passes, and Results records those edits under Gates. The nine questions in the restatement match the task. The Amuru / ganda / flow split answers the task's request to confirm or correct the assumed split.

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5, M6
- Round 2: Done
## Notes

- A PATH shim is the only way to catch `| tail`. The pipe is made by the shell, so the
  command upstream cannot see what reads it.
- Related: timewarp-ganda task 683 (audit summary and saved report).
