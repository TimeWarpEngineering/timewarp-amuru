# TimeWarp.Amuru Caching Strategy

## The Question

Should we provide caching at all? No shell caches commands automatically. Users can cache results themselves in C# if needed.

## Arguments For Caching

1. **Expensive Operations**: Commands like `find /` or large `git log` operations
2. **Repeated Calls**: Scripts that call the same command multiple times
3. **Development Iteration**: Re-running scripts during development
4. **CI/CD Optimization**: Avoiding redundant operations in pipelines

## Arguments Against Caching

1. **Not Shell-Like**: Bash/PowerShell don't cache - why should we?
2. **User Responsibility**: In C#, users can easily cache themselves:
   ```csharp
   // User can do this themselves
   private static CommandOutput? cachedGitStatus;
   public static async Task<CommandOutput> GetGitStatus()
   {
       return cachedGitStatus ??= await Shell.Builder("git", "status").CaptureAsync();
   }
   ```
3. **Dangerous Defaults**: Caching state-changing commands is a footgun
4. **Complexity**: Cache invalidation, TTL, memory management adds complexity
5. **Debugging Issues**: Cached results can make debugging confusing

## If We Provide Caching

### Design Requirements

1. **Explicit Opt-In**: Never cache by default
2. **Clear Warnings**: Document that only read-only commands should be cached
3. **Transparent Keys**: No SHA256 hashing, use readable compound keys

### Proposed API

```csharp
// Basic caching with auto-generated key
var findCommand = Shell.Builder("find", ".", "-name", "*.cs")
    .Cached();  // Key: "find|.|name|*.cs|/current/dir"

// Cache with TTL
var gitStatus = Shell.Builder("git", "status")
    .Cached(TimeSpan.FromMinutes(5));

// Custom cache key for explicit control
var report = Shell.Builder("generate-report.sh")
    .Cached($"report-{date:yyyy-MM-dd}");

// Global cache management
CommandCache.Clear();
CommandCache.Invalidate("report-2024-01-15");
CommandCache.Contains("git|status|/repo/path");
```

### Cache Key Generation

Simple, readable compound keys (no hashing):
```csharp
string GenerateCacheKey(CommandBuilder builder)
{
    var parts = new List<string>
    {
        builder.Executable,
        string.Join("|", builder.Arguments),
        builder.WorkingDirectory ?? Environment.CurrentDirectory
    };
    
    // Only include env vars if they exist
    if (builder.EnvironmentVariables?.Any() == true)
    {
        parts.Add(string.Join("|", builder.EnvironmentVariables
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}={kvp.Value}")));
    }
    
    return string.Join("|", parts);
}
```

### Safety Measures

1. **Consider ReadOnly Flag**: `.Cached(readOnly: true)` to make intent explicit
2. **Debug Logging**: Log cache hits/misses in debug mode
3. **Memory Limits**: LRU eviction or max cache size
4. **Process Lifetime Only**: No persistence between runs

## Alternative: No Built-In Caching

Let users handle their own caching:

```csharp
// Extension method users can write themselves
public static class CacheExtensions
{
    private static readonly Dictionary<string, CommandOutput> cache = new();
    
    public static async Task<CommandOutput> CaptureWithCache(
        this CommandBuilder builder, 
        string? cacheKey = null,
        TimeSpan? ttl = null)
    {
        var key = cacheKey ?? builder.ToString();
        if (cache.TryGetValue(key, out var cached))
            return cached;
            
        var result = await builder.CaptureAsync();
        cache[key] = result;
        
        if (ttl.HasValue)
        {
            Task.Delay(ttl.Value).ContinueWith(_ => cache.Remove(key));
        }
        
        return result;
    }
}
```

## Recommendation

**Lean towards NO built-in caching.**

Reasons:
1. Keeps the library focused and simple
2. Avoids dangerous misuse
3. Users can easily add caching if needed
4. Not a standard shell feature
5. Reduces API surface and complexity

If we do add it later based on user feedback, it should be:
- Opt-in via separate package (TimeWarp.Amuru.Caching)
- Very explicit about dangers
- Simple implementation without complex features