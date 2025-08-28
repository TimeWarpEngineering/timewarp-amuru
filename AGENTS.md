# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build/Test Commands
- **Single test execution**: `./Tests/RunTests.cs` - runs executable C# scripts in `Tests/Integration/` directory
- **Build workflow**: Run `./Scripts/Build.cs` then `./Scripts/Pack.cs` (two-step process for local NuGet feed)
- **Local development**: Use `#:package TimeWarp.Amuru@*-*` and `#:property RestoreNoCache true` in scripts for fresh package downloads

## Non-Obvious Patterns
- **Build scripts avoid circular dependency**: Use raw `System.Diagnostics.Process` instead of TimeWarp.Amuru library
- **C# script execution**: `.cs` files get `--` prefix inserted before arguments to prevent dotnet interception
- **Directory management**: Scripts use `[CallerFilePath]` pattern for relative path resolution from script location
- **Test discovery**: Tests found via `find Integration/ -name "*.cs" -type f` command
- **Package cache**: `RestorePackagesPath` set to `LocalNuGetCache/` for development iteration
- **Pragma warnings**: `#pragma warning disable IDE0005` required for `using System.Diagnostics` in scripts
- **Analyzer overrides**: Different analyzer settings in `Scripts/`, `Tests/`, `Samples/` directories

## Code Style Rules
See `.ai/04-csharp-coding-standards.md` and `.editorconfig` for project-specific formatting requirements.

## Related AI Assistant Rules
- See `CLAUDE.md` for comprehensive project documentation and workflow guidance