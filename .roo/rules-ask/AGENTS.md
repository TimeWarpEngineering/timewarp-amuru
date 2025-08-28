# Ask Mode Rules (Non-Obvious Only)

## Documentation Context
- **Hidden documentation**: CLAUDE.md contains comprehensive project context not evident from code structure alone
- **Coding standards location**: `.ai/04-csharp-coding-standards.md` contains detailed style rules not covered by standard .editorconfig
- **Analyzer overrides**: Different analyzer settings apply in `Scripts/`, `Tests/`, `Samples/` directories (not obvious from file structure)

## Architecture Context
- **Build system architecture**: Build scripts deliberately use raw `System.Diagnostics.Process` to avoid circular dependencies with the library being built
- **Script execution model**: C# scripts use shebang lines and special argument handling (`--` prefix) that differs from standard .NET execution
- **Directory management pattern**: All scripts use `[CallerFilePath]` parameter for relative path resolution, equivalent to PowerShell's `$PSScriptRoot`

## Testing Context
- **Test runner pattern**: Tests are executable C# scripts that return exit codes, using TimeWarp.Amuru itself for execution
- **Test discovery mechanism**: Tests found via shell `find` command rather than test framework discovery
- **Package cache strategy**: `RestorePackagesPath` set to local directory for development iteration

## Development Workflow Context
- **Two-step build process**: Build then pack to local NuGet feed (not standard single-step build)
- **Local package development**: Use `#:package TimeWarp.Amuru@*-*` for latest local builds during development
- **Cache management**: `#:property RestoreNoCache true` required for fresh package downloads in development