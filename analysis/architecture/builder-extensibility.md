# Builder Extensibility and Native/External Hybrid Patterns

## The Power of Overlap: Native + External

### Key Insight: Selective Override Pattern

Natives CAN wrap external commands, enabling:
- **Progressive enhancement** - Start with external, optimize hot paths with native
- **Selective override** - Native for some operations, external for others
- **Fallback patterns** - Try native first, fall back to external
- **Platform adaptation** - Native on Windows, external on Unix (or vice versa)

## When to Use What

### Decision Matrix

| Scenario | Use This | Example |
|----------|----------|---------|
| Simple external command | `Shell.Builder()` | `Shell.Builder("echo", "hello")` |
| Complex external tool | Strongly-typed builder | `Git.Clone().WithDepth(1)` |
| Pure in-process operation | Native implementation | `Cat("file.txt")` // Pure C# |
| Hybrid optimization | Native with external fallback | `Ls()` // Native or external |
| Custom team tools | Custom builder | `MyTool.Deploy().ToProduction()` |

## Hybrid Native Pattern

### Example: Ls Command

```csharp
namespace TimeWarp.Amuru.Native.Directory;

public static class Commands
{
    public static CommandOutput Ls(string path = ".", bool detailed = false)
    {
        // Option 1: Pure native implementation
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !detailed)
        {
            // Use .NET APIs for basic listing
            var dir = new DirectoryInfo(path);
            var entries = dir.GetFileSystemInfos();
            var output = string.Join("\n", entries.Select(e => e.Name));
            return new CommandOutput(output, "", 0);
        }
        
        // Option 2: Delegate to external for advanced features
        else
        {
            // On Unix with detailed flag, use real ls for full features
            return Shell.Builder("ls", "-la", path).CaptureSync();
        }
    }
}
```

## Creating Custom Builders

### Basic Pattern

```csharp
public class MyToolBuilder : CommandBuilder
{
    public MyToolBuilder() : base("mytool") { }
    
    public MyToolBuilder WithConfig(string path)
    {
        WithArguments("--config", path);
        return this;
    }
    
    public MyToolBuilder WithVerbose(bool verbose = true)
    {
        if (verbose) WithArguments("--verbose");
        return this;
    }
}

// Static entry point for convenience
public static class MyTool
{
    public static MyToolBuilder Builder() => new MyToolBuilder();
    
    // Specific commands as entry points
    public static MyToolDeployBuilder Deploy() => new MyToolDeployBuilder();
    public static MyToolTestBuilder Test() => new MyToolTestBuilder();
}
```

### Advanced: Builder with Native Override

```csharp
public static class Docker
{
    // Native implementation for common operations
    public static class Native
    {
        public static CommandOutput Ps()
        {
            // Could read from /var/run/docker.sock directly
            // Or use Docker SDK for .NET
            // For now, delegate to external
            return Shell.Builder("docker", "ps").CaptureSync();
        }
        
        public static async IAsyncEnumerable<string> Logs(string container)
        {
            // Stream directly from Docker API
            using var client = new DockerClient();
            await foreach (var line in client.StreamLogs(container))
            {
                yield return line;
            }
        }
    }
    
    // Builder for complex operations
    public static DockerBuilder Builder() => new DockerBuilder();
    public static DockerBuildBuilder Build() => new DockerBuildBuilder();
    public static DockerRunBuilder Run(string image) => new DockerRunBuilder(image);
}

// Usage - choose native or builder
var containers = Docker.Native.Ps();  // Native/optimized
await Docker.Build().WithTag("app:latest").RunAsync();  // Builder for complex
```

## Composing Natives with Builders

### Native Wrapping External

```csharp
public static class Find
{
    public static CommandOutput Execute(string path, string pattern)
    {
        // Windows: Use native C# implementation
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            return new CommandOutput(string.Join("\n", files), "", 0);
        }
        
        // Unix: Delegate to external find (more features)
        return Shell.Builder("find", path, "-name", pattern).CaptureSync();
    }
}
```

### Piping Native Output to Builders

```csharp
// Use native Cat to provide input to external command
var config = Cat("config.json");  // Native command
await Shell.Builder("jq", ".version")
    .WithStandardInput(config.Stdout)
    .RunAsync();

// Chain native with external
var files = Native.Find(".", "*.cs");  // Might be native or external
await Shell.Builder("xargs", "grep", "TODO")
    .WithStandardInput(string.Join("\n", files.Lines))
    .RunAsync();
```

## Conflict Resolution Strategies

### 1. Namespace Separation

```csharp
// Clear separation when both exist
Docker.Native.Ps();        // Native implementation
Docker.Builder().Ps();      // External command
Docker.Ps();               // Could be either - document clearly!
```

### 2. Platform-Specific Defaults

```csharp
public static CommandOutput Ls(string path = ".")
{
    // Windows: Always native (no good ls.exe)
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return NativeLs(path);
    
    // Unix: Prefer external (full-featured)
    if (Shell.Which("ls").Success)
        return Shell.Builder("ls", path).CaptureSync();
    
    // Fallback to native
    return NativeLs(path);
}
```

### 3. Feature Detection

```csharp
public static class Grep
{
    public static CommandOutput Search(string pattern, string path)
    {
        // Use ripgrep if available (faster)
        if (Shell.Which("rg").Success)
            return Shell.Builder("rg", pattern, path).CaptureSync();
        
        // Fall back to grep if available
        if (Shell.Which("grep").Success)
            return Shell.Builder("grep", pattern, path).CaptureSync();
        
        // Ultimate fallback to native C#
        return NativeGrep(pattern, path);
    }
}
```

## Extension Best Practices

### 1. Document Native vs External Clearly

```csharp
/// <summary>
/// Lists directory contents.
/// On Windows: Uses native .NET implementation.
/// On Unix: Delegates to external 'ls' command for full compatibility.
/// </summary>
public static CommandOutput Ls(string path = ".")
```

### 2. Provide Explicit Control

```csharp
public static class Git
{
    // Let users choose
    public static GitBuilder External() => new GitBuilder();
    public static GitNative Native() => new GitNative();
    
    // Smart default
    public static IGitOperations Smart() => 
        HasNativeGitSupport() ? new GitNative() : new GitBuilder();
}
```

### 3. Performance Hints

```csharp
public static class Find
{
    // Fast path for common cases
    public static CommandOutput QuickFind(string pattern)
    {
        // Native C# for simple patterns
        if (!pattern.Contains("*") && !pattern.Contains("?"))
            return NativeFind(".", pattern);
        
        // External for complex patterns
        return Shell.Builder("find", ".", "-name", pattern).CaptureSync();
    }
}
```

## Community Builder Examples

### Progressive Enhancement Pattern

```csharp
// Start with external wrapper
public class KubectlBuilder : CommandBuilder
{
    public KubectlBuilder() : base("kubectl") { }
}

// Later, optimize hot paths with native
public static class Kubectl
{
    // Native for frequently used, simple operations
    public static class Native
    {
        public static CommandOutput GetPods(string ns = "default")
        {
            // Could use Kubernetes C# client
            using var client = new KubernetesClient();
            var pods = client.ListPods(ns);
            return FormatAsCommandOutput(pods);
        }
    }
    
    // Builder for complex operations
    public static KubectlBuilder Builder() => new KubectlBuilder();
    
    // Smart routing
    public static CommandOutput Get(string resource)
    {
        // Use native for common resources
        if (resource == "pods" || resource == "services")
            return Native.GetPods();
        
        // External for everything else
        return Builder().WithArguments("get", resource).CaptureSync();
    }
}
```

## Summary

- **Overlap is a FEATURE** - Enables progressive enhancement
- **Natives CAN wrap externals** - For optimization and platform adaptation
- **Builders and Natives complement** - Use both in same tool
- **Document clearly** - When using native vs external
- **Provide escape hatches** - Let users choose implementation
- **Optimize hot paths** - Native for frequent operations
- **Delegate complexity** - External for feature-rich operations

This hybrid model provides maximum flexibility: start simple with external commands, optimize with native implementations where it matters, and give users control when they need it.