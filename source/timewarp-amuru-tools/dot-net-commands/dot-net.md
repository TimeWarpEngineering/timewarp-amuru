# DotNet Fluent API Reference

The `DotNet` class provides a comprehensive fluent API for executing .NET CLI commands. It wraps the standard `dotnet` command-line interface with strongly-typed, discoverable methods that follow the same graceful error handling philosophy as the rest of TimeWarp.Cli.

## Overview

All DotNet commands follow a consistent fluent builder pattern:

```csharp
// Basic usage
await DotNet.Build().ExecuteAsync();

// With configuration
await DotNet.Build()
    .WithProject("MyApp.csproj")
    .WithConfiguration("Release")
    .WithNoRestore()
    .ExecuteAsync();

// Capture output
var testResults = await DotNet.Test()
    .WithProject("MyApp.Tests.csproj")
    .WithFilter("Category=Unit")
    .GetStringAsync();
```

## Available Commands

### Core Development Commands

#### `DotNet.Run()`
Execute applications with full configuration support including launch profiles, architecture targeting, and environment variables.

```csharp
// Basic application execution
await DotNet.Run().ExecuteAsync();

// Run specific project with configuration
await DotNet.Run()
    .WithProject("MyApp.csproj")
    .WithConfiguration("Release")
    .WithLaunchProfile("Production")
    .ExecuteAsync();

// Run with environment variables and arguments
await DotNet.Run()
    .WithEnvironmentVariable("NODE_ENV", "development")
    .WithArguments("--verbose", "--port", "8080")
    .ExecuteAsync();
```

**Key Features:**
- Launch profile configuration from `launchSettings.json`
- Architecture and OS targeting (`--arch`, `--os`)
- Environment variable configuration
- Program argument passing
- Build and restore control

#### `DotNet.Build()`
Build projects with comprehensive configuration options.

```csharp
// Basic build
await DotNet.Build().ExecuteAsync();

// Advanced build configuration
await DotNet.Build("MyApp.csproj")
    .WithConfiguration("Release")
    .WithFramework("net10.0")
    .WithRuntime("win-x64")
    .WithProperty("Version", "1.0.0")
    .WithNoRestore()
    .ExecuteAsync();
```

**Key Features:**
- Multi-framework targeting
- Runtime-specific builds
- MSBuild property configuration
- Verbosity control
- Terminal logger integration

#### `DotNet.Test()`
Run tests with filtering, logging, and blame detection.

```csharp
// Run all tests
await DotNet.Test().ExecuteAsync();

// Advanced test configuration
var testResults = await DotNet.Test()
    .WithProject("MyApp.Tests.csproj")
    .WithConfiguration("Release")
    .WithFilter("Category=Unit&Priority=High")
    .WithLogger("trx;LogFileName=TestResults.trx")
    .WithBlame()
    .GetStringAsync();
```

**Key Features:**
- Test filtering with complex expressions
- Multiple logger support
- Blame detection for crash analysis
- Coverage collection
- Results file generation

### Maintenance Commands

#### `DotNet.Clean()`
Clean build outputs with targeted framework and runtime support.

```csharp
// Clean all outputs
await DotNet.Clean().ExecuteAsync();

// Clean specific configuration
await DotNet.Clean("MyApp.csproj")
    .WithConfiguration("Release")
    .WithFramework("net10.0")
    .WithRuntime("win-x64")
    .ExecuteAsync();
```

#### `DotNet.Restore()`
Restore dependencies with cache control and source configuration.

```csharp
// Basic restore
await DotNet.Restore().ExecuteAsync();

// Advanced restore with cache control
await DotNet.Restore()
    .WithProject("MyApp.csproj")
    .WithSource("https://api.nuget.org/v3/index.json")
    .WithSource("https://my-private-feed.com/v3/index.json")
    .WithNoCache()
    .WithLockMode()
    .ExecuteAsync();
```

**Key Features:**
- Multiple package source support
- Cache control (`--no-cache`)
- Lock file management
- Package directory configuration
- Force resolution

### Publishing Commands

#### `DotNet.Publish()`
Publish applications with deployment configuration including self-contained, ReadyToRun, and single-file options.

```csharp
// Basic publish
await DotNet.Publish().ExecuteAsync();

// Production deployment
await DotNet.Publish("MyApp.csproj")
    .WithConfiguration("Release")
    .WithRuntime("win-x64")
    .WithSelfContained()
    .WithReadyToRun()
    .WithSingleFile()
    .WithTrimmed()
    .WithOutput("./publish")
    .ExecuteAsync();
```

**Key Features:**
- Self-contained deployment
- ReadyToRun optimization
- Single-file publishing
- Code trimming
- Framework-dependent deployment

#### `DotNet.Pack()`
Create NuGet packages with symbol and source support.

```csharp
// Basic package creation
await DotNet.Pack().ExecuteAsync();

// Package with symbols and metadata
await DotNet.Pack("MyLibrary.csproj")
    .WithConfiguration("Release")
    .WithOutput("./packages")
    .WithVersionSuffix("beta")
    .IncludeSymbols()
    .IncludeSource()
    .WithServiceable()
    .ExecuteAsync();
```

### Package Management Commands

#### `DotNet.ListPackages()`
List package references with vulnerability and outdated package detection.

```csharp
// List all packages
var packages = await DotNet.ListPackages().ToListAsync();

// Find outdated packages
var outdatedPackages = await DotNet.ListPackages()
    .WithProject("MyApp.csproj")
    .WithOutdated()
    .IncludeTransitive()
    .GetLinesAsync();

// Check for vulnerabilities
var vulnerablePackages = await DotNet.ListPackages()
    .WithVulnerable()
    .WithFormat("json")
    .GetStringAsync();
```

**Key Features:**
- Outdated package detection
- Vulnerability scanning
- Transitive dependency analysis
- JSON output format
- Prerelease package inclusion

#### `DotNet.AddPackage()`
Add NuGet package references with version and source control.

```csharp
// Add latest package
await DotNet.AddPackage("Newtonsoft.Json").ExecuteAsync();

// Add specific version
await DotNet.AddPackage("Newtonsoft.Json", "13.0.3").ExecuteAsync();

// Add with specific configuration
await DotNet.AddPackage("Microsoft.Extensions.Logging")
    .WithProject("MyApp.csproj")
    .WithFramework("net10.0")
    .WithPrerelease()
    .WithSource("https://api.nuget.org/v3/index.json")
    .WithNoRestore()
    .ExecuteAsync();
```

#### `DotNet.RemovePackage()`
Remove NuGet package references.

```csharp
// Remove package
await DotNet.RemovePackage("Newtonsoft.Json").ExecuteAsync();

// Remove from specific project
await DotNet.RemovePackage("Microsoft.Extensions.Logging")
    .WithProject("MyApp.csproj")
    .ExecuteAsync();
```

## Universal Features

All DotNet commands support these common features:

### Working Directory Configuration
```csharp
await DotNet.Build()
    .WithWorkingDirectory("/path/to/project")
    .ExecuteAsync();
```

### Environment Variables
```csharp
await DotNet.Run()
    .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production")
    .WithEnvironmentVariable("ConnectionString", "...")
    .ExecuteAsync();
```

### MSBuild Properties
```csharp
await DotNet.Build()
    .WithProperty("Version", "1.0.0")
    .WithProperty("AssemblyVersion", "1.0.0.0")
    .ExecuteAsync();
```

### Execution Methods
All commands provide three execution methods:

```csharp
// Execute without capturing output
await DotNet.Build().ExecuteAsync();

// Capture output as string
var output = await DotNet.Build().GetStringAsync();

// Capture output as line array
var lines = await DotNet.Build().GetLinesAsync();
```

## Error Handling

DotNet commands follow TimeWarp.Cli's graceful error handling philosophy:

- **No exceptions thrown** for command failures
- **Empty results** returned instead of throwing
- **Validation errors** return safe defaults
- **Non-zero exit codes** don't cause exceptions

```csharp
// Safe execution - won't throw even if command fails
var testResults = await DotNet.Test()
    .WithProject("NonExistentProject.csproj")
    .GetStringAsync(); // Returns empty string on failure

var buildOutput = await DotNet.Build()
    .WithConfiguration("InvalidConfig")
    .GetLinesAsync(); // Returns empty array on failure
```

## Integration with Microsoft Documentation

For detailed information about specific command options and their behavior, refer to the official Microsoft documentation:

- [dotnet run command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run)
- [dotnet build command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
- [dotnet test command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [dotnet clean command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-clean)
- [dotnet restore command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [dotnet publish command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
- [dotnet pack command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack)
- [dotnet list package command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-list-package)
- [dotnet add package command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package)
- [dotnet remove package command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-remove-package)

The TimeWarp.Cli DotNet API provides a fluent, strongly-typed interface over these standard CLI commands while maintaining full compatibility with their documented behavior and options.

## Pipeline Integration

DotNet commands integrate with TimeWarp.Cli's pipeline system through their `CommandResult` return values:

```csharp
// Chain dotnet commands with other commands
var filteredOutput = await DotNet.ListPackages()
    .WithOutdated()
    .Pipe("grep", "Microsoft")
    .GetLinesAsync();

// Process test results
var failedTests = await DotNet.Test()
    .WithLogger("console;verbosity=detailed")
    .Pipe("grep", "Failed")
    .GetLinesAsync();
```

## Best Practices

### Command Selection
- Use `ExecuteAsync()` for commands that perform actions (build, clean, publish)
- Use `GetStringAsync()` for formatted output that should be displayed
- Use `GetLinesAsync()` for output that needs line-by-line processing

### Configuration
- Always specify project files explicitly in multi-project solutions
- Use configuration-specific settings for different environments
- Leverage MSBuild properties for version and metadata control

### Error Handling
- Check for empty results to detect command failures
- Use appropriate timeouts for long-running operations
- Consider the graceful failure philosophy in your application logic

### Performance
- Use `WithNoRestore()` when appropriate to skip redundant restores
- Consider `WithNoLogo()` to reduce output verbosity
- Use cached commands for expensive operations that are repeated