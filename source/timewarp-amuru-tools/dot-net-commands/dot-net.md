# DotNet Fluent API Reference

The `DotNet` class in `TimeWarp.Amuru` is a fluent wrapper over the `dotnet` CLI. Builders run through `RunAsync` (streams to the terminal, returns the exit code) and `CaptureAsync` (returns `CommandOutput`). There is no `ExecuteAsync`, `GetStringAsync`, or `GetLinesAsync`.

Default validation is `None`: a non-zero exit is the returned exit code or `CommandOutput.Success == false`. It is not thrown, and it is not rewritten into an empty string.

## Execution

```csharp
int exitCode = await DotNet.Build()
  .WithProject("MyApp.csproj")
  .WithConfiguration("Release")
  .WithNoRestore()
  .RunAsync();

CommandOutput output = await DotNet.Build()
  .WithProject("MyApp.csproj")
  .CaptureAsync();

string stdout = output.Stdout;
string[] lines = output.GetLines();
bool succeeded = output.Success;
```

`RunAndCaptureAsync` is on the build, clean, and pack builders, and on `DotNet.WithVersion()` / `WithListSdks()` / `WithInfo()` (`DotNetBuilder`). Test, Run, Publish, Restore, ListPackages, AddPackage, and RemovePackage expose `RunAsync` and `CaptureAsync` only.

## Commands

### `DotNet.Run()`

```csharp
await DotNet.Run().RunAsync();

await DotNet.Run()
  .WithProject("MyApp.csproj")
  .WithConfiguration("Release")
  .WithLaunchProfile("Production")
  .RunAsync();

await DotNet.Run()
  .WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")
  .WithArguments("--verbose", "--port", "8080")
  .RunAsync();
```

### `DotNet.Build()`

`Build()` and `Build(string project)` both exist. The string overload is `WithProject`.

```csharp
await DotNet.Build().RunAsync();

await DotNet.Build("MyApp.csproj")
  .WithConfiguration("Release")
  .WithFramework("net10.0")
  .WithRuntime("win-x64")
  .WithProperty("Version", "1.0.0")
  .WithNoRestore()
  .RunAsync();
```

### `DotNet.Test()`

```csharp
await DotNet.Test().RunAsync();

CommandOutput testOutput = await DotNet.Test()
  .WithProject("MyApp.Tests.csproj")
  .WithConfiguration("Release")
  .WithFilter("Category=Unit")
  .WithLogger("trx;LogFileName=TestResults.trx")
  .WithBlame()
  .CaptureAsync();
```

### `DotNet.Clean()`

```csharp
await DotNet.Clean().RunAsync();

await DotNet.Clean("MyApp.csproj")
  .WithConfiguration("Release")
  .WithFramework("net10.0")
  .WithRuntime("win-x64")
  .RunAsync();
```

### `DotNet.Restore()`

Lock-file restore is `WithLockedMode()`, not `WithLockMode()`.

```csharp
await DotNet.Restore().RunAsync();

await DotNet.Restore()
  .WithProject("MyApp.csproj")
  .WithSource("https://api.nuget.org/v3/index.json")
  .WithNoCache()
  .WithLockedMode()
  .RunAsync();
```

### `DotNet.Publish()`

Single-file and trimming are `WithSingleFile()` and `WithTrimmed()`.

```csharp
await DotNet.Publish().RunAsync();

await DotNet.Publish("MyApp.csproj")
  .WithConfiguration("Release")
  .WithRuntime("win-x64")
  .WithSelfContained()
  .WithReadyToRun()
  .WithSingleFile()
  .WithTrimmed()
  .WithOutput("./publish")
  .RunAsync();
```

### `DotNet.Pack()`

```csharp
await DotNet.Pack().RunAsync();

await DotNet.Pack("MyLibrary.csproj")
  .WithConfiguration("Release")
  .WithOutput("./packages")
  .WithVersionSuffix("beta")
  .IncludeSymbols()
  .IncludeSource()
  .WithServiceable()
  .RunAsync();
```

### `DotNet.ListPackages()`

Outdated and vulnerable listings are `Outdated()` and `Vulnerable()`, not `WithOutdated()` / `WithVulnerable()`. `ToListAsync()` returns stdout lines.

```csharp
string[] packages = await DotNet.ListPackages().ToListAsync();

string[] outdated = await DotNet.ListPackages()
  .WithProject("MyApp.csproj")
  .Outdated()
  .IncludeTransitive()
  .ToListAsync();

CommandOutput vulnerable = await DotNet.ListPackages()
  .Vulnerable()
  .WithFormat("json")
  .CaptureAsync();
```

### `DotNet.AddPackage()` / `DotNet.RemovePackage()`

```csharp
await DotNet.AddPackage("Newtonsoft.Json").RunAsync();
await DotNet.AddPackage("Newtonsoft.Json", "13.0.3").RunAsync();

await DotNet.AddPackage("Microsoft.Extensions.Logging")
  .WithProject("MyApp.csproj")
  .WithFramework("net10.0")
  .WithPrerelease()
  .WithSource("https://api.nuget.org/v3/index.json")
  .WithNoRestore()
  .RunAsync();

await DotNet.RemovePackage("Newtonsoft.Json").RunAsync();

await DotNet.RemovePackage("Microsoft.Extensions.Logging")
  .WithProject("MyApp.csproj")
  .RunAsync();
```

## Shared builder members

`WithWorkingDirectory`, `WithEnvironmentVariable`, and `WithNoValidation` exist on every builder in this reference.

`WithProperty` exists on Build, Clean, Restore, Run, Test, Publish, and Pack. It does not exist on ListPackages, AddPackage, RemovePackage, or Watch.

`WithTerminalLogger` emits one argument, `--tl:<mode>`, on build, restore, run, test, publish, and pack. `dotnet pack` has no framework switch. `dotnet test` data collection is `WithCollect(dataCollector)`. `dotnet dev-certs https` export is `WithExport().WithExportPath(path)` and emits `--export-path`. `dotnet nuget why` takes the project as a positional argument. `dotnet nuget delete` uses `WithNonInteractive()` and has no config-file option. `dotnet run` rejects `WithProject` together with `WithFile`.

```csharp
await DotNet.Build()
  .WithWorkingDirectory("/path/to/project")
  .WithEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
  .WithProperty("Version", "1.0.0")
  .WithNoValidation()
  .RunAsync();
```

`WithNoValidation()` is the default made explicit. DotNet builders do not expose `WithZeroExitCodeValidation()`.

Query helpers on `DotNet`:

```csharp
CommandOutput sdks = await DotNet.WithListSdks().CaptureAsync();
CommandOutput version = await DotNet.WithVersion().CaptureAsync();
```

## Error handling

```csharp
CommandOutput failed = await DotNet.Test()
  .WithProject("NonExistentProject.csproj")
  .CaptureAsync();

if (!failed.Success)
{
  int exitCode = failed.ExitCode;
  string stderr = failed.Stderr;
}
```

`CaptureAsync` still returns the real stdout, stderr, and exit code. It does not collapse failure into an empty string. `RunAsync` returns that same exit code and does not throw.

A command that never ran reports `CommandResult.NeverRanExitCode` (`-1`).

## Pipelines

Builders do not pipe directly. `Build()` returns a `CommandResult`, which does.

```csharp
CommandOutput filtered = await DotNet.ListPackages()
  .Outdated()
  .Build()
  .Pipe("grep", "Microsoft")
  .CaptureAsync();

string[] failedTests = (await DotNet.Test()
  .WithLogger("console;verbosity=detailed")
  .Build()
  .Pipe("grep", "Failed")
  .CaptureAsync()).GetLines();
```

## Microsoft documentation

Option meanings match the `dotnet` CLI:

- [dotnet run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run)
- [dotnet build](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
- [dotnet test](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [dotnet clean](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-clean)
- [dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [dotnet publish](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
- [dotnet pack](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack)
- [dotnet list package](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-list-package)
- [dotnet add package](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package)
- [dotnet remove package](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-remove-package)
