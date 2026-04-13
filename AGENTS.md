# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build/Test Commands
- **Single test execution**: `./Tests/RunTests.cs` - runs executable C# scripts in `Tests/Integration/` directory
- **Build workflow**: `dotnet build` or use `./tools/dev-cli/dev.cs build`
- **Local development**: Use `#:package TimeWarp.Amuru@*-*` and `#:property RestoreNoCache true` in scripts for fresh package downloads

## Non-Obvious Patterns
- **Build scripts and dev-cli should use TimeWarp.Amuru**: Prefer `Shell.Builder` over raw `System.Diagnostics.Process`; only use raw process APIs in rare implementation boundaries like true TTY passthrough
- **C# script execution**: `.cs` files get `--` prefix inserted before arguments to prevent dotnet interception
- **Directory management**: Scripts use `[CallerFilePath]` pattern for relative path resolution from script location
- **Test discovery**: Tests found via `find Integration/ -name "*.cs" -type f` command
- **Pragma warnings**: `#pragma warning disable IDE0005` required for `using System.Diagnostics` in scripts
- **Analyzer overrides**: Different analyzer settings in `tests/`, `samples/` directories

## Code Style Rules
See `.ai/04-csharp-coding-standards.md` and `.editorconfig` for project-specific formatting requirements.
