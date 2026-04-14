# Clean up RS0030 banned symbol audit issues

## Description

Review and resolve RS0030 banned symbol warnings throughout the repository. The project uses a banned symbol analyzer to enforce use of TimeWarp.Amuru's `Shell.Builder` instead of raw `System.Diagnostics.Process` and `ProcessStartInfo`.

## Checklist

- [x] Audit all RS0030 pragma exemptions in the codebase
- [x] Evaluate if `SshKeyHelper.cs` should use Shell.Builder instead of raw Process/ProcessStartInfo
- [x] Evaluate if `CommandResult.cs` TtyPassthroughAsync needs Process directly (TTY inheritance requirement)
- [x] Document legitimate exemptions with clear justification comments
- [x] Refactor any code that should use Amuru's API but doesn't
- [x] Consider if banned symbol configuration needs adjustment for Amuru's own source

## Notes

### Final State

Only one RS0030 pragma exemption remains in the entire codebase:

1. `source/timewarp-amuru/core/command-result.cs` — `TtyPassthroughAsync`
   - **Legitimate**: True TTY inheritance requires `Process.Start` without stream redirection; CliWrap cannot express this
   - **Documented**: `#pragma warning disable RS0030` with justification comment, plus `#region Implementation Boundaries` explaining the design decision

All other RS0030 exemptions have been eliminated:
- `SshKeyHelper.cs` — refactored to use `Shell.Builder`
- Test files (`fzf-builder.select-async.cs`, `shell-builder.select-async.cs`) — replaced `ProcessStartInfo` with `Shell.Builder`
- `tools/dev-cli/services/process-helpers.cs` — replaced `ProcessStartInfo` with `Shell.Builder` + argument parser

### Banned Symbol Configuration

No adjustment needed. The single remaining exemption in `command-result.cs` is properly justified and documented. Amuru correctly "eats its own dog food" — it uses `Shell.Builder` everywhere except for the one case where raw process APIs are architecturally required (TTY passthrough).

### Architectural Decision

Amuru should use `Shell.Builder` internally wherever possible. The only exception is `TtyPassthroughAsync`, which is an implementation boundary where raw process APIs are required because CliWrap's stream-piped model cannot preserve TTY characteristics. This is documented in `#region Implementation Boundaries` in the source file.