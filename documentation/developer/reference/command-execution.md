# Command execution

`Shell.Builder` and `Shell.Run` build a command. `CommandResult` runs it. `CommandOutput` is the result: stdout, stderr, exit code, runtime, and whether the command exceeded its timeout.

## Timeout

Timeout is per command. There is no process-wide default.

```csharp
CommandOutput output = await Shell.Builder("python3")
  .WithArguments("-c", "import time; time.sleep(30)")
  .WithTimeout(TimeSpan.FromSeconds(2))
  .WithTimeoutGracePeriod(TimeSpan.FromSeconds(1))
  .CaptureAsync();
```

The same two methods exist on `CommandOptions` and on `DotNetBuilder`. Leaf tool builders do not each declare `WithTimeout`; they take a `CommandOptions` snapshot, and that snapshot already carries the timeout. `WithTimeout` rejects a non-positive duration. `WithTimeoutGracePeriod` allows zero. Both reject a duration above `Int32.MaxValue` milliseconds, because the timer is millisecond-based.

`CommandOptions.Timeout` is null when unset. `TimeoutGracePeriod` defaults to `CommandOptions.DefaultTimeoutGracePeriod` (5 seconds).

### Signals

CliWrap 3.10.5 runs the process with `ExecuteAsync(forcefulCancellationToken, gracefulCancellationToken)`.

- The graceful token fires when `Timeout` elapses. CliWrap sends the graceful signal (SIGINT on Linux and macOS, Ctrl+C on Windows).
- The forceful token fires when `Timeout` plus `TimeoutGracePeriod` elapses, or immediately when the caller's `CancellationToken` is cancelled. CliWrap then kills the process tree.
- The caller's token and the forceful timer share one linked source. Caller cancellation wins when both fire.

`TtyPassthroughAsync` does not use CliWrap. On Linux and macOS it sends SIGINT itself, then kills the tree after the grace period. Windows has no SIGINT for a non-console child, so the graceful signal is skipped and the grace period ends in the kill. That Windows limit applies to CliWrap's graceful signal as well.

### Result contract

Default validation (`None`) reports a timeout as a result. It does not throw.

| Outcome | What the caller sees |
| --- | --- |
| Timeout, default validation | `CommandOutput.TimedOut` is true, `ExitCode` is `CommandResult.TimeoutExitCode` (124), `Success` is false |
| Child handles the graceful signal and exits 0 | Still a timeout: exit 124. The child's status is not preserved |
| Timeout, `WithZeroExitCodeValidation()` | `TimeoutException` whose message says the command timed out and was terminated. Captured stdout and stderr are on `CommandResult.LastOutput` |
| Caller cancels | `OperationCanceledException`. `TimedOut` is not set and `LastOutput` is left unchanged. `LastOutput` is likewise not updated when any other exception escapes (strict non-zero exit, a mock that throws) or when a stream is abandoned before completion |
| `TtyPassthroughAsync` times out | The same exit-124 result, including under strict validation. This method does not throw on timeout |

124 is the exit code GNU `timeout` uses, so a shell-minded caller can recognize it.

Streaming methods (`StreamStdoutAsync`, `StreamStderrAsync`, `StreamCombinedAsync`) finish the enumerator on timeout under default validation. The lines already produced are yielded. `LastOutput` is set before the enumerator ends; for a real stream it records exit code, timeout, and runtime. Strict validation throws `TimeoutException` after that. `SelectAsync` returns the captured text under default validation and throws under strict validation.

`Pipe` keeps one window for the whole pipeline. The shorter timeout wins. The grace period is the one from that shorter side; equal timeouts keep the smaller grace. Zero-exit validation on either stage makes the pipeline strict. `Pipe` without an options argument keeps the upstream timeout, grace, and validation.

CliWrap applies those two tokens to the last stage. An earlier stage is the last stage's stdin command, and CliWrap cancels it with the forceful token when that pipe stops. The graceful signal is delivered to the last stage. Earlier stages are killed when the pipe cancellation fires, which is when the last stage exits or the forceful timer elapses.

### Mocks

`CommandMock` produces the same result shape. `TimesOut()` sets `TimedOut` and exit 124. A `Delays` value greater than or equal to the command timeout does too. The mock runtime stays zero. Under strict validation the mock throws `TimeoutException`, except `TtyPassthroughAsync`, which still returns the result. Cancelling the caller's token throws `OperationCanceledException`.

## Concurrency

A `CommandResult` is immutable aside from `LastOutput`. Execution methods may be called concurrently on one instance. Each call returns its own `CommandOutput`, exit code, or streamed lines.

`LastOutput` is the most recent execution on this instance. It is not meaningful when the same instance executes concurrently: the last writer wins. Prefer the `CommandOutput` returned by the call. `StreamStdoutAsync`, `StreamStderrAsync`, `StreamCombinedAsync`, and `StreamToFileAsync` do not return `CommandOutput`. For those methods `LastOutput` is the completion record (exit code, timeout, and runtime) when the enumerator or write finishes. A strict timeout still sets `LastOutput` before throwing `TimeoutException`. That property has the same single-writer limit.

`CommandMock` keeps its state in `AsyncLocal`. Parallel async flows that each call `CommandMock.Enable` do not see each other's setups.

## Output size

There is no size limit. The limit is process memory.

| Method | What it retains |
| --- | --- |
| `CaptureAsync`, `RunAndCaptureAsync` | The full output: one `OutputLine` per line, plus the combined strings (`Stdout`, `Stderr`, `Combined`) |
| `StreamStdoutAsync`, `StreamStderrAsync`, `StreamCombinedAsync` | Constant memory per line. A line stays allocated only while the caller keeps it |
| `RunAsync`, `PassthroughAsync` | Do not buffer the output. `RunAsync` writes each line to the terminal. `PassthroughAsync` copies the child's streams to the console |
| `StreamToFileAsync` | Writes each line to the file and does not keep the text on `CommandOutput` |

`CaptureAsync` records lines in arrival order, so stdout and stderr may interleave in `OutputLines`. `RunAndCaptureAsync`, and any `CommandOutput` built from separate stdout and stderr strings, orders all stdout lines before all stderr lines. Those two strings do not carry interleaving.
