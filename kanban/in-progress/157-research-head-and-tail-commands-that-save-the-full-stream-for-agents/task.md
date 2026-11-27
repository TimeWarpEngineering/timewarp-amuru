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

- [ ] Each question above has an answer with its evidence
- [ ] A recommendation: build it, build a smaller version, or drop it
- [ ] If build: which part goes in Amuru and which in timewarp-ganda, as task titles
- [ ] The answer is written into `## Results` in this file

## Session

- Created: f616d67d-0c1e-4d2d-afe5-e4a3068beb00 (2026-11-19), from a timewarp-ganda session

## Notes

- A PATH shim is the only way to catch `| tail`. The pipe is made by the shell, so the
  command upstream cannot see what reads it.
- Related: timewarp-ganda task 683 (audit summary and saved report).
