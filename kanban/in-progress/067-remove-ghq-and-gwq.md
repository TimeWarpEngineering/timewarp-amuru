# Remove GHQ and GWQ

> GHQ (GitHub Query) and GWQ (GitHub Worktree Query) are no longer used as native capabilities are now available in amuru and zana.

## Description

Remove the GHQ and GWQ abstractions from the codebase since equivalent functionality is now available through native git commands in amuru (Git.Bare, Git.Worktree) and zana CLI capabilities.

## Requirements

- All GHQ and GWQ usage replaced with native amuru/zana equivalents
- No references to GHQ or GWQ remain in the codebase
- Tests updated accordingly

## Checklist

### Discovery
- [ ] Search for all GHQ references and usage patterns
- [ ] Search for all GWQ references and usage patterns
- [ ] Identify which native amuru/zana replacements cover each use case

### Removal
- [ ] Remove GHQ class/interface definitions
- [ ] Remove GWQ class/interface definitions
- [ ] Update any consumers to use native alternatives
- [ ] Remove related tests

### Validation
- [ ] Build passes successfully
- [ ] Existing tests pass
- [ ] No references to GHQ or GWQ in codebase

## Notes

GHQ and GWQ were previously used for:
- GitHub-related queries and worktree operations

These are now superseded by:
- `Amuru.Git.Bare` for bare repo operations
- `Amuru.Git.Worktree` for worktree operations
- Zana CLI native commands for GitHub integration

## Implementation Notes
