# Create Amuru skill at skills/amuru/SKILL.md

## Description

Create the Amuru skill file in this repository (timewarp-amuru) and establish it as the source of truth. The skill will be created here first, then after it's merged to master, timewarp-flow will symlink to it.

## Checklist

Phase 1 - Create skill in timewarp-amuru (this task):
- [x] Copy SKILL.md from timewarp-flow to `skills/amuru/SKILL.md`
  - Source: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-flow/Cramer-2025-08-01-cron/opencode/skills/amuru/SKILL.md`
- [x] Review and update the copied SKILL.md as needed
  - Update package version to current
  - Verify all code examples are accurate
  - Ensure local paths point to this repo
- [x] Commit and push to master

Phase 2 - Create symlink in timewarp-flow (blocked until Phase 1 complete):
- [ ] Create task in timewarp-flow to symlink skills/amuru to this repo's master
  - Symlink target: `<timewarp-amuru-master>/skills/amuru`
  - Only doable after this PR is merged to master

## Notes

- **Source**: `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-flow/Cramer-2025-08-01-cron/opencode/skills/amuru/SKILL.md`
- **Destination**: `skills/amuru/SKILL.md` (in this repo)
- **Symlink**: Cannot be created until this skill exists in master (chicken-and-egg)
- Beta version: 1.0.0-beta.16 (or latest)
- After the skill is in master and symlink is created, timewarp-amuru becomes the authoritative source
- Future skill updates should be made in this repo, then pulled via symlink refresh

### Completion Notes

- The skill has been created at `skills/amuru/SKILL.md`
- Package version updated to 1.0.0-beta.18
- Added note that this is the authoritative source
