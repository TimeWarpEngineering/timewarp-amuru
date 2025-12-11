# Ganda Migration Analysis: Moving to Separate Private Repository

## Executive Summary

Moving TimeWarp.Ganda to a separate private repository involves extracting the CLI tool and creating a new private utilities library (TimeWarp.Zana). The private `timewarp-ganda` repo will contain two projects: **Zana** (in-process tools library) and **Ganda** (CLI wrapper). Public utilities remain in Amuru; private utilities go in Zana.

## Package Naming Convention

| Package | Swahili Meaning | Purpose | Visibility |
|---------|-----------------|---------|------------|
| `TimeWarp.Amuru` | "Command" | Public shell execution library | Public |
| `TimeWarp.Zana` | "Tools" | Private native utilities (in-process) | Private |
| `TimeWarp.Ganda` | "Shell" | Private CLI tool (out-of-process) | Private |

## Scope

**Source Location**: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/Cramer-2025-12-06-dev`  
**Target Location**: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-ganda/Cramer-2025-12-06-dev`

## Target Architecture

```
timewarp-ganda/                       # Private repo (your shell toolkit)
├── Source/
│   ├── TimeWarp.Zana/                # "Tools" - Private utilities library
│   │   ├── Utilities/
│   │   │   ├── MyPrivateTool.cs
│   │   │   └── AnotherTool.cs
│   │   └── TimeWarp.Zana.csproj      # References Amuru
│   │
│   └── TimeWarp.Ganda/               # "Shell" - CLI tool
│       ├── Program.cs                # Nuru routes
│       ├── GlobalUsings.cs
│       └── TimeWarp.Ganda.csproj     # References Zana (ProjectReference)
│
├── exe/                              # Private standalone runfiles
│   └── my-private-tool.cs
│
├── assets/
│   └── logo.png
│
├── Directory.Build.props
├── Directory.Packages.props
├── nuget.config
├── .editorconfig
└── .github/
    └── workflows/
        └── ci-cd.yml                 # Publishes both Zana and Ganda packages
```

## Dependency Chain

```
TimeWarp.Amuru (PUBLIC - NuGet.org)
       ↑
TimeWarp.Zana (PRIVATE) ← References Amuru for Shell.Builder(), etc.
       ↑
TimeWarp.Ganda (PRIVATE) ← CLI wraps Zana utilities via ProjectReference
```

## Usage Patterns

### In-Process (Fast, Type-Safe)

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru@1.0.0
#:package TimeWarp.Zana@1.0.0   // From private feed

using TimeWarp.Zana.Utilities;

var result = MyPrivateTool.DoThing();  // Direct call, no process spawn
```

### Out-of-Process (CLI)

```bash
timewarp my-private-command --flag
```

## Current Architecture (Before Migration)

```
timewarp-amuru/
├── Source/
│   ├── TimeWarp.Amuru/           # Public library (stays)
│   │   ├── Core/                 # Shell execution core
│   │   ├── Native/
│   │   │   ├── Utilities/        # Public utilities (stay here)
│   │   │   │   ├── ConvertTimestamp.cs
│   │   │   │   ├── GenerateColor.cs
│   │   │   │   ├── Post.cs
│   │   │   │   └── SshKeyHelper.cs
│   │   │   ├── FileSystem/       # Native file operations
│   │   │   └── Aliases/          # Bash aliases
│   │   └── Installer.cs          # Downloads from GitHub releases
│   │
│   └── TimeWarp.Ganda/           # CLI tool (to migrate)
│       ├── Program.cs            # Nuru-based CLI routes
│       ├── TimeWarp.Ganda.csproj
│       ├── GlobalUsings.cs
│       └── readme.md
│
├── exe/                          # Standalone runfiles
│   ├── convert-timestamp.cs
│   ├── generate-avatar.cs
│   └── ... (13 total)
│
└── .github/workflows/ci-cd.yml   # Builds both packages + executables
```

## What Stays vs What Moves

### Stays in Amuru (Public)

| Component | Reason |
|-----------|--------|
| `Shell.Builder()` | Core execution API |
| `DotNet`, `Fzf`, `Ghq`, `Gwq` builders | Generic command builders |
| `Native/Utilities/*` | Generic public utilities |
| `Native/FileSystem/*` | File operations |
| `Installer.cs` | Downloads from Amuru releases |
| `exe/` (public utilities) | Public standalone tools |

### Moves to Ganda Repo (Private)

| Component | Target Project | Reason |
|-----------|----------------|--------|
| `Source/TimeWarp.Ganda/` | `TimeWarp.Ganda` | CLI tool |
| (new) Private utilities | `TimeWarp.Zana` | In-process private tools |
| (new) Private runfiles | `exe/` | Private standalone utilities |

---

## Migration Checklist

### Phase 1: Setup Target Repository Structure

- [ ] Create directory structure:
  ```
  timewarp-ganda/
  ├── Source/
  │   ├── TimeWarp.Zana/
  │   └── TimeWarp.Ganda/
  ├── exe/
  └── assets/
  ```
- [ ] Copy/adapt `Directory.Build.props` (root level)
- [ ] Copy/adapt `Directory.Packages.props`
- [ ] Copy `.editorconfig`
- [ ] Create `nuget.config` with NuGet.org + private feed sources

### Phase 2: Create TimeWarp.Zana Project

- [ ] Create `Source/TimeWarp.Zana/TimeWarp.Zana.csproj`
- [ ] Create `Source/TimeWarp.Zana/GlobalUsings.cs`
- [ ] Add initial private utility classes
- [ ] Reference `TimeWarp.Amuru` via PackageReference

### Phase 3: Migrate TimeWarp.Ganda Project

- [ ] Copy `Source/TimeWarp.Ganda/` to target
- [ ] Update `TimeWarp.Ganda.csproj`:
  - Change Amuru `ProjectReference` to `PackageReference`
  - Add `ProjectReference` to Zana
- [ ] Update `GlobalUsings.cs`:
  - Add `global using TimeWarp.Zana;`
- [ ] Copy/create `readme.md` at repo root
- [ ] Copy `assets/` directory (logo, etc.)

### Phase 4: CI/CD Setup

- [ ] Create `.github/workflows/ci-cd.yml`
- [ ] Configure to publish both packages:
  - `TimeWarp.Zana` (library)
  - `TimeWarp.Ganda` (dotnet tool)
- [ ] Configure private NuGet feed (GitHub Packages)
- [ ] Set up secrets

### Phase 5: Cleanup Source Repository

- [ ] Remove `Source/TimeWarp.Ganda/` from timewarp-amuru
- [ ] Update timewarp-amuru CI/CD to not build Ganda
- [ ] Update timewarp-amuru readme

---

## Files to Create in Target Repository

### 1. `Directory.Build.props` (root)

```xml
<Project>
  <PropertyGroup Label="Custom Repository Variables">
    <RepositoryName>timewarp-ganda</RepositoryName>
    <RepositoryRoot>$(MSBuildThisFileDirectory)</RepositoryRoot>
    <SourceDirectory>$(RepositoryRoot)Source/</SourceDirectory>
    <ArtifactsDirectory>$(RepositoryRoot)artifacts/</ArtifactsDirectory>
    <LocalNuGetFeed>$(ArtifactsDirectory)packages/</LocalNuGetFeed>
    <LocalNuGetCache>$(RepositoryRoot)LocalNuGetCache/</LocalNuGetCache>
  </PropertyGroup>

  <PropertyGroup Label="MSBuild/NuGet Configuration">
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageOutputPath>$(LocalNuGetFeed)</PackageOutputPath>
    <RestorePackagesPath>$(LocalNuGetCache)</RestorePackagesPath>
    <SuppressNETCoreSdkPreviewMessage>true</SuppressNETCoreSdkPreviewMessage>
  </PropertyGroup>

  <PropertyGroup Label="Project Defaults">
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <PropertyGroup Label="Code Quality">
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningLevel>5</WarningLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisMode>All</AnalysisMode>
    <AnalysisLevel>latest-all</AnalysisLevel>
    <NoWarn>$(NoWarn);CA1303</NoWarn>
  </PropertyGroup>

  <ItemGroup Label="Code Analyzers">
    <PackageReference Include="Roslynator.Analyzers" PrivateAssets="all" />
    <PackageReference Include="Roslynator.CodeAnalysis.Analyzers" PrivateAssets="all" />
    <PackageReference Include="Roslynator.Formatting.Analyzers" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

### 2. `Directory.Packages.props`

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- TimeWarp packages (public) -->
    <PackageVersion Include="TimeWarp.Amuru" Version="1.0.0-beta.15" />
    <PackageVersion Include="TimeWarp.Multiavatar" Version="1.0.0-beta.13" />
    <PackageVersion Include="TimeWarp.Nuru" Version="2.1.0-beta.21" />

    <!-- Analyzers -->
    <PackageVersion Include="Roslynator.Analyzers" Version="4.14.1" />
    <PackageVersion Include="Roslynator.CodeAnalysis.Analyzers" Version="4.14.1" />
    <PackageVersion Include="Roslynator.Formatting.Analyzers" Version="4.14.1" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" Version="4.14.0" />
  </ItemGroup>
</Project>
```

### 3. `Source/Directory.Build.props`

```xml
<Project>
  <!-- Import parent Directory.Build.props -->
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />

  <!-- Package metadata for Zana and Ganda -->
  <PropertyGroup Label="Package Metadata">
    <IsPackable>true</IsPackable>
    <Version>1.0.0-beta.1</Version>
    <Authors>Steven T. Cramer</Authors>
    <RepositoryUrl>https://github.com/TimeWarpEngineering/timewarp-ganda</RepositoryUrl>
    <PackageLicenseExpression>Unlicense</PackageLicenseExpression>
    <PackageIcon>logo.png</PackageIcon>
  </PropertyGroup>

  <ItemGroup>
    <None Include="$(RepositoryRoot)assets/logo.png" Pack="true" PackagePath="" />
  </ItemGroup>
</Project>
```

### 4. `Source/TimeWarp.Zana/TimeWarp.Zana.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <PackageId>TimeWarp.Zana</PackageId>
    <Title>TimeWarp Zana - Private Tools Library</Title>
    <Description>Private native utilities (Zana = "Tools" in Swahili) for in-process execution. Part of the TimeWarp shell toolkit.</Description>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>

  <ItemGroup>
    <!-- Public dependency -->
    <PackageReference Include="TimeWarp.Amuru" />
    
    <None Include="README.md" Pack="true" PackagePath="" />
  </ItemGroup>

</Project>
```

### 5. `Source/TimeWarp.Zana/GlobalUsings.cs`

```csharp
global using System;
global using TimeWarp.Amuru;
```

### 6. `Source/TimeWarp.Zana/README.md`

```markdown
# TimeWarp.Zana

*Zana means "Tools" in Swahili*

Private native utilities library for in-process execution. Part of the TimeWarp shell toolkit.

## Usage

```csharp
#:package TimeWarp.Zana@1.0.0

using TimeWarp.Zana;

// Call utilities directly (in-process, fast)
var result = MyTool.Execute();
```

## Related Packages

- **TimeWarp.Amuru** - Public shell execution library
- **TimeWarp.Ganda** - CLI tool wrapping Zana utilities
```

### 7. `Source/TimeWarp.Ganda/TimeWarp.Ganda.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <PackageReadmeFile>README.md</PackageReadmeFile>

    <!-- .NET Tool Configuration -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>timewarp</ToolCommandName>
    <PackageId>TimeWarp.Ganda</PackageId>
    <Title>TimeWarp Ganda Shell Toolkit</Title>
    <Description>A private shell toolkit (Ganda = "Shell" in Swahili) providing CLI utilities.</Description>
  </PropertyGroup>

  <ItemGroup>
    <!-- Zana (in same repo - ProjectReference) -->
    <ProjectReference Include="../TimeWarp.Zana/TimeWarp.Zana.csproj" />
    
    <!-- Public packages -->
    <PackageReference Include="TimeWarp.Amuru" />
    <PackageReference Include="TimeWarp.Multiavatar" />
    <PackageReference Include="TimeWarp.Nuru" />

    <None Include="README.md" Pack="true" PackagePath="" />
  </ItemGroup>

</Project>
```

### 8. `Source/TimeWarp.Ganda/GlobalUsings.cs`

```csharp
global using System;
global using TimeWarp.Amuru;
global using TimeWarp.Amuru.Native.Utilities;  // Public utilities from Amuru
global using TimeWarp.Zana;                     // Private utilities
global using TimeWarp.Multiavatar;
global using TimeWarp.Nuru;
global using static System.Console;
```

### 9. `nuget.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/TimeWarpEngineering/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="USERNAME" />
      <add key="ClearTextPassword" value="TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

---

## CI/CD Configuration

### `.github/workflows/ci-cd.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]
  release:
    types: [published]

permissions:
  contents: write
  packages: write

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'

      - name: Build
        run: |
          dotnet build Source/TimeWarp.Zana/TimeWarp.Zana.csproj -c Release
          dotnet build Source/TimeWarp.Ganda/TimeWarp.Ganda.csproj -c Release

      - name: Upload packages
        uses: actions/upload-artifact@v4
        with:
          name: packages
          path: artifacts/packages/*.nupkg

  publish:
    if: github.event_name == 'release'
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Download packages
        uses: actions/download-artifact@v4
        with:
          name: packages
          path: packages

      - name: Publish to GitHub Packages
        run: |
          dotnet nuget push "packages/*.nupkg" \
            --api-key ${{ secrets.GITHUB_TOKEN }} \
            --source "https://nuget.pkg.github.com/TimeWarpEngineering/index.json" \
            --skip-duplicate
```

---

## Summary

| Aspect | Decision |
|--------|----------|
| **Repo structure** | Single repo (`timewarp-ganda`) with two projects |
| **Zana location** | `Source/TimeWarp.Zana/` in Ganda repo |
| **Package output** | Two packages: `TimeWarp.Zana` + `TimeWarp.Ganda` |
| **Zana → Ganda reference** | `ProjectReference` (same repo) |
| **Amuru reference** | `PackageReference` (from NuGet.org) |
| **Private feed** | GitHub Packages |

**Key benefit**: One repo, one CI/CD pipeline, atomic versioning. Zana provides in-process utilities; Ganda wraps them as CLI commands.

---

## References

- Current Ganda project: `Source/TimeWarp.Ganda/`
- CI/CD workflow: `.github/workflows/ci-cd.yml`
- Package versions: `Directory.Packages.props`
- Public utilities (stay in Amuru): `Source/TimeWarp.Amuru/Native/Utilities/`
