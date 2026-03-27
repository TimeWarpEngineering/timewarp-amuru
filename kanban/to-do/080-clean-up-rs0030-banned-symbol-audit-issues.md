# Clean up RS0030 banned symbol audit issues

## Description

Review and resolve RS0030 banned symbol warnings throughout the repository. The project uses a banned symbol analyzer to enforce use of TimeWarp.Amuru's `Shell.Builder` instead of raw `System.Diagnostics.Process` and `ProcessStartInfo`. However, some files have received pragma exemptions that may need architectural review.

## Checklist

- [ ] Audit all RS0030 pragma exemptions in the codebase
- [ ] Evaluate if `SshKeyHelper.cs` should use Shell.Builder instead of raw Process/ProcessStartInfo
- [ ] Evaluate if `CommandResult.cs` TtyPassthroughAsync needs Process directly (TTY inheritance requirement)
- [ ] Document legitimate exemptions with clear justification comments
- [ ] Refactor any code that should use Amuru's API but doesn't
- [ ] Consider if banned symbol configuration needs adjustment for Amuru's own source

## Notes

### Current State (2026-03-27)

Files with RS0030 pragma exemptions:
1. `source/timewarp-amuru/core/CommandResult.cs` - TtyPassthroughAsync method
   - Uses Process directly for TTY inheritance (no stream redirection)
   - May be legitimate: TTY passthrough requires Process.Start without CliWrap's stream piping

2. `source/timewarp-amuru/native/utilities/SshKeyHelper.cs` - 6 methods
   - Uses Process directly for synchronous ssh-keygen execution
   - Should likely be refactored to use Shell.Builder

### Architectural Questions

- Should Amuru's own source be exempt from the banned symbol analyzer entirely?
- Or should Amuru "eat its own dog food" and use Shell.Builder internally?
- Is there a pattern where certain low-level operations legitimately need Process directly?

### Related

- The banned symbol analyzer enforces: `Use TimeWarp.Amuru Shell.Builder instead`
- See the 'amuru' skill for usage patterns
