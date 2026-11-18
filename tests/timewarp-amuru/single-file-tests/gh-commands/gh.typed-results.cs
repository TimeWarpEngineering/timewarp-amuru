#!/usr/bin/env -S dotnet --

#region Purpose
// Typed results for gh run list, gh pr view, and gh auth status (task 152). gh runs under
// CommandMock with recorded JSON shapes; no GitHub call.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The mock setups spell out the exact argument list, so a test also proves the typed method
// replaced earlier --json fields and --jq with the record's JsonFields.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Gh_
{
  [TestTag("GhCommands")]
  public class TypedResults_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<TypedResults_Given_>();

    private const string RunsJson =
      """
      [{"attempt":1,"conclusion":"success","createdAt":"2026-11-18T10:00:00Z","databaseId":123456789,
        "displayTitle":"feat: gh builder","event":"push","headBranch":"master","headSha":"abc123","name":"CI",
        "number":42,"startedAt":"2026-11-18T10:00:05Z","status":"completed","updatedAt":"2026-11-18T10:05:00Z",
        "url":"https://github.com/o/r/actions/runs/123456789","workflowDatabaseId":987,"workflowName":"CI"},
       {"attempt":2,"conclusion":"","createdAt":"2026-11-18T11:00:00Z","databaseId":123456790,
        "displayTitle":"wip","event":"workflow_dispatch","headBranch":"task/152","headSha":"def456","name":"CI",
        "number":43,"startedAt":"2026-11-18T11:00:05Z","status":"in_progress","updatedAt":"2026-11-18T11:01:00Z",
        "url":"https://github.com/o/r/actions/runs/123456790","workflowDatabaseId":987,"workflowName":"CI","newField":1}]
      """;

    private const string PullRequestJson =
      """
      {"author":{"id":"MDQ","is_bot":false,"login":"octocat","name":"Mona"},"baseRefName":"master",
       "createdAt":"2026-11-18T10:00:00Z","headRefName":"task/152","headRefOid":"def456","isDraft":false,
       "mergeStateStatus":"CLEAN","mergeable":"MERGEABLE","mergedAt":null,"number":42,"reviewDecision":"",
       "state":"OPEN","title":"feat: gh builder","url":"https://github.com/o/r/pull/42"}
      """;

    private const string SignedInJson =
      """
      {"hosts":{"github.com":[
        {"state":"success","active":true,"host":"github.com","login":"octocat","tokenSource":"keyring","scopes":"repo","gitProtocol":"https"},
        {"state":"success","active":false,"host":"github.com","login":"other","tokenSource":"keyring","scopes":"repo","gitProtocol":"https"}]}}
      """;

    private const string FailedJson =
      """
      {"hosts":{"github.com":[{"state":"error","active":true,"host":"github.com","login":"octocat","tokenSource":"GH_TOKEN","error":"bad credentials"}]}}
      """;

    public static async Task ListRunsAsync_Should_ParseTheRuns()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "run", "list", "--workflow", "workflow.yml", "--repo", "o/r", "--json", string.Join(',', GhWorkflowRun.JsonFields), "--limit", "2")
        .Returns(RunsJson);

      GhRunListResult result = await Gh.Run().List()
        .WithWorkflow("workflow.yml")
        .WithRepo("o/r")
        .WithJson("name")
        .WithJq(".[].name")
        .WithLimit(2)
        .ListRunsAsync();

      result.Success.ShouldBeTrue();
      result.Runs.Count.ShouldBe(2);
      GhWorkflowRun first = result.Runs[0];
      first.DatabaseId.ShouldBe(123456789L);
      first.Number.ShouldBe(42);
      first.Attempt.ShouldBe(1);
      first.Conclusion.ShouldBe("success");
      first.Status.ShouldBe("completed");
      first.Event.ShouldBe("push");
      first.HeadBranch.ShouldBe("master");
      first.HeadSha.ShouldBe("abc123");
      first.WorkflowName.ShouldBe("CI");
      first.WorkflowDatabaseId.ShouldBe(987L);
      first.DisplayTitle.ShouldBe("feat: gh builder");
      first.Url.ShouldBe(new Uri("https://github.com/o/r/actions/runs/123456789"));
      first.CreatedAt.ShouldBe(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero));
      result.Runs[1].Conclusion.ShouldBe(string.Empty);
      result.Runs[1].Status.ShouldBe("in_progress");
    }

    public static async Task ListRunsAsyncOnFailure_Should_ReturnNoRuns()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "run", "list", "--json", string.Join(',', GhWorkflowRun.JsonFields))
        .ReturnsError("HTTP 404: Not Found", exitCode: 1);

      GhRunListResult result = await Gh.Run().List().ListRunsAsync();

      result.Success.ShouldBeFalse();
      result.Runs.ShouldBeEmpty();
      result.Output.Stderr.ShouldContain("404");
    }

    public static async Task ViewPullRequestAsync_Should_ParseThePullRequest()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "pr", "view", "42", "--repo", "o/r", "--json", string.Join(',', GhPullRequest.JsonFields))
        .Returns(PullRequestJson);

      GhPullRequestViewResult result = await Gh.Pr().View("42").WithRepo("o/r").WithJq(".state").ViewPullRequestAsync();

      result.Success.ShouldBeTrue();
      GhPullRequest pr = result.PullRequest.ShouldNotBeNull();
      pr.Number.ShouldBe(42);
      pr.Title.ShouldBe("feat: gh builder");
      pr.State.ShouldBe("OPEN");
      pr.IsDraft.ShouldBeFalse();
      pr.HeadRefName.ShouldBe("task/152");
      pr.HeadRefOid.ShouldBe("def456");
      pr.BaseRefName.ShouldBe("master");
      pr.Mergeable.ShouldBe("MERGEABLE");
      pr.MergeStateStatus.ShouldBe("CLEAN");
      pr.ReviewDecision.ShouldBe(string.Empty);
      pr.MergedAt.ShouldBeNull();
      pr.Url.ShouldBe(new Uri("https://github.com/o/r/pull/42"));
      pr.Author.ShouldNotBeNull().Login.ShouldBe("octocat");
    }

    public static async Task ViewPullRequestAsyncWithNoPullRequest_Should_ReturnFailure()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "pr", "view", "--json", string.Join(',', GhPullRequest.JsonFields))
        .ReturnsError("no pull requests found for branch \"master\"", exitCode: 1);

      GhPullRequestViewResult result = await Gh.Pr().View().ViewPullRequestAsync();

      result.Success.ShouldBeFalse();
      result.PullRequest.ShouldBeNull();
    }

    public static async Task GetStatusAsyncWhenSignedIn_Should_ReportHostAndLogin()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "status", "--json", "hosts").Returns(SignedInJson);

      GhAuthStatus status = await Gh.Auth().Status().GetStatusAsync();

      status.SignedIn.ShouldBeTrue();
      status.Host.ShouldBe("github.com");
      status.Login.ShouldBe("octocat");
    }

    public static async Task GetStatusAsyncWithBadCredentials_Should_ReportSignedOut()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "status", "--hostname", "github.com", "--json", "hosts").Returns(FailedJson);

      GhAuthStatus status = await Gh.Auth().Status().WithHostname("github.com").GetStatusAsync();

      status.SignedIn.ShouldBeFalse();
      status.Host.ShouldBe("github.com");
      status.Login.ShouldBe("octocat");
    }

    public static async Task GetStatusAsyncWithUnknownHost_Should_ReportSignedOut()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "status", "--hostname", "ghe.example.com", "--json", "hosts").Returns("""{"hosts":{}}""");

      GhAuthStatus status = await Gh.Auth().Status().WithHostname("ghe.example.com").GetStatusAsync();

      status.SignedIn.ShouldBeFalse();
      status.Host.ShouldBe("ghe.example.com");
      status.Login.ShouldBeNull();
    }

    public static async Task GetStatusAsyncWithNoHosts_Should_ReportSignedOutWithNoHost()
    {
      using IDisposable mock = CommandMock.Enable();
      CommandMock.Setup("gh", "auth", "status", "--json", "hosts").Returns("""{"hosts":{}}""");

      GhAuthStatus status = await Gh.Auth().Status().GetStatusAsync();

      status.SignedIn.ShouldBeFalse();
      status.Host.ShouldBeNull();
    }
  }
}
