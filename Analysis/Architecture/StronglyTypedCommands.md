# Strongly-Typed Command Builders

## Overview

Strongly-typed builders provide fluent APIs for specific external tools, offering IntelliSense, compile-time checking, and better discoverability than raw string arguments.

## Current Implementations

### DotNet Commands

```csharp
// Fluent API for dotnet CLI
await DotNet.Build()
    .WithConfiguration("Release")
    .WithNoRestore()
    .RunAsync();

await DotNet.Test()
    .WithFilter("Category=Unit")
    .WithLogger("trx")
    .RunAsync();

// Global dotnet options
var sdks = await DotNet.WithListSdks().CaptureAsync();
var version = await DotNet.WithVersion().CaptureAsync();
```

### Fzf (Fuzzy Finder)

```csharp
// Interactive selection
var env = await Fzf.Builder()
    .FromInput("dev", "staging", "prod")
    .WithPrompt("Select environment: ")
    .SelectAsync();

// Multi-select
var selections = await Fzf.Builder()
    .FromInput("Red", "Green", "Blue")
    .WithMulti()
    .SelectAsync();
```

### Ghq (Git Repository Manager)

```csharp
// List repositories
var repos = await Ghq.Builder()
    .List()
    .CaptureAsync();

// Clone repository
await Ghq.Builder()
    .Get("github.com/user/repo")
    .RunAsync();
```

## Potential Future Builders

### Git Commands

```csharp
await Git.Clone("https://github.com/user/repo")
    .WithDepth(1)
    .WithBranch("main")
    .RunAsync();

await Git.Commit()
    .WithMessage("feat: add new feature")
    .WithAll()
    .RunAsync();

var log = await Git.Log()
    .WithOneline()
    .WithLimit(10)
    .CaptureAsync();
```

### Docker Commands

```csharp
await Docker.Build()
    .WithTag("myapp:latest")
    .WithFile("Dockerfile.prod")
    .WithBuildArg("VERSION", "1.0.0")
    .RunAsync();

await Docker.Run("myapp:latest")
    .WithPort(8080, 80)
    .WithEnv("NODE_ENV", "production")
    .WithDetached()
    .RunAsync();

var containers = await Docker.Ps()
    .WithAll()
    .WithFormat("json")
    .CaptureAsync();
```

### Kubectl Commands

```csharp
await Kubectl.Apply()
    .FromFile("deployment.yaml")
    .WithNamespace("production")
    .RunAsync();

var pods = await Kubectl.Get("pods")
    .WithAllNamespaces()
    .WithOutput("json")
    .CaptureAsync();

await Kubectl.Logs("pod-name")
    .WithFollow()
    .WithTail(100)
    .StreamStdoutAsync();
```

### NPM/Yarn Commands

```csharp
await Npm.Install()
    .WithPackage("typescript", "5.0.0")
    .WithSaveDev()
    .RunAsync();

await Npm.Run("build")
    .WithScript("build:prod")
    .RunAsync();

var outdated = await Npm.Outdated()
    .WithJson()
    .CaptureAsync();
```

## Design Principles

1. **Discoverability**: IntelliSense shows available options
2. **Type Safety**: Compile-time validation of arguments
3. **Consistency**: All builders follow similar patterns
4. **Flexibility**: Can always fall back to Shell.Builder() for edge cases
5. **Documentation**: XML comments on methods explain options

## Implementation Pattern

```csharp
public static class Git
{
    public static GitBuilder Builder() => new GitBuilder();
    public static GitCommitBuilder Commit() => new GitCommitBuilder();
    public static GitCloneBuilder Clone(string repo) => new GitCloneBuilder(repo);
}

public class GitCommitBuilder : CommandBuilder
{
    public GitCommitBuilder WithMessage(string message) { }
    public GitCommitBuilder WithAll() { }
    public GitCommitBuilder WithAmend() { }
}
```

## When to Create a Builder

Create a strongly-typed builder when:
- The tool is commonly used in the target domain
- The tool has complex options that benefit from IntelliSense
- Type safety prevents common mistakes
- The tool's API is stable enough to warrant the effort

Don't create a builder for:
- Simple commands with few options
- Rarely used tools
- Tools with frequently changing APIs
- Internal/proprietary tools (users can create their own)