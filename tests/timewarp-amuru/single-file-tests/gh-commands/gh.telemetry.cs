#!/usr/bin/env -S dotnet --

#region Purpose
// Span name, parent, and tags for Gh builder executions, and the rule that no token, field value,
// query string, or stdout reaches a tag (task 152). gh runs under CommandMock; no GitHub call.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// One process-wide ActivityListener samples Gh.ActivitySourceName and keeps every stopped span.
// Each test starts its own parent Activity and reads back only the spans in that trace, so tests
// running in parallel do not see each other's spans.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Gh_
{
  using System.Collections.Concurrent;

  [TestTag("GhCommands")]
  public class Telemetry_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Telemetry_Given_>();

    private const string Token = "gho_DO_NOT_EXPORT_0123456789";

    public static async Task CaptureAsync_Should_OpenAChildSpanNamedAfterTheCommand()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "run", "list", "--repo", "o/r", "--limit", "5").Returns("[]");

      CommandOutput output = await Gh.Run().List().WithRepo("o/r").WithLimit(5).CaptureAsync();

      output.Success.ShouldBeTrue();
      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.DisplayName.ShouldBe("gh run list");
      span.Source.Name.ShouldBe(Gh.ActivitySourceName);
      span.Kind.ShouldBe(ActivityKind.Client);
      span.ParentSpanId.ShouldBe(parent.SpanId);
      span.GetTagItem("gh.command").ShouldBe("run list");
      span.GetTagItem("gh.repo").ShouldBe("o/r");
      span.GetTagItem("gh.exit_code").ShouldBe(0);
      span.GetTagItem("gh.elapsed_ms").ShouldBeOfType<double>().ShouldBeGreaterThanOrEqualTo(0);
      span.GetTagItem("gh.api.endpoint").ShouldBeNull();
      span.Status.ShouldBe(ActivityStatusCode.Unset);
    }

    public static async Task NoRepo_Should_OmitTheRepoTag()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "issue", "list").Returns("");

      await Gh.Issue().List().CaptureAsync();

      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.GetTagItem("gh.command").ShouldBe("issue list");
      span.GetTagItem("gh.repo").ShouldBeNull();
    }

    public static async Task RunAsyncAndRunAndCaptureAsync_Should_EachOpenASpan()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "pr", "checks", "42").Returns("", exitCode: 8);
      CommandMock.Setup("gh", "release", "list").Returns("");

      int exitCode = await Gh.Pr().Checks("42").RunAsync();
      CommandOutput output = await Gh.Release().List().RunAndCaptureAsync();

      exitCode.ShouldBe(8);
      output.Success.ShouldBeTrue();
      string[] names = [.. SpansOf(parent).Select(span => span.DisplayName)];
      names.ShouldBe(["gh pr checks", "gh release list"], ignoreOrder: true);
      SpansOf(parent).Single(span => span.DisplayName == "gh pr checks").GetTagItem("gh.exit_code").ShouldBe(8);
    }

    public static async Task NonZeroExit_Should_SetErrorStatusWithTheExitCode()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "pr", "view").ReturnsError("no pull requests found for branch", exitCode: 1);

      CommandOutput output = await Gh.Pr().View().CaptureAsync();

      output.ExitCode.ShouldBe(1);
      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.Status.ShouldBe(ActivityStatusCode.Error);
      span.StatusDescription.ShouldBe("exit code 1");
      span.GetTagItem("gh.exit_code").ShouldBe(1);
    }

    public static async Task Exception_Should_SetErrorStatusWithTheTypeNameOnly()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "status").Throws(new InvalidOperationException($"leaked {Token}"));

      await Should.ThrowAsync<InvalidOperationException>(() => Gh.Auth().Status().CaptureAsync());

      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.Status.ShouldBe(ActivityStatusCode.Error);
      span.StatusDescription.ShouldBe(nameof(InvalidOperationException));
      AssertNoTagContains(span, Token);
    }

    public static async Task AuthToken_Should_NeverTagTheToken()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "token", "--hostname", "github.com").Returns(Token + "\n");

      CommandOutput output = await Gh.Auth().Token().WithHostname("github.com").CaptureAsync();

      output.Stdout.Trim().ShouldBe(Token);
      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      string[] keys = [.. span.TagObjects.Select(tag => tag.Key)];
      keys.ShouldBe(["gh.command", "gh.exit_code", "gh.elapsed_ms"], ignoreOrder: true);
      AssertNoTagContains(span, Token);
      span.Events.ShouldBeEmpty();
    }

    public static async Task Api_Should_TagThePathWithoutQueryOrFieldValues()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup(
        "gh",
        "api", "repos/o/r/dispatches?access_token=" + Token,
        "-f", "event_type=rebuild",
        "-f", "client_payload[secret]=" + Token,
        "--input", "body.json").Returns("");

      await Gh.Api("repos/o/r/dispatches?access_token=" + Token)
        .WithRawField("event_type", "rebuild")
        .WithRawField("client_payload[secret]", Token)
        .WithInput("body.json")
        .CaptureAsync();

      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.DisplayName.ShouldBe("gh api");
      span.GetTagItem("gh.command").ShouldBe("api");
      span.GetTagItem("gh.api.endpoint").ShouldBe("repos/o/r/dispatches");
      AssertNoTagContains(span, Token);
      AssertNoTagContains(span, "access_token");
      AssertNoTagContains(span, "rebuild");
      AssertNoTagContains(span, "body.json");
    }

    public static async Task ApiWithFullUrl_Should_TagOnlyThePath()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      string endpoint = $"https://user:{Token}@api.github.com/repos/o/r/actions/runs?per_page=5#top";
      CommandMock.Setup("gh", "api", endpoint).Returns("{}");

      await Gh.Api(endpoint).CaptureAsync();

      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.GetTagItem("gh.api.endpoint").ShouldBe("/repos/o/r/actions/runs");
      AssertNoTagContains(span, Token);
      AssertNoTagContains(span, "api.github.com");
    }

    public static async Task CreateBodies_Should_NeverReachATag()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "issue", "create", "--title", "t", "--body", Token, "--repo", "o/r").Returns("");

      await Gh.Issue().Create().WithTitle("t").WithBody(Token).WithRepo("o/r").CaptureAsync();

      Activity span = SpansOf(parent).ShouldHaveSingleItem();
      span.GetTagItem("gh.repo").ShouldBe("o/r");
      AssertNoTagContains(span, Token);
    }

    public static async Task Build_Should_NotOpenASpan()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "run", "list").Returns("");

      await Gh.Run().List().Build().CaptureAsync();

      SpansOf(parent).ShouldBeEmpty();
    }

    public static async Task CallerActivity_Should_BeRestoredAfterTheRun()
    {
      using Activity parent = StartParent();
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "run", "list").Returns("");

      await Gh.Run().List().CaptureAsync();

      Activity.Current.ShouldBeSameAs(parent);
    }

    private static readonly ConcurrentQueue<Activity> Stopped = new();

    private static readonly ActivityListener Listener = CreateListener();

    private static ActivityListener CreateListener()
    {
      ActivityListener listener = new()
      {
        ShouldListenTo = source => source.Name == Gh.ActivitySourceName,
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        ActivityStopped = Stopped.Enqueue,
      };
      ActivitySource.AddActivityListener(listener);
      return listener;
    }

    private static Activity StartParent()
    {
      _ = Listener;
      Activity parent = new("gh-telemetry-test");
      parent.SetIdFormat(ActivityIdFormat.W3C);
      parent.Start();
      return parent;
    }

    private static List<Activity> SpansOf(Activity parent) =>
      [.. Stopped.Where(span => span.TraceId == parent.TraceId)];

    private static void AssertNoTagContains(Activity span, string text)
    {
      foreach (KeyValuePair<string, object?> tag in span.TagObjects)
      {
        (tag.Value?.ToString() ?? string.Empty).ShouldNotContain(text);
      }

      (span.StatusDescription ?? string.Empty).ShouldNotContain(text);
      span.DisplayName.ShouldNotContain(text);
    }
  }
}
