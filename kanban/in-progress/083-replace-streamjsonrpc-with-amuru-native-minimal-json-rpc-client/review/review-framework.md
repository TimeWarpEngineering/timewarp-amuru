# Review framework

## Budget (by-diff)

- Lines changed: 2343
- Effort: 3
- TCB hits: none
- Roster axes: general
- Turn cap: 200 (--max-turns; cursor uncapped)

## Framework — task 083

**Date:** 2026-11-08
**Host task:** kanban/in-progress/083-replace-streamjsonrpc-with-amuru-native-minimal-json-rpc-client/
**Diff scope:** branch `task/083-replace-streamjsonrpc-with-amuru-native-minimal-js` vs `master` (merge base), commit 362475f plus kanban landings
**Plan / brief:** `## Goal (implementer restatement)` and the checklist in `task.md`. The work adds the public `JsonRpcClient` / `JsonRpcClientBuilder` and `ShellBuilder.AsJsonRpcClient()`, a real `IJsonRpcProcessSeam` on .NET 11 anonymous pipes (`JsonRpcChildProcess`), M9 (a completion per in-flight entry and a `ConcurrentDictionary`), docs, an AOT sample, and a Python fixture test.
**Effort:** 3 (by-diff budget). The roster axis is general only.
**Reviewer roster:** general
**Session IDs:** review oracle, claude-opus-5-5 (the harness did not expose a session id)

## Ground rules

- Reviewers are read-only on product code. They write only under `review/round-N/`.
- Severity is bug, suggestion, or nit. Status starts as open.
- Do not invent issues to fill space. Zero issues is a valid outcome.
- Address the diff and the call sites around it. Re-verify falsifiable claims against the repo.
- Prior rounds are immutable. New work goes in `round-(N+1)/`.
