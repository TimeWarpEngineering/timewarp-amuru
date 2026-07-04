# Fix CommandMock argument matching and interception gaps

## Description

The mock system can silently execute REAL commands inside unit tests. Mock lookup reconstructs arguments by splitting CliWrap's *escaped* argument string on spaces (`core/command-result.cs:324,381,458`), while `CommandMock.Setup` keys on the raw argument array — so any argument containing a space (e.g. `Setup("git","commit","-m","my message")`) never matches, and on mismatch execution falls through to the real process. A test mocking a destructive command can run the real thing on the developer/CI machine.

**BLOCKER for 1.0** — this is the library's testing-story credibility.

## Checklist

- [ ] Carry the original `string[]` arguments through `CommandResult` instead of re-splitting the escaped string (`core/command-result.cs:324,381,458`)
- [ ] Extend mock interception beyond `RunAsync`/`CaptureAsync`/`RunAndCaptureAsync`: `StreamStdoutAsync`/`StreamStderrAsync`/`StreamCombinedAsync`/`StreamToFileAsync` (`core/command-result.cs:511+`), `PassthroughAsync`, `SelectAsync`, and `Pipe` chains currently bypass mocks entirely — intercept them or document the boundary on `CommandMock.Enable`
- [ ] Add a strict mode that throws on unmocked commands when mocking is enabled (today a typo'd `Setup` silently runs arbitrary real commands)
- [ ] Align mocked vs real validation semantics: mock path returns non-zero `ExitCode` while real path (default validation) throws `CommandExecutionException` (`core/command-result.cs:326-352`) — tests pass, production crashes (see task 090)
- [ ] `testing/MockSetup.cs:94-98` — `Delays()` never calls `AddSetup`; `Setup(...).Delays(...)` without `Returns`/`Throws` silently registers nothing
- [ ] `testing/MockState.cs:87-90` — key joins args with `|`; arguments containing `|` collide (`["a|b"]` == `["a","b"]`)
- [ ] `testing/MockSetup.cs:65` — `Throws<TException>(message)` uses `Activator.CreateInstance`: not trim/AOT-safe, throws `MissingMethodException` for exceptions without a string ctor (see task 096)
- [ ] `testing/CommandMock.cs:39` — scope cleanup sets `CurrentMockState.Value = null` in the dispose context only; document/handle cross-context disposal
- [ ] Add regression tests: multi-word args, pipe-char args, streaming APIs under mock, strict mode

## Notes

Found by multi-agent release review (2026-07-04); confirmed independently by two reviewers. The AsyncLocal isolation design itself is sound (parallel tests don't share state). Paths relative to `source/timewarp-amuru/`.
