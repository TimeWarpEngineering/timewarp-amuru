# Update to latest TimeWarp.Nuru and adopt Endpoints API

Update the TimeWarp.Nuru package dependency and migrate from the fluent DSL pattern to the new class-based Endpoints API.

## Current State
- Current version: 3.0.0-beta.28 (latest)
- Using fluent DSL with `.Map()` calls

## Target State
- Use class-based Endpoints with `[NuruRoute]` attributes
- Use `.DiscoverEndpoints()` for source generator discovery
- Define commands as classes with handler interfaces

## Checklist

- [ ] Verify current version is 3.0.0-beta.28 or later
- [ ] Review current route definitions in source files
- [ ] Create endpoint classes for each existing route:
  - `[NuruRoute]` attribute for route pattern
  - `[Parameter]` for positional arguments
  - `[Option]` for named options
  - Nested Handler class implementing `ICommandHandler<T, TResult>` or `IQueryHandler<T, TResult>`
- [ ] Add `.DiscoverEndpoints()` call to NuruApp builder
- [ ] Remove fluent DSL `.Map()` calls
- [ ] Update Program.cs to use new endpoint pattern
- [ ] Test all commands work correctly
- [ ] Verify build succeeds
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
