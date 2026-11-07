# Model the JSON-RPC child session as a Ramani machine

## Description

Task 083 adds an Amuru-native JSON-RPC client over a child process. The session is long-lived: start, ready, requests in flight, stop, and the child dying are different phases, and a send is legal in only one of them. Implement that session as a Ramani state machine in `TimeWarp.Amuru`. Task 083 keeps the public entry point, the MCP sample, and the NativeAOT consumer check. This task does not uncomment `ShellBuilder.AsJsonRpcClient()`.

`TimeWarp.Ramani` becomes a package dependency of `TimeWarp.Amuru`. That cost is accepted here because the machine is the session, not a test double. Do not reference Ramani from `TimeWarp.Amuru.Tools`.

## Requirements

- The machine is internal. Public API files stay unchanged. `AsJsonRpcClient()` stays commented.
- States are `Idle`, `Starting`, `Ready`, `Stopping`, `Exited`, and `Faulted`.
- The context holds the process seam and the in-flight request map. A request is a map entry, not a state. Concurrent sends stay in `Ready`.
- The process seam is a narrow internal interface: start, write one frame, read one frame, and stop. Tests use a fake. This task does not implement net11 anonymous pipes, `KillOnParentExit`, or `ProcessExitStatus`. Task 083 builds that seam.
- `.Exhaustive()` is on for the signal set below.
- `Idle` accepts `Start` and enters `Starting`. It refuses `Send` and `Response`.
- `Starting` accepts `Started` into `Ready`, `ChildExited` into `Exited`, and `ReadFault` into `Faulted`. It refuses `Send` with the reason `the child is not ready`. It refuses a second `Start`.
- `Ready` accepts `Send`, `Response`, and `RequestTimeout` and stays in `Ready`. `Send` adds one in-flight id. `Response` and `RequestTimeout` complete that id and leave the session in `Ready`. An unknown id is refused with a reason.
- `Ready` accepts `ChildExited` into `Exited` and fails every in-flight id. It accepts `Dispose` into `Stopping`. It refuses `Start` with the reason `already running`.
- `Stopping` accepts `ChildExited` into `Exited` and `ReadFault` into `Faulted`. It refuses `Send`.
- `Exited` and `Faulted` refuse `Send` and `Start`.
- `Ready` owns the stdout read as its do-activity. A read fault takes `ReadFault` into `Faulted`. A request timeout does not fault the session.
- Frames are newline-delimited JSON via `System.Text.Json`. No `Newtonsoft.Json` and no StreamJsonRpc.
- Leave `<Version>` at `3.0.0-beta.1`. Do not cut a release. Task 083 ships the public client and the release note for this dependency.

## Checklist

- [ ] Internal exhaustive Ramani machine and process seam in `TimeWarp.Amuru`
- [ ] `TimeWarp.Ramani` referenced by the core package only
- [ ] Tests step every accepted transition and every refusal, through the fake seam
- [ ] A request timeout completes one id and stays in `Ready`
- [ ] `ChildExited` fails every in-flight id
- [ ] Public API unchanged, including the commented `AsJsonRpcClient()` stub
- [ ] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [ ] `ganda repo audit` is clean

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)

## Notes

Filed from the Ramani review on 2026-11-07. Task 083 remains the public client: `ShellBuilder.AsJsonRpcClient()` (or the equivalent entry point), `JsonSerializerOptions` / `JsonSerializerContext`, stderr observation, PublicAPI, XML docs, the NativeAOT sample, the MCP sample, and the docs. After this task merges, 083 implements the real process seam behind the interface this machine calls and deletes nothing this task proved.

Do not model one state per request. Two overlapping sends would then be illegal.
