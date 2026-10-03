# Implement Claude Fluent API

## Description

Create a strongly typed fluent API wrapper for the `claude` CLI command, following the same pattern as the existing GwqCommand implementation. This will provide type-safe, IntelliSense-friendly access to all claude command options and subcommands.

## Requirements

- Follow the existing pattern established by GwqCommand with partial classes split by command/feature
- Support all major claude CLI options and subcommands
- Maintain consistency with existing code style and architecture
- Include comprehensive integration tests

## Checklist

### Implementation
- [ ] Create Claude.cs with static entry point and base builder class
- [ ] Create Claude.Options.cs with all main command options (--debug, --print, --model, etc.)
- [ ] Create Claude.ConfigCommand.cs for config subcommand
- [ ] Create Claude.McpCommand.cs for MCP server management
- [ ] Create Claude.MigrateInstallerCommand.cs for migrate-installer command
- [ ] Create Claude.SetupTokenCommand.cs for setup-token command
- [ ] Create Claude.DoctorCommand.cs for doctor command
- [ ] Create Claude.UpdateCommand.cs for update command
- [ ] Create Claude.InstallCommand.cs for install command
- [ ] Create Claude.Extensions.cs with extension methods
- [ ] Verify all commands build correct command strings

### Documentation
- [ ] Add XML documentation to all public methods
- [ ] Update CLAUDE.md with usage examples

### Testing
- [ ] Create comprehensive integration tests in Tests/Integration/ClaudeCommand/
- [ ] Test command string generation for all options and subcommands
- [ ] Verify method chaining works correctly

## Notes

The claude command has the following structure from `claude --help`:
- Main options: debug, verbose, print, output-format, input-format, mcp-debug, permissions, tools, mcp-config, system-prompt, permission-mode, continue, resume, model, fallback-model, settings, add-dir, ide, strict-mcp-config, session-id
- Subcommands: config, mcp, migrate-installer, setup-token, doctor, update, install

Each command file should follow the partial class pattern used in GwqCommand for consistency and maintainability.