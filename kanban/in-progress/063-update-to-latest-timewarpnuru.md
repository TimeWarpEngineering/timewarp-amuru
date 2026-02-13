# Update to latest TimeWarp.Nuru and adopt Endpoints API

Update the TimeWarp.Nuru package dependency and migrate from the fluent DSL pattern to the new class-based Endpoints API.

## Current State
- Current version: 2.1.0-beta.44
- Using fluent DSL with `.Map()` calls
- Need to upgrade to 3.x series for Endpoints API support

## Target State
- Version: 3.0.0-beta.47 (latest as of 2026-02-13)
- Use class-based Endpoints with `[NuruRoute]` attributes
- Use `.DiscoverEndpoints()` for source generator discovery
- Define commands as classes with handler interfaces

## Implementation Plan

### Step 1: Update Package Versions
- [ ] Update Directory.Packages.props:
  - TimeWarp.Nuru: 2.1.0-beta.44 → 3.0.0-beta.47
  - Add any required analyzers/source generators
  - Update TimeWarp.Nuru.Mcp to match (if present)

### Step 2: Explore Nuru Repo Structure
- [ ] Reference TimeWarp.Nuru examples for endpoint class patterns
- [ ] Identify standard endpoint file organization

### Step 3: Create Endpoint Classes
Create endpoint classes for each dev.cs command:
- [ ] BuildCommand (ICommand<Unit>) - maps to `build` command
- [ ] TestCommand (ICommand<Unit>) - maps to `test` command
- [ ] CleanCommand (ICommand<Unit>) - maps to `clean` command
- [ ] SelfInstallCommand (ICommand<Unit>) - maps to `self-install` command
- [ ] CheckVersionQuery (IQuery<Unit>) - maps to `--version` option
- [ ] HelpQuery (IQuery<Unit>) - maps to `--help` option

Each endpoint class will have:
- [NuruRoute("pattern")] attribute
- [Parameter]/[Option] properties as needed
- Nested Handler class implementing appropriate interface

### Step 4: Update dev.cs
- [ ] Replace switch/case with NuruApp builder
- [ ] Add .DiscoverEndpoints() call
- [ ] Remove manual argument parsing

### Step 5: Add to Solution
- [ ] Add tools/dev-cli/dev-cli.csproj to the .sln file

### Step 6: Build and Test
- [ ] Verify build succeeds
- [ ] Test all commands work correctly
- [ ] Commit changes

## Notes

**Endpoints API requires:**
1. `NuruApp.CreateBuilder().DiscoverEndpoints().Build()`
2. Classes decorated with `[NuruRoute("pattern")]`
3. Properties with `[Parameter]` and `[Option]` attributes
4. Nested `Handler` class implementing handler interface
5. Constructor injection supported in handlers

**Migration pattern:**
```csharp
// Old (fluent DSL):
builder.Map("greet {name}").WithHandler((string name) => ...).AsCommand().Done();

// New (endpoints):
[NuruRoute("greet {name}")]
public sealed class GreetCommand : ICommand<Unit>
{
  [Parameter] public string Name { get; set; } = string.Empty;

  public sealed class Handler : ICommandHandler<GreetCommand, Unit>
  {
    public ValueTask<Unit> Handle(GreetCommand cmd, CancellationToken ct) { ... }
  }
}
```
