#!/usr/bin/env -S dotnet --

#region Purpose
// Tests that a child process receives TRACEPARENT and TRACESTATE from Activity.Current
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The child is `sh -c` printing both variables, or <unset>, so the test reads exactly what the child saw.
// Each test sets Activity.Current itself (AsyncLocal, so it stays in this test's flow) and restores it,
// which keeps the result independent of any span the test runner has open.
// "Untouched" means the child sees the value this process inherited, read at the same moment.
// TTY passthrough inherits the terminal, so its child writes the variables to a temp file instead.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class TraceContext_Given_
  {
    private const string Unset = "<unset>";
    private const string PrintScript = "printf '%s|%s' \"${TRACEPARENT-<unset>}\" \"${TRACESTATE-<unset>}\"";

    [ModuleInitializer]
    internal static void Register() => RegisterTests<TraceContext_Given_>();

    public static async Task CurrentActivity_Should_SetTraceParent()
    {
      using Activity activity = StartActivity(recorded: true, traceState: null);

      (string traceParent, _) = await CaptureChildAsync(Shell.Builder("sh").WithArguments("-c", PrintScript));

      traceParent.ShouldBe($"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-01");
    }

    public static async Task UnrecordedActivity_Should_UseZeroFlags()
    {
      using Activity activity = StartActivity(recorded: false, traceState: null);

      (string traceParent, _) = await CaptureChildAsync(Shell.Builder("sh").WithArguments("-c", PrintScript));

      traceParent.ShouldBe($"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-00");
    }

    public static async Task NoActivity_Should_LeaveEnvironmentUntouched()
    {
      Activity? previous = Activity.Current;
      Activity.Current = null;
      try
      {
        (string traceParent, string traceState) =
          await CaptureChildAsync(Shell.Builder("sh").WithArguments("-c", PrintScript));

        traceParent.ShouldBe(Environment.GetEnvironmentVariable("TRACEPARENT") ?? Unset);
        traceState.ShouldBe(Environment.GetEnvironmentVariable("TRACESTATE") ?? Unset);
      }
      finally
      {
        Activity.Current = previous;
      }
    }

    public static async Task ExplicitCallerValue_Should_Win()
    {
      using Activity activity = StartActivity(recorded: true, traceState: "vendor=activity");
      const string callerParent = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";

      (string traceParent, string traceState) = await CaptureChildAsync
      (
        Shell.Builder("sh")
          .WithArguments("-c", PrintScript)
          .WithEnvironmentVariable("TRACEPARENT", callerParent)
          .WithEnvironmentVariable("TRACESTATE", "vendor=caller")
      );

      traceParent.ShouldBe(callerParent);
      traceState.ShouldBe("vendor=caller");
    }

    public static async Task ExplicitNull_Should_RemoveVariables()
    {
      using Activity activity = StartActivity(recorded: true, traceState: "vendor=activity");

      (string traceParent, string traceState) = await CaptureChildAsync
      (
        Shell.Builder("sh")
          .WithArguments("-c", PrintScript)
          .WithEnvironmentVariable("TRACEPARENT", null)
          .WithEnvironmentVariable("TRACESTATE", null)
      );

      traceParent.ShouldBe(Unset);
      traceState.ShouldBe(Unset);
    }

    public static async Task NonEmptyTraceState_Should_SetTraceState()
    {
      using Activity activity = StartActivity(recorded: true, traceState: "vendor=value");

      (_, string traceState) = await CaptureChildAsync(Shell.Builder("sh").WithArguments("-c", PrintScript));

      traceState.ShouldBe("vendor=value");
    }

    public static async Task EmptyTraceState_Should_LeaveTraceStateUntouched()
    {
      using Activity activity = StartActivity(recorded: true, traceState: null);

      (_, string traceState) = await CaptureChildAsync(Shell.Builder("sh").WithArguments("-c", PrintScript));

      traceState.ShouldBe(Environment.GetEnvironmentVariable("TRACESTATE") ?? Unset);
    }

    public static async Task ResultBuiltBeforeSpan_Should_UseSpanActiveAtStart()
    {
      CommandResult command = Shell.Builder("sh").WithArguments("-c", PrintScript).Build();
      using Activity activity = StartActivity(recorded: true, traceState: null);

      CommandOutput output = await command.CaptureAsync();

      output.Stdout.Split('|')[0]
        .ShouldBe($"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-01");
    }

    public static async Task Pipeline_Should_SetTraceParentOnDownstreamStage()
    {
      using Activity activity = StartActivity(recorded: true, traceState: null);

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("ignored")
        .Pipe("sh", "-c", "cat > /dev/null; " + PrintScript)
        .CaptureAsync();

      output.Stdout.Split('|')[0]
        .ShouldBe($"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-01");
    }

    public static async Task TtyPassthrough_Should_SetTraceParent()
    {
      using Activity activity = StartActivity(recorded: true, traceState: "vendor=tty");
      string file = Path.Combine(Path.GetTempPath(), $"amuru-trace-{Guid.NewGuid():N}.txt");
      try
      {
        CommandOutput result = await Shell.Builder("sh")
          .WithArguments("-c", PrintScript + " > \"$1\"", "sh", file)
          .TtyPassthroughAsync();

        result.ExitCode.ShouldBe(0);
        string[] parts = (await File.ReadAllTextAsync(file)).Split('|');
        parts[0].ShouldBe($"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-01");
        parts[1].ShouldBe("vendor=tty");
      }
      finally
      {
        File.Delete(file);
      }
    }

    public static async Task TtyPassthroughExplicitNull_Should_RemoveTraceParent()
    {
      using Activity activity = StartActivity(recorded: true, traceState: null);
      string file = Path.Combine(Path.GetTempPath(), $"amuru-trace-{Guid.NewGuid():N}.txt");
      try
      {
        await Shell.Builder("sh")
          .WithArguments("-c", PrintScript + " > \"$1\"", "sh", file)
          .WithEnvironmentVariable("TRACEPARENT", null)
          .TtyPassthroughAsync();

        (await File.ReadAllTextAsync(file)).Split('|')[0].ShouldBe(Unset);
      }
      finally
      {
        File.Delete(file);
      }
    }

    private static Activity StartActivity(bool recorded, string? traceState)
    {
      var activity = new Activity("amuru-trace-context-test");
      activity.SetIdFormat(ActivityIdFormat.W3C);
      activity.Start();
      activity.ActivityTraceFlags = recorded ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
      activity.TraceStateString = traceState;
      return activity;
    }

    private static async Task<(string TraceParent, string TraceState)> CaptureChildAsync(ShellBuilder builder)
    {
      CommandOutput output = await builder.CaptureAsync();
      output.ExitCode.ShouldBe(0);
      string[] parts = output.Stdout.Split('|');
      parts.Length.ShouldBe(2);
      return (parts[0], parts[1]);
    }
  }
}
