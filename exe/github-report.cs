#!/usr/bin/dotnet --

#:property PublishAot=false
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

#pragma warning disable CA1303, CA1304, CA1305, CA1307, CA1311, CA1812, CA1852, CA1854, IDE0007, IL2026, IL3050

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TimeWarp.Amuru;

// Parse command line arguments
int daysBack = args.Length > 0 && int.TryParse(args[0], out int days) ? days : 30;
string outputDir = args.Length > 1 ? args[1] : ".";
string startDate = DateTime.Now.AddDays(-daysBack).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

Console.WriteLine($"📊 Generating GitHub Activity Report");
Console.WriteLine($"📅 Date Range: {startDate} to {DateTime.Now:yyyy-MM-dd} ({daysBack} days)");
Console.WriteLine($"📁 Output Directory: {outputDir}");
Console.WriteLine();

// Fetch data from GitHub
Console.WriteLine("🔍 Fetching commits...");
CommandOutput commitsResult = await Shell.Builder("gh")
    .WithArguments("search", "commits", $"--author=@me", $"--author-date=>={startDate}", "--json", "repository,commit,author", "--limit", "1000")
    .CaptureAsync();

Console.WriteLine("🔍 Fetching pull requests...");
CommandOutput prsResult = await Shell.Builder("gh")
    .WithArguments("search", "prs", $"--author=@me", $"--created=>={startDate}", "--json", "repository,title,number,state,createdAt,closedAt", "--limit", "100")
    .CaptureAsync();

Console.WriteLine("🔍 Fetching issues...");
CommandOutput issuesResult = await Shell.Builder("gh")
    .WithArguments("search", "issues", $"--author=@me", $"--created=>={startDate}", "--json", "repository,title,number,state,createdAt,closedAt", "--limit", "100")
    .CaptureAsync();

if (!commitsResult.Success || !prsResult.Success || !issuesResult.Success)
{
    Console.WriteLine("❌ Failed to fetch GitHub data");
    return 1;
}

// Parse JSON data
List<CommitSearchResult> commits = JsonSerializer.Deserialize<List<CommitSearchResult>>(commitsResult.Stdout) ?? [];
List<PullRequest> prs = JsonSerializer.Deserialize<List<PullRequest>>(prsResult.Stdout) ?? [];
List<Issue> issues = JsonSerializer.Deserialize<List<Issue>>(issuesResult.Stdout) ?? [];

Console.WriteLine($"✅ Found {commits.Count} commits, {prs.Count} PRs, {issues.Count} issues");
Console.WriteLine();

// Build timeline events
List<TimelineEvent> timeline = BuildTimeline(commits, prs, issues);

// Group by repository
Dictionary<string, RepositoryActivity> repoActivity = GroupByRepository(commits, prs, issues);

// Ensure output directory exists
if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
}

// Generate reports
Console.WriteLine("📝 Generating HTML report...");
string htmlPath = Path.Combine(outputDir, "github-activity-report.html");
await File.WriteAllTextAsync(htmlPath, GenerateHtmlReport(timeline, repoActivity, startDate, daysBack));
Console.WriteLine($"✅ HTML report: {htmlPath}");

Console.WriteLine("📝 Generating Markdown report...");
string mdPath = Path.Combine(outputDir, "GITHUB_ACTIVITY_REPORT.md");
await File.WriteAllTextAsync(mdPath, GenerateMarkdownReport(timeline, repoActivity, startDate, daysBack));
Console.WriteLine($"✅ Markdown report: {mdPath}");

Console.WriteLine();
Console.WriteLine("🎉 Reports generated successfully!");

return 0;

// Helper methods

static List<TimelineEvent> BuildTimeline(List<CommitSearchResult> commits, List<PullRequest> prs, List<Issue> issues)
{
    List<TimelineEvent> events = [];

    // Add PR events
    foreach (PullRequest pr in prs)
    {
        events.Add(new TimelineEvent
        {
            Timestamp = pr.CreatedAt,
            Type = "PR_OPENED",
            Repository = pr.Repository.NameWithOwner,
            Title = pr.Title,
            Number = pr.Number,
            State = pr.State,
            Url = $"https://github.com/{pr.Repository.NameWithOwner}/pull/{pr.Number}"
        });

        if (pr.ClosedAt > DateTime.MinValue)
        {
            events.Add(new TimelineEvent
            {
                Timestamp = pr.ClosedAt,
                Type = pr.State == "merged" ? "PR_MERGED" : "PR_CLOSED",
                Repository = pr.Repository.NameWithOwner,
                Title = pr.Title,
                Number = pr.Number,
                State = pr.State,
                Url = $"https://github.com/{pr.Repository.NameWithOwner}/pull/{pr.Number}"
            });
        }
    }

    // Add issue events
    foreach (Issue issue in issues)
    {
        events.Add(new TimelineEvent
        {
            Timestamp = issue.CreatedAt,
            Type = "ISSUE_OPENED",
            Repository = issue.Repository.NameWithOwner,
            Title = issue.Title,
            Number = issue.Number,
            State = issue.State,
            Url = $"https://github.com/{issue.Repository.NameWithOwner}/issues/{issue.Number}"
        });

        if (issue.ClosedAt > DateTime.MinValue)
        {
            events.Add(new TimelineEvent
            {
                Timestamp = issue.ClosedAt,
                Type = "ISSUE_CLOSED",
                Repository = issue.Repository.NameWithOwner,
                Title = issue.Title,
                Number = issue.Number,
                State = issue.State,
                Url = $"https://github.com/{issue.Repository.NameWithOwner}/issues/{issue.Number}"
            });
        }
    }

    // Add commit events (sample, not all)
    IEnumerable<CommitSearchResult> recentCommits = commits
        .OrderByDescending(c => c.Commit.Author.Date)
        .Take(50);

    foreach (CommitSearchResult commit in recentCommits)
    {
        events.Add(new TimelineEvent
        {
            Timestamp = commit.Commit.Author.Date,
            Type = "COMMIT",
            Repository = commit.Repository.FullName,
            Title = commit.Commit.Message.Split('\n')[0],
            Url = $"https://github.com/{commit.Repository.FullName}/commit/{commit.Commit.Tree.Sha}"
        });
    }

    return [.. events.OrderByDescending(e => e.Timestamp)];
}

static Dictionary<string, RepositoryActivity> GroupByRepository(
    List<CommitSearchResult> commits,
    List<PullRequest> prs,
    List<Issue> issues)
{
    Dictionary<string, RepositoryActivity> repoMap = [];

    // Group PRs
    foreach (PullRequest pr in prs)
    {
        string repoName = pr.Repository.NameWithOwner;
        if (!repoMap.ContainsKey(repoName))
        {
            repoMap[repoName] = new RepositoryActivity { Name = repoName };
        }

        repoMap[repoName].PullRequests.Add(pr);
    }

    // Group Issues
    foreach (Issue issue in issues)
    {
        string repoName = issue.Repository.NameWithOwner;
        if (!repoMap.ContainsKey(repoName))
        {
            repoMap[repoName] = new RepositoryActivity { Name = repoName };
        }

        repoMap[repoName].Issues.Add(issue);
    }

    // Count commits by repo
    foreach (CommitSearchResult commit in commits)
    {
        string repoName = commit.Repository.FullName;
        if (!repoMap.ContainsKey(repoName))
        {
            repoMap[repoName] = new RepositoryActivity { Name = repoName };
        }

        repoMap[repoName].CommitCount++;
    }

    return repoMap.OrderByDescending(kvp =>
        kvp.Value.PullRequests.Count + kvp.Value.Issues.Count + kvp.Value.CommitCount)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

static string GenerateHtmlReport(List<TimelineEvent> timeline, Dictionary<string, RepositoryActivity> repoActivity, string startDate, int daysBack)
{
    StringBuilder sb = new();

    sb.AppendLine($$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>GitHub Activity Report - {{startDate}} to {{DateTime.Now:yyyy-MM-dd}}</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.js"></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 2rem;
        }
        .container {
            max-width: 1600px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 3rem 2rem;
            text-align: center;
        }
        .header h1 { font-size: 2.5rem; margin-bottom: 0.5rem; }
        .header p { font-size: 1.1rem; opacity: 0.9; }
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1.5rem;
            padding: 2rem;
            background: #f8f9fa;
        }
        .stat-card {
            background: white;
            padding: 1.5rem;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            text-align: center;
            transition: transform 0.2s;
        }
        .stat-card:hover { transform: translateY(-5px); box-shadow: 0 5px 20px rgba(0,0,0,0.15); }
        .stat-number { font-size: 3rem; font-weight: bold; color: #667eea; display: block; margin-bottom: 0.5rem; }
        .stat-label { color: #6c757d; font-size: 0.9rem; text-transform: uppercase; letter-spacing: 1px; }
        .content { padding: 2rem; }
        .section { margin-bottom: 3rem; }
        .section h2 {
            color: #333;
            margin-bottom: 1.5rem;
            padding-bottom: 0.5rem;
            border-bottom: 3px solid #667eea;
            font-size: 1.8rem;
        }
        .timeline-container {
            position: relative;
            max-height: 800px;
            overflow-y: auto;
            background: white;
            padding: 2rem;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .timeline { position: relative; padding-left: 3rem; }
        .timeline::before {
            content: '';
            position: absolute;
            left: 20px;
            top: 0;
            bottom: 0;
            width: 4px;
            background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
        }
        .timeline-event {
            position: relative;
            margin-bottom: 2rem;
            padding: 1.5rem;
            background: #f8f9fa;
            border-radius: 10px;
            border-left: 4px solid #667eea;
            transition: all 0.3s;
        }
        .timeline-event:hover { transform: translateX(5px); box-shadow: 0 5px 20px rgba(0,0,0,0.1); }
        .timeline-event::before {
            content: '';
            position: absolute;
            left: -3.8rem;
            top: 1.5rem;
            width: 16px;
            height: 16px;
            border-radius: 50%;
            background: white;
            border: 4px solid #667eea;
            box-shadow: 0 0 0 4px white;
            z-index: 1;
        }
        .timeline-event.commit::before { border-color: #667eea; }
        .timeline-event.pr-opened::before { border-color: #28a745; }
        .timeline-event.pr-merged::before { border-color: #6f42c1; }
        .timeline-event.pr-closed::before { border-color: #dc3545; }
        .timeline-event.issue-opened::before { border-color: #ffc107; }
        .timeline-event.issue-closed::before { border-color: #17a2b8; }
        .event-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 0.5rem; flex-wrap: wrap; }
        .event-type {
            padding: 0.25rem 0.75rem;
            border-radius: 12px;
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
        }
        .event-type.commit { background: #667eea; color: white; }
        .event-type.pr-opened { background: #28a745; color: white; }
        .event-type.pr-merged { background: #6f42c1; color: white; }
        .event-type.pr-closed { background: #dc3545; color: white; }
        .event-type.issue-opened { background: #ffc107; color: #333; }
        .event-type.issue-closed { background: #17a2b8; color: white; }
        .event-time { color: #6c757d; font-size: 0.9rem; }
        .event-repo { color: #667eea; font-weight: 600; font-size: 0.9rem; }
        .event-title { font-size: 1.1rem; color: #333; margin: 0.5rem 0; }
        .event-title a { color: inherit; text-decoration: none; }
        .event-title a:hover { color: #667eea; text-decoration: underline; }
        .repo-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 1.5rem; }
        .repo-card {
            background: white;
            border: 1px solid #e0e0e0;
            border-radius: 10px;
            padding: 1.5rem;
            transition: all 0.3s;
        }
        .repo-card:hover { box-shadow: 0 5px 20px rgba(0,0,0,0.15); border-color: #667eea; }
        .repo-name { font-size: 1.3rem; font-weight: bold; color: #667eea; margin-bottom: 1rem; }
        .repo-stats { display: flex; gap: 1rem; margin: 1rem 0; flex-wrap: wrap; }
        .repo-stat { background: #f8f9fa; padding: 0.5rem 1rem; border-radius: 20px; font-size: 0.9rem; }
        .repo-stat strong { color: #667eea; }
        .chart-container {
            position: relative;
            height: 400px;
            margin-bottom: 2rem;
            background: white;
            padding: 1rem;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .stack-viz {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 2rem;
            border-radius: 10px;
            margin-bottom: 2rem;
        }
        .stack-viz h3 { margin-bottom: 1rem; }
        .stack-item {
            background: rgba(255,255,255,0.1);
            padding: 1rem;
            margin-bottom: 0.5rem;
            border-radius: 5px;
            backdrop-filter: blur(10px);
            border-left: 4px solid white;
        }
        .filters { display: flex; gap: 1rem; margin-bottom: 2rem; flex-wrap: wrap; }
        .filter-btn {
            padding: 0.75rem 1.5rem;
            border: 2px solid #667eea;
            background: white;
            color: #667eea;
            border-radius: 25px;
            cursor: pointer;
            transition: all 0.3s;
            font-weight: 600;
        }
        .filter-btn:hover, .filter-btn.active { background: #667eea; color: white; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>🚀 GitHub Activity Report</h1>
            <p>{{startDate}} to {{DateTime.Now:yyyy-MM-dd}} ({{daysBack}} days)</p>
        </div>
        <div class="stats-grid">
            <div class="stat-card">
                <span class="stat-number">{{repoActivity.Sum(r => r.Value.CommitCount)}}</span>
                <span class="stat-label">Commits</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{{repoActivity.Sum(r => r.Value.PullRequests.Count)}}</span>
                <span class="stat-label">Pull Requests</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{{repoActivity.Sum(r => r.Value.Issues.Count)}}</span>
                <span class="stat-label">Issues</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{{repoActivity.Count}}</span>
                <span class="stat-label">Repositories</span>
            </div>
            <div class="stat-card">
                <span class="stat-number">{{repoActivity.Sum(r => r.Value.PullRequests.Count(pr => pr.State == "merged"))}}</span>
                <span class="stat-label">PRs Merged</span>
            </div>
        </div>
        <div class="content">
            <div class="section">
                <h2>📈 Activity Stack Visualization</h2>
                <div class="stack-viz">
                    <h3>🔄 Context Switch Analysis</h3>
                    <p style="margin-bottom: 1rem;">Your activity shows these work contexts (most recent first):</p>
""");

    // Analyze context switches
    List<ContextSwitch> contextSwitches = AnalyzeContextSwitches(timeline);
    foreach (ContextSwitch context in contextSwitches.Take(10))
    {
        sb.AppendLine($@"                    <div class=""stack-item"">
                        <strong>{context.Repository}</strong> → {context.ItemsCount} items
                        <br><small>Last active: {context.LastActivity:MMM dd, HH:mm} | Types: {string.Join(", ", context.EventTypes.Distinct().Take(3))}</small>
                    </div>");
    }

    sb.AppendLine("""
                </div>
            </div>
            <div class="section">
                <h2>⏱️ Activity Timeline</h2>
                <div class="filters">
                    <button class="filter-btn active" onclick="filterTimeline('all')">All Events</button>
                    <button class="filter-btn" onclick="filterTimeline('pr')">Pull Requests</button>
                    <button class="filter-btn" onclick="filterTimeline('issue')">Issues</button>
                    <button class="filter-btn" onclick="filterTimeline('commit')">Commits</button>
                </div>
                <div class="timeline-container">
                    <div class="timeline" id="timeline">
""");

    // Generate timeline events
    foreach (TimelineEvent evt in timeline)
    {
        string typeClass = evt.Type.ToLower().Replace("_", "-");
        string typeDisplay = evt.Type.Replace("_", " ");

        sb.AppendLine($@"                        <div class=""timeline-event {typeClass}"" data-type=""{typeClass}"">
                            <div class=""event-header"">
                                <span class=""event-type {typeClass}"">{typeDisplay}</span>
                                <span class=""event-time"">{evt.Timestamp:MMM dd, yyyy HH:mm}</span>
                                <span class=""event-repo"">{evt.Repository}</span>
                            </div>
                            <div class=""event-title"">
                                <a href=""{evt.Url}"" target=""_blank"">{System.Net.WebUtility.HtmlEncode(evt.Title)}</a>
                            </div>
                        </div>");
    }

    sb.AppendLine("""
                    </div>
                </div>
            </div>
            <div class="section">
                <h2>📊 Repository Activity</h2>
                <div class="chart-container">
                    <canvas id="repoActivityChart"></canvas>
                </div>
                <div class="repo-grid">
""");

    // Generate repository cards
    foreach (KeyValuePair<string, RepositoryActivity> repo in repoActivity.Take(20))
    {
        int prCount = repo.Value.PullRequests.Count;
        int issueCount = repo.Value.Issues.Count;
        int commitCount = repo.Value.CommitCount;
        int mergedCount = repo.Value.PullRequests.Count(pr => pr.State == "merged");

        sb.AppendLine($@"                    <div class=""repo-card"">
                        <div class=""repo-name"">{repo.Key}</div>
                        <div class=""repo-stats"">
                            <div class=""repo-stat""><strong>{commitCount}</strong> Commits</div>
                            <div class=""repo-stat""><strong>{prCount}</strong> PRs</div>
                            <div class=""repo-stat""><strong>{issueCount}</strong> Issues</div>
                            <div class=""repo-stat""><strong>{mergedCount}</strong> Merged</div>
                        </div>
                    </div>");
    }

    sb.AppendLine("""
                </div>
            </div>
        </div>
    </div>
    <script>
        const repoCtx = document.getElementById('repoActivityChart').getContext('2d');
        const repoData = {
""");

    List<KeyValuePair<string, RepositoryActivity>> topRepos = repoActivity.Take(15).ToList();
    sb.AppendLine($"            labels: {JsonSerializer.Serialize(topRepos.Select(r => r.Key.Split('/').Last()))},");
    sb.AppendLine($"            commits: {JsonSerializer.Serialize(topRepos.Select(r => r.Value.CommitCount))},");
    sb.AppendLine($"            prs: {JsonSerializer.Serialize(topRepos.Select(r => r.Value.PullRequests.Count))},");
    sb.AppendLine($"            issues: {JsonSerializer.Serialize(topRepos.Select(r => r.Value.Issues.Count))}");

    sb.AppendLine("""
        };
        new Chart(repoCtx, {
            type: 'bar',
            data: {
                labels: repoData.labels,
                datasets: [{
                    label: 'Commits',
                    data: repoData.commits,
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                }, {
                    label: 'Pull Requests',
                    data: repoData.prs,
                    backgroundColor: 'rgba(40, 167, 69, 0.8)',
                }, {
                    label: 'Issues',
                    data: repoData.issues,
                    backgroundColor: 'rgba(255, 193, 7, 0.8)',
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'top' },
                    title: { display: true, text: 'Activity by Repository', font: { size: 18 } }
                },
                scales: {
                    x: { stacked: true },
                    y: { stacked: true, beginAtZero: true }
                }
            }
        });
        function filterTimeline(type) {
            const events = document.querySelectorAll('.timeline-event');
            const buttons = document.querySelectorAll('.filter-btn');
            buttons.forEach(btn => btn.classList.remove('active'));
            event.target.classList.add('active');
            events.forEach(evt => {
                evt.style.display = (type === 'all' || evt.dataset.type.includes(type)) ? 'block' : 'none';
            });
        }
    </script>
</body>
</html>
""");

    return sb.ToString();
}

static string GenerateMarkdownReport(List<TimelineEvent> timeline, Dictionary<string, RepositoryActivity> repoActivity, string startDate, int daysBack)
{
    StringBuilder sb = new();
    int totalCommits = repoActivity.Sum(r => r.Value.CommitCount);
    int totalPRs = repoActivity.Sum(r => r.Value.PullRequests.Count);
    int totalIssues = repoActivity.Sum(r => r.Value.Issues.Count);
    int mergedPRs = repoActivity.Sum(r => r.Value.PullRequests.Count(pr => pr.State == "merged"));
    double mergeRate = totalPRs > 0 ? (mergedPRs * 100.0 / totalPRs) : 0;

    sb.AppendLine("# 🚀 GitHub Activity Report");
    sb.AppendLine();
    sb.AppendLine($"**Period:** {startDate} to {DateTime.Now:yyyy-MM-dd} ({daysBack} days)");
    sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine();
    sb.AppendLine("---");
    sb.AppendLine();
    sb.AppendLine("## 📊 Summary");
    sb.AppendLine();
    sb.AppendLine("| Metric | Count |");
    sb.AppendLine("|--------|-------|");
    sb.AppendLine($"| **Commits** | {totalCommits} |");
    sb.AppendLine($"| **Pull Requests** | {totalPRs} |");
    sb.AppendLine($"| **PRs Merged** | {mergedPRs} |");
    sb.AppendLine($"| **PR Merge Rate** | {mergeRate:F1}% |");
    sb.AppendLine($"| **Issues Created** | {totalIssues} |");
    sb.AppendLine($"| **Repositories** | {repoActivity.Count} |");
    sb.AppendLine();
    sb.AppendLine("---");
    sb.AppendLine();

    // Context switch analysis
    sb.AppendLine("## 🔄 Context Switch Analysis");
    sb.AppendLine();
    sb.AppendLine("Your activity shows these primary work contexts (stack visualization):");
    sb.AppendLine();

    List<ContextSwitch> contextSwitches = AnalyzeContextSwitches(timeline);
    foreach (ContextSwitch context in contextSwitches.Take(10))
    {
        sb.AppendLine($"### {context.Repository}");
        sb.AppendLine($"- **Items:** {context.ItemsCount}");
        sb.AppendLine($"- **Last Active:** {context.LastActivity:MMM dd, yyyy HH:mm}");
        sb.AppendLine($"- **Event Types:** {string.Join(", ", context.EventTypes.Distinct())}");
        sb.AppendLine();
    }

    sb.AppendLine("---");
    sb.AppendLine();

    // Timeline
    sb.AppendLine("## ⏱️ Activity Timeline");
    sb.AppendLine();

    string currentDate = "";
    foreach (TimelineEvent evt in timeline)
    {
        string eventDate = evt.Timestamp.ToString("yyyy-MM-dd");
        if (eventDate != currentDate)
        {
            currentDate = eventDate;
            sb.AppendLine();
            sb.AppendLine($"### {evt.Timestamp:MMMM dd, yyyy}");
            sb.AppendLine();
        }

        string icon = evt.Type switch
        {
            "COMMIT" => "💾",
            "PR_OPENED" => "🔀",
            "PR_MERGED" => "✅",
            "PR_CLOSED" => "❌",
            "ISSUE_OPENED" => "🐛",
            "ISSUE_CLOSED" => "✔️",
            _ => "📝"
        };

        sb.AppendLine($"- **{evt.Timestamp:HH:mm}** {icon} `{evt.Type}` [{evt.Repository}]({evt.Url}) - {evt.Title}");
    }

    sb.AppendLine();
    sb.AppendLine("---");
    sb.AppendLine();

    // Repository activity
    sb.AppendLine("## 📦 Repository Activity");
    sb.AppendLine();

    foreach (KeyValuePair<string, RepositoryActivity> repo in repoActivity)
    {
        sb.AppendLine($"### {repo.Key}");
        sb.AppendLine();
        sb.AppendLine($"- **Commits:** {repo.Value.CommitCount}");
        sb.AppendLine($"- **Pull Requests:** {repo.Value.PullRequests.Count} ({repo.Value.PullRequests.Count(pr => pr.State == "merged")} merged)");
        sb.AppendLine($"- **Issues:** {repo.Value.Issues.Count}");
        sb.AppendLine();

        if (repo.Value.PullRequests.Count != 0)
        {
            sb.AppendLine("#### Pull Requests");
            foreach (PullRequest pr in repo.Value.PullRequests.Take(10))
            {
                string prIcon = pr.State == "merged" ? "✅" : pr.State == "open" ? "🔀" : "❌";
                sb.AppendLine($"- {prIcon} [#{pr.Number}](https://github.com/{repo.Key}/pull/{pr.Number}) {pr.Title}");
            }

            sb.AppendLine();
        }

        if (repo.Value.Issues.Count != 0)
        {
            sb.AppendLine("#### Issues");
            foreach (Issue issue in repo.Value.Issues.Take(10))
            {
                string issueIcon = issue.State == "closed" ? "✔️" : "🐛";
                sb.AppendLine($"- {issueIcon} [#{issue.Number}](https://github.com/{repo.Key}/issues/{issue.Number}) {issue.Title}");
            }

            sb.AppendLine();
        }
    }

    sb.AppendLine("---");
    sb.AppendLine();
    sb.AppendLine($"*Report generated by github-report.cs on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

    return sb.ToString();
}

static List<ContextSwitch> AnalyzeContextSwitches(List<TimelineEvent> timeline)
{
    Dictionary<string, ContextSwitch> contexts = [];

    foreach (TimelineEvent evt in timeline)
    {
        if (!contexts.ContainsKey(evt.Repository))
        {
            contexts[evt.Repository] = new ContextSwitch
            {
                Repository = evt.Repository,
                LastActivity = evt.Timestamp
            };
        }

        ContextSwitch context = contexts[evt.Repository];
        context.ItemsCount++;
        context.EventTypes.Add(evt.Type);
        if (evt.Timestamp > context.LastActivity)
        {
            context.LastActivity = evt.Timestamp;
        }
    }

    return [.. contexts.Values.OrderByDescending(c => c.LastActivity)];
}

// Data models

sealed class CommitSearchResult
{
    [JsonPropertyName("repository")]
    public Repository Repository { get; set; } = new();

    [JsonPropertyName("commit")]
    public Commit Commit { get; set; } = new();

    [JsonPropertyName("author")]
    public Author Author { get; set; } = new();
}

sealed class Repository
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("nameWithOwner")]
    public string NameWithOwner { get; set; } = "";
}

sealed class Commit
{
    [JsonPropertyName("author")]
    public CommitAuthor Author { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("tree")]
    public Tree Tree { get; set; } = new();
}

sealed class CommitAuthor
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

sealed class Tree
{
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = "";
}

sealed class Author
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";
}

sealed class PullRequest
{
    [JsonPropertyName("repository")]
    public Repository Repository { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("closedAt")]
    public DateTime ClosedAt { get; set; }
}

sealed class Issue
{
    [JsonPropertyName("repository")]
    public Repository Repository { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("closedAt")]
    public DateTime ClosedAt { get; set; }
}

sealed class TimelineEvent
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = "";
    public string Repository { get; set; } = "";
    public string Title { get; set; } = "";
    public int Number { get; set; }
    public string State { get; set; } = "";
    public string Url { get; set; } = "";
}

sealed class RepositoryActivity
{
    public string Name { get; set; } = "";
    public int CommitCount { get; set; }
    public List<PullRequest> PullRequests { get; set; } = [];
    public List<Issue> Issues { get; set; } = [];
}

sealed class ContextSwitch
{
    public string Repository { get; set; } = "";
    public int ItemsCount { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> EventTypes { get; set; } = [];
}
