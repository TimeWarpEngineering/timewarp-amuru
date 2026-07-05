# Fix CommandMock argument matching and interception gaps

## Description

The mock system can silently execute REAL commands inside unit tests. Mock lookup reconstructs arguments by splitting CliWrap's *escaped* argument string on spaces (`core/command-result.cs:324,381,458`), while `CommandMock.Setup` keys on the raw argument array — so any argument containing a space (e.g. `Setup("git","commit","-m","my message")`) never matches, and on mismatch execution falls through to the real process. A test mocking a destructive command can run the real thing on the developer/CI machine.

**BLOCKER for 1.0** — this is the library's testing-story credibility.

## Checklist

- [x] Carry the original `string[]` arguments through `CommandResult` instead of re-splitting the escaped string (`core/command-result.cs:324,381,458`)
- [x] Extend mock interception beyond `RunAsync`/`CaptureAsync`/`RunAndCaptureAsync`: `StreamStdoutAsync`/`StreamStderrAsync`/`StreamCombinedAsync`/`StreamToFileAsync` (`core/command-result.cs:511+`), `PassthroughAsync`, `SelectAsync`, and `Pipe` chains currently bypass mocks entirely — intercept them or document the boundary on `CommandMock.Enable`
- [x] Add a strict mode (STRICT IS NOW THE DEFAULT; MockBehavior.Loose opts out) that throws on unmocked commands when mocking is enabled (today a typo'd `Setup` silently runs arbitrary real commands)
- [x] Align mocked vs real validation (default None via 090 makes mock and real exit-code semantics match) semantics: mock path returns non-zero `ExitCode` while real path (default validation) throws `CommandExecutionException` (`core/command-result.cs:326-352`) — tests pass, production crashes (see task 090)
- [x] `testing/MockSetup.cs:94-98` — `Delays()` never calls `AddSetup`; `Setup(...).Delays(...)` without `Returns`/`Throws` silently registers nothing
- [x] `testing/MockState.cs:87-90` (NUL separator) — key joins args with `|`; arguments containing `|` collide (`["a|b"]` == `["a","b"]`)
- [x] `testing/MockSetup.cs:65` (Throws<T> is now parameterless-ctor only, AOT-safe; use Throws(instance) for messages) — `Throws<TException>(message)` uses `Activator.CreateInstance`: not trim/AOT-safe, throws `MissingMethodException` for exceptions without a string ctor (see task 096)
- [x] `testing/CommandMock.cs:39` (documented same-context disposal requirement) — scope cleanup sets `CurrentMockState.Value = null` in the dispose context only; document/handle cross-context disposal
- [x] Add regression tests (6 added: space args, pipe-char args, strict throw, loose fallthrough, streaming mock, delay-only): multi-word args, pipe-char args, streaming APIs under mock, strict mode

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers. The AsyncLocal isolation design itself is sound (parallel tests don't share state). Paths relative to `source/timewarp-amuru/`.
