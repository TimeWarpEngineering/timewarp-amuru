#!/usr/bin/env -S dotnet --

#region Purpose
// Argument lists for every Gh builder (task 152). No gh process runs.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Asserts ToArguments(), the list Build() hands to Shell.Run. ToCommandString is not used: a
// CliConfiguration override of "gh" elsewhere in the aggregate run would change the executable.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Gh_
{
  [TestTag("GhCommands")]
  public class Arguments_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Arguments_Given_>();

    public static async Task RunList_Should_EmitFiltersThenSharedFlags()
    {
      Gh.Run().List()
        .WithWorkflow("workflow.yml")
        .WithBranch("master")
        .WithStatus("success")
        .WithEvent("push")
        .WithCommit("abc123")
        .WithUser("octocat")
        .WithRepo("TimeWarpEngineering/timewarp-amuru")
        .WithJson("databaseId", "event")
        .WithJson("event", "headSha")
        .WithJq(".[0].databaseId")
        .WithLimit(5)
        .ToArguments()
        .ShouldBe(
        [
          "run", "list",
          "--workflow", "workflow.yml",
          "--branch", "master",
          "--status", "success",
          "--event", "push",
          "--commit", "abc123",
          "--user", "octocat",
          "--repo", "TimeWarpEngineering/timewarp-amuru",
          "--json", "databaseId,event,headSha",
          "--jq", ".[0].databaseId",
          "--limit", "5",
        ]);

      await Task.CompletedTask;
    }

    public static async Task RunListWithNothingSet_Should_BeTheCommandWordsOnly()
    {
      Gh.Run().List().ToArguments().ShouldBe(["run", "list"]);
      await Task.CompletedTask;
    }

    public static async Task RunDownload_Should_PutRunIdFirst()
    {
      Gh.Run().Download("123456789")
        .WithName("Packages-3.0.0")
        .WithName("Logs")
        .WithPattern("Packages-*")
        .WithDirectory("artifacts/packages")
        .WithRepo("o/r")
        .ToArguments()
        .ShouldBe(
        [
          "run", "download", "123456789",
          "--name", "Packages-3.0.0",
          "--name", "Logs",
          "--pattern", "Packages-*",
          "--dir", "artifacts/packages",
          "--repo", "o/r",
        ]);

      Gh.Run().Download().ToArguments().ShouldBe(["run", "download"]);
      await Task.CompletedTask;
    }

    public static async Task Api_Should_EmitEndpointMethodFieldsAndFlags()
    {
      Gh.Api("repos/{owner}/{repo}/dispatches")
        .WithMethod("POST")
        .WithHostname("github.com")
        .WithRawField("event_type", "rebuild")
        .WithField("client_payload[count]", "3")
        .WithHeader("Accept: application/vnd.github+json")
        .WithInput("body.json")
        .WithPaginate()
        .WithSlurp()
        .WithSilent()
        .WithInclude()
        .WithJq(".id")
        .ToArguments()
        .ShouldBe(
        [
          "api", "repos/{owner}/{repo}/dispatches",
          "--method", "POST",
          "--hostname", "github.com",
          "-f", "event_type=rebuild",
          "-F", "client_payload[count]=3",
          "-H", "Accept: application/vnd.github+json",
          "--input", "body.json",
          "--paginate",
          "--slurp",
          "--silent",
          "--include",
          "--jq", ".id",
        ]);

      await Task.CompletedTask;
    }

    public static async Task ApiWithBlankEndpoint_Should_Throw()
    {
      Should.Throw<ArgumentException>(() => Gh.Api(" "));
      await Task.CompletedTask;
    }

    public static async Task AuthToken_Should_EmitHostnameAndUser()
    {
      Gh.Auth().Token().ToArguments().ShouldBe(["auth", "token"]);
      Gh.Auth().Token()
        .WithHostname("github.example.com")
        .WithUser("octocat")
        .ToArguments()
        .ShouldBe(["auth", "token", "--hostname", "github.example.com", "--user", "octocat"]);

      await Task.CompletedTask;
    }

    public static async Task AuthStatus_Should_EmitHostnameAndActive()
    {
      Gh.Auth().Status()
        .WithHostname("github.com")
        .WithActive()
        .ToArguments()
        .ShouldBe(["auth", "status", "--hostname", "github.com", "--active"]);

      await Task.CompletedTask;
    }

    public static async Task IssueList_Should_EmitFiltersAndListFlags()
    {
      Gh.Issue().List()
        .WithState("open")
        .WithLabel("bug")
        .WithLabel("p1")
        .WithAssignee("@me")
        .WithAuthor("octocat")
        .WithSearch("sort:created-asc")
        .WithRepo("o/r")
        .WithJson("number", "title")
        .WithLimit(50)
        .ToArguments()
        .ShouldBe(
        [
          "issue", "list",
          "--state", "open",
          "--label", "bug",
          "--label", "p1",
          "--assignee", "@me",
          "--author", "octocat",
          "--search", "sort:created-asc",
          "--repo", "o/r",
          "--json", "number,title",
          "--limit", "50",
        ]);

      await Task.CompletedTask;
    }

    public static async Task IssueCreate_Should_EmitTitleBodyAndLists()
    {
      Gh.Issue().Create()
        .WithTitle("Flaky test")
        .WithBody("Seen twice")
        .WithBodyFile("body.md")
        .WithLabel("bug")
        .WithAssignee("@me")
        .WithMilestone("3.0")
        .WithRepo("o/r")
        .ToArguments()
        .ShouldBe(
        [
          "issue", "create",
          "--title", "Flaky test",
          "--body", "Seen twice",
          "--body-file", "body.md",
          "--label", "bug",
          "--assignee", "@me",
          "--milestone", "3.0",
          "--repo", "o/r",
        ]);

      await Task.CompletedTask;
    }

    public static async Task PrView_Should_EmitSelectorAndJson()
    {
      Gh.Pr().View("42")
        .WithComments()
        .WithRepo("o/r")
        .WithJson("state", "mergeStateStatus")
        .WithJq(".state")
        .ToArguments()
        .ShouldBe(["pr", "view", "42", "--comments", "--repo", "o/r", "--json", "state,mergeStateStatus", "--jq", ".state"]);

      Gh.Pr().View().ToArguments().ShouldBe(["pr", "view"]);
      await Task.CompletedTask;
    }

    public static async Task PrChecks_Should_EmitWatchFlags()
    {
      Gh.Pr().Checks("42")
        .WithRequired()
        .WithWatch()
        .WithFailFast()
        .WithInterval(30)
        .WithJson("name", "state")
        .ToArguments()
        .ShouldBe(["pr", "checks", "42", "--required", "--watch", "--fail-fast", "--interval", "30", "--json", "name,state"]);

      Gh.Pr().Checks().ToArguments().ShouldBe(["pr", "checks"]);
      await Task.CompletedTask;
    }

    public static async Task PrCreate_Should_EmitAllModeledFlags()
    {
      Gh.Pr().Create()
        .WithTitle("feat: gh builder")
        .WithBody("Body")
        .WithBodyFile("pr.md")
        .WithBase("master")
        .WithHead("task/152")
        .WithDraft()
        .WithFill()
        .WithLabel("enhancement")
        .WithReviewer("octocat")
        .WithAssignee("@me")
        .WithRepo("o/r")
        .ToArguments()
        .ShouldBe(
        [
          "pr", "create",
          "--title", "feat: gh builder",
          "--body", "Body",
          "--body-file", "pr.md",
          "--base", "master",
          "--head", "task/152",
          "--draft",
          "--fill",
          "--label", "enhancement",
          "--reviewer", "octocat",
          "--assignee", "@me",
          "--repo", "o/r",
        ]);

      await Task.CompletedTask;
    }

    public static async Task PrMerge_Should_KeepOnlyTheLastStrategy()
    {
      Gh.Pr().Merge("42")
        .WithSquash()
        .WithMergeCommit()
        .WithAuto()
        .WithDeleteBranch()
        .WithAdmin()
        .WithSubject("Merge #42")
        .WithBody("Body")
        .WithMatchHeadCommit("def456")
        .WithRepo("o/r")
        .ToArguments()
        .ShouldBe(
        [
          "pr", "merge", "42",
          "--merge",
          "--auto",
          "--delete-branch",
          "--admin",
          "--subject", "Merge #42",
          "--body", "Body",
          "--match-head-commit", "def456",
          "--repo", "o/r",
        ]);

      Gh.Pr().Merge().WithRebase().ToArguments().ShouldBe(["pr", "merge", "--rebase"]);
      await Task.CompletedTask;
    }

    public static async Task ReleaseList_Should_EmitExcludesAndListFlags()
    {
      Gh.Release().List()
        .WithExcludeDrafts()
        .WithExcludePreReleases()
        .WithRepo("o/r")
        .WithJson("tagName", "isLatest")
        .WithLimit(3)
        .ToArguments()
        .ShouldBe(["release", "list", "--exclude-drafts", "--exclude-pre-releases", "--repo", "o/r", "--json", "tagName,isLatest", "--limit", "3"]);

      await Task.CompletedTask;
    }

    public static async Task ReleaseCreate_Should_PutTagAndAssetsFirst()
    {
      Gh.Release().Create("v3.0.0")
        .WithAsset("a.nupkg")
        .WithAsset("b.nupkg#Tools")
        .WithTitle("3.0.0")
        .WithNotes("Notes")
        .WithNotesFile("notes.md")
        .WithGenerateNotes()
        .WithTarget("master")
        .WithDraft()
        .WithPrerelease()
        .WithVerifyTag()
        .WithLatest(false)
        .WithRepo("o/r")
        .ToArguments()
        .ShouldBe(
        [
          "release", "create", "v3.0.0", "a.nupkg", "b.nupkg#Tools",
          "--title", "3.0.0",
          "--notes", "Notes",
          "--notes-file", "notes.md",
          "--generate-notes",
          "--target", "master",
          "--draft",
          "--prerelease",
          "--verify-tag",
          "--latest=false",
          "--repo", "o/r",
        ]);

      Gh.Release().Create("v1").WithLatest(true).ToArguments().ShouldBe(["release", "create", "v1", "--latest"]);
      await Task.CompletedTask;
    }

    public static async Task WithArgument_Should_AppendAfterModeledArguments()
    {
      Gh.Run().List()
        .WithArgument("--all")
        .WithLimit(1)
        .WithArguments("--created", ">2026-11-01")
        .ToArguments()
        .ShouldBe(["run", "list", "--limit", "1", "--all", "--created", ">2026-11-01"]);

      await Task.CompletedTask;
    }

    public static async Task WithLimitOfZero_Should_Throw()
    {
      Should.Throw<ArgumentOutOfRangeException>(() => Gh.Issue().List().WithLimit(0));
      await Task.CompletedTask;
    }

    public static async Task Command_Should_BeTheCommandWords()
    {
      Gh.Run().List().Command.ShouldBe("run list");
      Gh.Api("user").Command.ShouldBe("api");
      Gh.Auth().Token().Command.ShouldBe("auth token");
      Gh.Release().Create("v1").Command.ShouldBe("release create");
      await Task.CompletedTask;
    }
  }
}
