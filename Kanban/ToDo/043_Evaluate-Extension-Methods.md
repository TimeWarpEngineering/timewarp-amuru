# Evaluate Extension Methods on Internal Builders

## Description
Several builder classes in the library have extension methods defined for them, even though we fully control these classes. This should be evaluated to determine if these methods should be moved to the classes themselves.

## Current Extension Methods to Evaluate

### GhqBuilder Extensions (Ghq.Extensions.cs)
- `PipeTo()` - pipes to another command
- `SelectWithFzf()` - adds fzf selection

### GwqBuilder Extensions (Gwq.Extensions.cs)
- Similar methods (need to verify)

### FzfBuilder Extensions (Fzf.Extensions.cs)
- Extensions on `CommandResult` (may be justified since CommandResult is more generic)

## Questions to Answer
1. Why were these implemented as extensions instead of direct methods?
2. Is there any benefit to keeping them as extensions?
3. Would moving them to the classes improve discoverability and maintainability?

## Acceptance Criteria
- [ ] Review each extension method and document the rationale for its current implementation
- [ ] Determine if any should remain as extensions (with justification)
- [ ] Refactor those that should be direct methods
- [ ] Update any affected tests and documentation