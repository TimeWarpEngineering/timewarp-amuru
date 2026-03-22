# Build System Independence

* Status: accepted
* Architect: Steven T. Cramer
* Date: 2025-01-10

Technical Story: Ensure build scripts remain stable and avoid circular dependencies

## Context and Problem Statement

The build system needs to compile and package TimeWarp.Cli itself. If build scripts depend on the library they're building, this creates circular dependencies that can prevent builds from succeeding when the library has issues.

## Decision Drivers

* Avoid circular dependencies in build process
* Ensure build scripts remain stable and self-contained
* Prevent chicken-and-egg problems during development
* Enable builds to succeed even when library has issues
* Maintain build system reliability

## Considered Options

* Build scripts use raw System.Diagnostics.Process
* Build scripts use TimeWarp.Cli (dogfooding)
* Build scripts use external build tools
* Build scripts use different command execution library

## Decision Outcome

Chosen option: "Build scripts use raw System.Diagnostics.Process", because it ensures build system independence and prevents circular dependencies while maintaining reliability.

### Positive Consequences

* Build system remains stable and self-contained
* No circular dependencies in build process
* Builds can succeed even when library has issues
* Build scripts don't depend on library being built
* Clear separation of concerns

### Negative Consequences

* Build scripts don't demonstrate library usage
* More verbose code in build scripts
* No benefits from library improvements in build process

## Pros and Cons of the Options

### Build scripts use raw System.Diagnostics.Process

* Good, because completely independent of library
* Good, because prevents circular dependencies
* Good, because ensures build stability
* Good, because works even when library is broken
* Good, because clear separation of concerns
* Bad, because more verbose than using library
* Bad, because doesn't demonstrate library usage

### Build scripts use TimeWarp.Cli (dogfooding)

* Good, because demonstrates library usage
* Good, because benefits from library improvements
* Good, because provides real-world validation
* Good, because more concise build scripts
* Bad, because creates circular dependencies
* Bad, because builds can fail when library has issues
* Bad, because chicken-and-egg problem during development

### Build scripts use external build tools

* Good, because mature and stable tooling
* Good, because rich feature sets
* Good, because community support
* Bad, because adds external dependencies
* Bad, because requires learning additional tools
* Bad, because may not be available on all platforms

### Build scripts use different command execution library

* Good, because avoids circular dependencies
* Good, because can provide similar benefits
* Good, because proven stability
* Bad, because adds external dependencies
* Bad, because inconsistent with library philosophy
* Bad, because additional complexity

## Implementation Details

Build scripts use `System.Diagnostics.Process` directly:

```csharp
#pragma warning disable IDE0005 // Using directive is unnecessary
using System.Diagnostics;
#pragma warning restore IDE0005

// Build script example
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "build --configuration Release",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    }
};

process.Start();
process.WaitForExit();

if (process.ExitCode != 0)
{
    Environment.Exit(process.ExitCode);
}
```

## Pragmatic Warnings

The build scripts include pragma warnings to suppress IDE warnings about unnecessary using directives:

```csharp
#pragma warning disable IDE0005 // Using directive is unnecessary
using System.Diagnostics;
#pragma warning restore IDE0005
```

This is because C# script files have ImplicitUsings enabled, but `System.Diagnostics` is not included by default.

## Scope of Independence

The independence principle applies specifically to:
* `Scripts/Build.cs` - Core build script
* `Scripts/Pack.cs` - Package creation script  
* `Scripts/Clean.cs` - Cleanup script

Other scripts (tests, examples) can safely use TimeWarp.Cli once it's built and stable.

## Future Considerations

Once the library reaches maturity and stability, the build scripts could potentially be migrated to use TimeWarp.Cli for demonstration purposes. However, this should only be done when:
* The library is proven stable
* There's significant value in dogfooding
* Fallback mechanisms are in place

## Links

* Related to [ADR-0008](0008-integration-testing-strategy.md) - Integration Testing Strategy
* Contrasts with dogfooding approach used elsewhere in the project