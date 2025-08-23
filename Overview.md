# C# Script Command Execution Overview

## What We're Building

A fluent API wrapper around CliWrap to make shell command execution in C# scripts feel natural and concise, similar to PowerShell pipelines but with C# syntax and type safety.

## The Problem

Traditional C# command execution is verbose and cumbersome:

```csharp
// Old way with CliWrap
var result = await Cli.Wrap("find")
    .WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" })
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();

var files = result.StandardOutput.Split(
    new[] { '\n', '\r' }, 
    StringSplitOptions.RemoveEmptyEntries
);
```

## The Solution

Clean, fluent API that feels like natural C# but with shell power:

```csharp
// New way with our wrapper
var files = await Shell.Builder("find", homeDirectory, "-name", "*.ps1", "-type", "f").GetLinesAsync();
```

## Architecture

### Core Components

1. **`CommandExtensions.Run()`** - Static entry point that creates a command
2. **`CommandResult`** - Wrapper around CliWrap's `Command` with fluent methods
3. **Output Methods** - Common patterns for processing command output

### Key Design Decisions

- **Fluent Interface**: Each method returns an object that can be chained
- **Async by Default**: All operations return `Task<T>` for proper async/await
- **Sensible Defaults**: `ValidationMode.None` so non-zero exit codes don't throw
- **Type Safety**: Strong typing on return values (`string`, `string[]`, etc.)

## Current Implementation

```csharp
public static class CommandExtensions
{
    public static CommandResult Run(string command, params string[] args)
    {
        var cliCommand = Cli.Wrap(command)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None);
            
        return new CommandResult(cliCommand);
    }
}

public class CommandResult
{
    private readonly Command _command;
    
    public async Task<string> GetStringAsync()      // Raw stdout
    public async Task<string[]> GetLinesAsync()     // Split lines, no empties
    public async Task ExecuteAsync()                // Just run, no output
}
```

## Usage Examples

```csharp
// Get raw output
var date = await Shell.Builder("date").GetStringAsync();

// Get lines as array
var files = await Shell.Builder("ls", "-la").GetLinesAsync();

// Just execute (fire and forget)
await Shell.Builder("mkdir", "temp").ExecuteAsync();

// Strongly-typed dotnet command examples
var buildResult = await Shell.Builder("dotnet", "build", "--configuration", "Release").GetStringAsync();
var testOutput = await Shell.Builder("dotnet", "test", "--logger", "console").GetLinesAsync();
var packages = await Shell.Builder("dotnet", "list", "package").GetLinesAsync();
await Shell.Builder("dotnet", "restore").ExecuteAsync();
var runOutput = await Shell.Builder("dotnet", "run", "--project", "MyApp.csproj", "--", "arg1", "arg2").GetStringAsync();
```

## Why This Matters for C# Scripts

With the new shebang support (`#!/usr/bin/dotnet run`), C# can now compete with bash/PowerShell for system scripting. But we need better ergonomics than raw CliWrap.

## Next Steps (Future Phases)

1. **Pipeline Support**: `.Pipe("grep", "pattern")` for command chaining
2. **Strongly-Typed Helpers**: `FileSystem.Find("*.cs")` for common operations
3. **Interactive Tools**: `.ToFzf()` for selection UIs
4. **Error Handling**: Better exception handling and logging
5. **Performance**: Caching and optimization for repeated commands

## The Vision

Transform this 50-line monster:
```csharp
var result = await (
    Cli.Wrap("find").WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" }) | 
    Cli.Wrap("xargs").WithArguments(new[] { "grep", "-l", "Luna" })
).ExecuteBufferedAsync();
// ... 40 more lines of processing
```

Into this elegant one-liner:
```csharp
var selected = await Shell.Builder("find", "~", "-name", "*.ps1", "-type", "f")
    .Pipe("xargs", "grep", "-l", "Luna")
    .Pipe("fzf", "--preview=cat {}")
    .ExecuteAsync();
```

C# scripting with the power of shell commands, the safety of strong typing, and the elegance of LINQ-style fluent APIs.

## The Future: Advanced APIs

Once we have the foundation, we can build higher-level abstractions:

### Strongly-Typed File Operations
```csharp
// Instead of raw find commands
var files = await FileSystem.Find("*.cs")
    .InDirectory("~/projects")
    .ContainingText("async")
    .ModifiedAfter(DateTime.Today.AddDays(-7))
    .ToListAsync();

// Git operations with type safety
var branches = await Git.Branches()
    .Remote()
    .ContainingCommit("abc123")
    .ToListAsync();

// Dotnet CLI with fluent interface
var testResults = await DotNet.Test()
    .WithProject("MyApp.Tests.csproj")
    .WithConfiguration("Release")
    .WithFilter("Category=Unit")
    .WithLogger("trx")
    .RunAsync();

var buildOutput = await DotNet.Build()
    .WithConfiguration("Debug")
    .WithNoRestore()
    .WithWarningsAsErrors()
    .ExecuteAsync();

var packages = await DotNet.ListPackages()
    .IncludeTransitive()
    .Outdated()
    .ToListAsync();
```

### Interactive Tool Integration
```csharp
// Fuzzy finding with rich preview
var selected = await files
    .ToFzf()
    .WithPreview(file => File.ReadAllText(file))
    .WithMultiSelect()
    .SelectAsync();

// Process selection with rich UI
var processes = await System.Processes()
    .Where(p => p.Name.Contains("dotnet"))
    .ToFzf()
    .WithColumns("PID", "Name", "CPU%")
    .SelectAsync();
```

### Declarative Pipeline Building
```csharp
// Complex data processing pipelines
var results = await Pipeline
    .From(FileSystem.Find("*.log"))
    .Transform(file => File.ReadLines(file))
    .Filter(line => line.Contains("ERROR"))
    .GroupBy(line => ExtractTimestamp(line).Date)
    .OrderBy(group => group.Key)
    .ToListAsync();
```

### Natural Language-Style APIs
```csharp
// Almost English-like syntax
var deployment = await Docker.Containers()
    .Where(c => c.Name.StartsWith("web"))
    .That.AreRunning()
    .On.Port(80)
    .RestartAsync();

// System administration
await Services.Named("nginx")
    .EnsureRunning()
    .WithConfig(config => config.EnableSsl())
    .ApplyAsync();
```

This vision transforms C# from a "heavy" language into a concise, powerful scripting environment that rivals PowerShell's expressiveness while maintaining strong typing and IntelliSense support.

## Key Advantages Over PowerShell

### Simpler Input Handling
One pain point with PowerShell is sending special characters or control sequences to processes. Remember how easy it was to send ESC to fzf in our C# script?

```csharp
// C# - Clean and simple
echo -e '\033' | ./FindLunaPs1FilesWithFzfAsync.cs

// PowerShell - Requires complex escape sequences and workarounds
# Much more difficult to send control characters
```

Our C# approach will maintain this simplicity, making it easy to:
- Send control characters (ESC, Ctrl+C, etc.)
- Pipe binary data without corruption
- Handle interactive tool input naturally
- Test scripts with automated input

This is a fundamental design goal: make the simple things simple, and the hard things possible.

## Way Out There: AI-Powered Pipelines

The ultimate vision - integrate LLM inference directly into command pipelines:

### LLM as a Pipeline Stage
```csharp
// Extract structured data from unstructured logs
var incidents = await Shell.Builder("tail", "-f", "/var/log/system.log")
    .Pipe("grep", "ERROR")
    .PipeToLLM("Extract the timestamp, error code, and service name as JSON")
    .ParseJson<Incident>()
    .ToListAsync();

// Natural language file operations
var files = await FileSystem.Find("*.cs")
    .PipeToLLM("Which of these files look like unit tests?")
    .ToListAsync();
```

### Intelligent Code Analysis
```csharp
// Security vulnerability scanning with AI
var vulnerabilities = await Git.ChangedFiles()
    .Where(f => f.Extension == ".cs")
    .ReadContents()
    .PipeToLLM(@"Analyze for security issues:
        - SQL injection
        - Path traversal
        - Hardcoded secrets
        Return findings as JSON")
    .ParseJson<SecurityFinding>()
    .ToListAsync();

// Smart code refactoring suggestions
var suggestions = await FileSystem.Find("*.cs")
    .ContainingText("HttpClient")
    .PipeToLLM("Find instances where HttpClient is not properly disposed")
    .WithContext(file => File.ReadAllText(file))
    .ToListAsync();
```

### Conversational System Administration
```csharp
// Natural language system queries
var result = await System.Ask("What processes are using more than 50% CPU?")
    .ExecuteAsync();

// Intelligent log analysis
var analysis = await Logs.From("/var/log")
    .Since(DateTime.Now.AddHours(-1))
    .Ask("What unusual patterns do you see? Are there any security concerns?")
    .ExecuteAsync();

// Self-healing systems
await Monitor.System()
    .When("disk space below 10%")
    .Ask("What old files can we safely delete?")
    .ThenExecute(files => files.Delete())
    .StartAsync();
```

### AI-Powered Data Transformation
```csharp
// Transform between formats using natural language
var yaml = await Shell.Builder("cat", "config.json")
    .PipeToLLM("Convert this JSON to YAML format")
    .SaveAs("config.yaml");

// Generate code from data
var models = await Database.Query("SELECT * FROM schema")
    .PipeToLLM("Generate C# record types for these tables")
    .SaveAs("Models.cs");

// Smart data extraction
var contacts = await Shell.Builder("find", ".", "-name", "*.pdf", "-type", "f")
    .SelectFiles()
    .PipeToLLM("Extract all email addresses and phone numbers")
    .Distinct()
    .ToListAsync();
```

### Collaborative Scripting
```csharp
// Let the AI help write the script
var script = await ScriptBuilder
    .Describe("I need to find all Docker containers that have been running for more than 7 days and restart them")
    .GenerateScript()
    .Review()
    .ExecuteAsync();

// Dynamic pipeline generation
var pipeline = await PipelineBuilder
    .FromNaturalLanguage("Find all log files from yesterday that mention 'timeout', extract the service names, and create a summary report")
    .Build()
    .ExecuteAsync();
```

This transforms C# scripting from "automation" to "intelligent automation" - where AI becomes a first-class citizen in your command pipelines, enabling natural language interfaces to complex system operations.

## The Self-Improving Shell: Bridging Semantic and Syntactic

The real breakthrough is recognizing that we now live in a world with two types of code:

- **Syntactic Code**: Traditional, deterministic, performant code we write
- **Semantic Code**: Natural language instructions processed by LLMs

The magic happens when they work together:

### Semantic to Syntactic Evolution
```csharp
// First time - semantic approach (slower, flexible)
var result = await Shell.Semantic("Find all CSV files modified today and sum the values in the third column");

// The system learns and generates syntactic version
var optimized = await Shell.Learn(result)
    .GenerateOptimizedCode()
    .SaveAs("SumTodaysCsvColumn3.cs");

// Future runs use the optimized syntactic version (fast, deterministic)
var sum = await Shell.Builder("./SumTodaysCsvColumn3.cs").GetStringAsync();
```

### Adaptive Command Learning
```csharp
// The shell observes patterns in your semantic queries
await Shell.EnableLearning();

// After multiple similar queries like:
// "Show me the largest files in my Downloads folder"
// "What are the biggest files in ~/Downloads?"
// "List Downloads files by size"

// The shell automatically creates:
public static class MyCommands 
{
    public static Task<FileInfo[]> LargestDownloads(int count = 10) =>
        FileSystem.Find("*")
            .InDirectory("~/Downloads")
            .OrderBySize(descending: true)
            .Take(count)
            .ToListAsync();
}
```

### Progressive Optimization
```csharp
// Start with semantic (exploring the problem)
var analysis = await Data.Semantic(@"
    Read all the log files from last week,
    find patterns in error messages,
    group by service name,
    create a summary report
");

// System suggests syntactic optimization
var suggestion = await Shell.Suggest(analysis);
// "I noticed you run similar queries often. Would you like me to create an optimized version?"

// Approve and deploy
await suggestion.Review()
    .Optimize()
    .TestAgainstOriginal()
    .Deploy("WeeklyErrorReport.cs");
```

### Hybrid Execution Strategies
```csharp
// Smart routing between semantic and syntactic
var command = await Shell.Smart(@"
    Find processes using more than 80% CPU
    and restart any that have been running over 24 hours
");

// The shell recognizes:
// - "Find processes using more than 80% CPU" → maps to existing syntactic code
// - "restart any running over 24 hours" → new requirement, uses semantic
// Result: Hybrid execution for best performance

// Over time, frequently used semantic patterns become syntactic
await Shell.ShowLearningReport();
// "Converted 15 semantic patterns to syntactic code this month"
// "Average performance improvement: 150x"
// "Suggested new commands based on your usage..."
```

This creates a self-improving system where:
1. You start with natural language (semantic) for exploration
2. The system learns your patterns and generates optimized code
3. Common operations automatically become fast, syntactic implementations
4. You get the best of both worlds: flexibility when exploring, performance when repeating

We're essentially building a shell that gets smarter and faster the more you use it - just like what we're doing right now in this conversation!

## Today's Reality: Claude CLI

While we're building toward this vision, we already have a powerful semantic tool available today - Claude CLI (the very tool creating this document!). Claude Code can be used as a command-line tool for AI-powered operations:

```bash
# Use Claude directly in your shell
claude "analyze these log files and find patterns"

# Pipe command output to Claude
find . -name "*.cs" | claude "which of these files need refactoring?"

# Use Claude in scripts
result=$(git diff | claude "summarize these changes")
```

Learn more: [Claude CLI Reference](https://docs.anthropic.com/en/docs/claude-code/cli-reference)

This means you can start experimenting with semantic/syntactic hybrid approaches today, using Claude as your semantic execution engine while building syntactic C# implementations.

### Using Claude in C# Scripts Today
```csharp
// Semantic analysis with Claude CLI
var analysis = await Shell.Builder("git", "diff")
    .Pipe("claude", "summarize the changes and suggest a commit message")
    .GetStringAsync();

// Hybrid approach - syntactic search, semantic analysis
var testFiles = await Shell.Builder("find", ".", "-name", "*Test.cs", "-type", "f")
    .GetLinesAsync();
    
var coverage = await Shell.Builder("echo", string.Join('\n', testFiles))
    .Pipe("claude", "which test files seem to be missing coverage for their corresponding source files?")
    .GetStringAsync();

// Let Claude help generate syntactic code
var code = await Shell.Builder("claude", "generate a C# method that finds and deletes temporary files older than 7 days")
    .SaveAs("CleanupTempFiles.cs");
```
