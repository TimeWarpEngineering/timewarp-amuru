# Directory.Packages.props Usage Analysis

**Date:** 2026-03-16  
**Scope:** Analysis of all packages in Directory.Packages.props to determine actual usage in the repository.

---

## Executive Summary

Two packages in Directory.Packages.props are **not currently used** in the codebase:
- `Blockcore.Nostr.Client` - Planned for future TimeWarp.Kijamii library (not yet implemented)
- `TimeWarp.Multiavatar` - Was extracted to its own repository (task 032 completed)

All other packages are actively used in the library, tests, tools, or build infrastructure.

---

## Detailed Findings

### ❌ Unused Packages (2)

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `Blockcore.Nostr.Client` | 2.0.1 | **Not Used** | Planned for TimeWarp.Kijamii (see kanban/031, 033) |
| `TimeWarp.Multiavatar` | 1.0.0-beta.13 | **Not Used** | Extracted to own repo per task 032 |

#### Blockcore.Nostr.Client
- **Only found in:** Kanban/documentation files
- **References:**
  - `kanban/to-do/031-create-timewarp-kijamii-library.md` - Listed as planned dependency
  - `kanban/to-do/033-extract-kijamii-to-own-repo.md` - Task to remove it
  - `kanban/in-progress/028-implement-cli-utilities-as-native-commands.md` - Mentioned as future integration
- **Recommendation:** Remove from Directory.Packages.props. Add back when TimeWarp.Kijamii is implemented.

#### TimeWarp.Multiavatar
- **Only found in:** Kanban/documentation files
- **Context:** Task 032 extracted this library to its own repository
- **References:**
  - `kanban/done/032-extract-multiavatar-to-own-repo.md` - Documents the extraction
  - `Architecture/TimeWarp-Ecosystem-Architecture.md` - Shows it as separate package
- **Recommendation:** Remove from Directory.Packages.props. The library is now a separate NuGet package.

---

### ✅ Used Packages (12)

#### Core Library Dependencies

| Package | Version | Used In |
|---------|---------|---------|
| `CliWrap` | 3.10.0 | `Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj` (line 12) |
| `StreamJsonRpc` | 2.24.84 | `Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj` (line 13) |

**Evidence:**
- `Source/TimeWarp.Amuru/GlobalUsings.cs` - `global using CliWrap;` and `global using StreamJsonRpc;`
- Core library wraps CliWrap for command execution
- StreamJsonRpc used for JSON-RPC client implementation

#### Test Dependencies

| Package | Version | Used In |
|---------|---------|---------|
| `TimeWarp.Jaribu` | 1.0.0-beta.8 | `tests/timewarp-amuru/Directory.Build.props` (line 15) |
| `Shouldly` | 4.3.0 | `tests/timewarp-amuru/Directory.Build.props` (line 17) |
| `ModelContextProtocol.Core` | 0.3.0-preview.4 | `tests/timewarp-amuru/single-file-tests/json-rpc/*.cs` |

**Evidence:**
- `tests/timewarp-amuru/Directory.Build.props` - PackageReference entries
- `tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.mcp-initialize.cs` - Uses `#:package ModelContextProtocol.Core`

#### Tool Dependencies

| Package | Version | Used In |
|---------|---------|---------|
| `TimeWarp.Nuru` | 3.0.0-beta.47 | `tools/dev-cli/Directory.Build.props` (line 62) |

**Evidence:**
- `tools/dev-cli/endpoints/*.cs` - Multiple files use `using TimeWarp.Nuru;`
- CLI routing framework for dev-cli tool

#### Build Infrastructure

| Package | Version | Used In |
|---------|---------|---------|
| `TimeWarp.Build.Tasks` | 1.0.0 | `Source/Directory.Build.props` (line 25) |

**Evidence:**
- Provides source link and package metadata MSBuild tasks

#### Code Analyzers (Applied Globally)

| Package | Version | Used In |
|---------|---------|---------|
| `Roslynator.Analyzers` | 4.15.0 | `Directory.Build.props` (line 63) |
| `Roslynator.CodeAnalysis.Analyzers` | 4.15.0 | `Directory.Build.props` (line 64) |
| `Roslynator.Formatting.Analyzers` | 4.15.0 | `Directory.Build.props` (line 65) |
| `Microsoft.CodeAnalysis.NetAnalyzers` | 10.0.103 | `Directory.Build.props` (line 66) |
| `Microsoft.CodeAnalysis.CSharp.CodeStyle` | 5.0.0 | `Directory.Build.props` (line 67) |

**Evidence:**
- All analyzers referenced in root `Directory.Build.props`
- Note: `tools/Directory.Build.props` explicitly removes Roslynator analyzers (too aggressive for tools)

---

## Recommendations

### Immediate Actions

1. **Remove `Blockcore.Nostr.Client`** from Directory.Packages.props
   - Not currently used
   - Can be re-added when TimeWarp.Kijamii is implemented
   - See task 033 checklist item: "Remove Blockcore.Nostr.Client from Directory.Packages.props"

2. **Remove `TimeWarp.Multiavatar`** from Directory.Packages.props
   - Library was extracted to its own repository (task 032 completed)
   - Now consumed as external NuGet package, not built in this repo

### Suggested Changes to Directory.Packages.props

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- External packages -->
    <!-- REMOVED: Blockcore.Nostr.Client - not used, planned for TimeWarp.Kijamii -->
    <PackageVersion Include="CliWrap" Version="3.10.0" />
    <PackageVersion Include="ModelContextProtocol.Core" Version="0.3.0-preview.4" />
    <PackageVersion Include="Shouldly" Version="4.3.0" />
    <PackageVersion Include="StreamJsonRpc" Version="2.24.84" />
    <PackageVersion Include="TimeWarp.Build.Tasks" Version="1.0.0" />
    <!-- REMOVED: TimeWarp.Multiavatar - extracted to own repository -->
    <PackageVersion Include="TimeWarp.Jaribu" Version="1.0.0-beta.8" />
    <PackageVersion Include="TimeWarp.Nuru" Version="3.0.0-beta.47" />

    <!-- Analyzers -->
    <PackageVersion Include="Roslynator.Analyzers" Version="4.15.0" />
    <PackageVersion Include="Roslynator.CodeAnalysis.Analyzers" Version="4.15.0" />
    <PackageVersion Include="Roslynator.Formatting.Analyzers" Version="4.15.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="10.0.103" />
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" Version="5.0.0" />
    
  </ItemGroup>
</Project>
```

---

## Summary Table

| Package | Used | Location |
|---------|------|----------|
| Blockcore.Nostr.Client | ❌ | Only in kanban docs |
| CliWrap | ✅ | Core library |
| ModelContextProtocol.Core | ✅ | Test scripts |
| Shouldly | ✅ | Test assertions |
| StreamJsonRpc | ✅ | Core library |
| TimeWarp.Build.Tasks | ✅ | Build infrastructure |
| TimeWarp.Multiavatar | ❌ | Only in kanban docs |
| TimeWarp.Jaribu | ✅ | Test framework |
| TimeWarp.Nuru | ✅ | dev-cli tool |
| Roslynator.Analyzers | ✅ | Global analyzers |
| Roslynator.CodeAnalysis.Analyzers | ✅ | Global analyzers |
| Roslynator.Formatting.Analyzers | ✅ | Global analyzers |
| Microsoft.CodeAnalysis.NetAnalyzers | ✅ | Global analyzers |
| Microsoft.CodeAnalysis.CSharp.CodeStyle | ✅ | Global analyzers |

**Total:** 14 packages, 12 used, 2 unused
