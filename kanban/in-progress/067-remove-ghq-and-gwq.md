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

### Implementation Plan

#### Files to Delete

##### GhqCommand Directory (7 files)
- Source/TimeWarp.Amuru/GhqCommand/Ghq.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.CreateCommand.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.Extensions.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.GetCommand.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.ListCommand.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.RemoveCommand.cs
- Source/TimeWarp.Amuru/GhqCommand/Ghq.RootCommand.cs

##### GwqCommand Directory (10 files)
- Source/TimeWarp.Amuru/GwqCommand/Gwq.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.AddCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.ConfigCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.ExecCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.Extensions.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.GetCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.ListCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.OtherCommands.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.RemoveCommand.cs
- Source/TimeWarp.Amuru/GwqCommand/Gwq.StatusCommand.cs

##### Documentation Files to Update (7 files)
1. Documentation/Conceptual/ArchitecturalLayers.md - Remove ghq/gwq from wrapper list
2. Documentation/Developer/ProgressiveEnhancementPattern.md - Remove ghq mentions
3. Analysis/Architecture/FinalConsensus.md - Remove hybrid ghq example reference
4. Analysis/Architecture/ResponseToAIFeedback.md - Remove ghq pattern mentions
5. Analysis/Architecture/BuilderExtensibility.md - Remove ghq code examples

##### Native Replacements
- ghq get <repo> → Git.CloneBareAsync()
- gwq add <branch> → Git.WorktreeAddAsync()
- gwq add -b <branch> → Git.WorktreeAddNewBranchAsync()
- gwq remove <worktree> → Git.WorktreeRemoveAsync()
- gwq list → Git.WorktreeListPorcelainAsync()
- ghq list → Native Shell: Shell.Builder("ghq", "list")...

##### Execution Steps
1. Delete GhqCommand directory
2. Delete GwqCommand directory
3. Update documentation files
4. Build and validate
5. Verify no GHQ/GWQ references remain

## Implementation Notes
