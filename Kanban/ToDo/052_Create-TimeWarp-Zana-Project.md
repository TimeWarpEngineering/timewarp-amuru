# Create TimeWarp.Zana Project

## Description

Create the TimeWarp.Zana project - a private utilities library for in-process execution. Zana ("Tools" in Swahili) provides native utilities that can be called directly from C# code without spawning external processes.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Requirements

- Must reference TimeWarp.Amuru via PackageReference (not ProjectReference)
- Must be packable as a NuGet library
- Must follow existing code style and analyzer settings

## Checklist

### Project Setup
- [ ] Create `TimeWarp.Zana.csproj` with correct metadata
- [ ] Create `GlobalUsings.cs`
- [ ] Create `README.md` for package

### Initial Utilities
- [ ] Create placeholder utility class to validate build
- [ ] Verify project builds successfully
- [ ] Verify NuGet package generates correctly

## Notes

Zana is the in-process utilities library. Users can reference it directly:

```csharp
#:package TimeWarp.Zana@1.0.0

using TimeWarp.Zana;
var result = MyTool.Execute();  // Fast, no process spawn
```

Dependency chain: `Amuru (public) → Zana (private) → Ganda (private)`
