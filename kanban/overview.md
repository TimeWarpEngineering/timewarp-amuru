# Kanban Board

This Kanban board manages and tracks epics and tasks for TimeWarp.Cli using a simple folder structure.
Each item is represented by a Markdown file, and the status of the item is indicated by the folder it is in.

## Folders

1. **Backlog**: Contains tasks that are not yet ready to be worked on. These tasks have a temporary backlog scoped unique identifier.
   a. **Scratch** - contains epics or tasks that are works in progress or ideas.  They can be stored in folders under users names if needed.
2. **ToDo**: Contains tasks that are ready to be worked on. When a task from the Backlog becomes ready, it is assigned a unique identifier and moved to this folder.
3. **InProgress**: Contains tasks that are currently being worked on.
4. **Done**: Contains tasks that have been completed.

## File Naming Convention

### Tasks
- For tasks in the Backlog folder, use a short description with a 'B' prefix followed by a three-digit identifier,
such as `B001_research-caching-strategies.md` or `B002_design-pipeline-optimizations.md`.
- When a task becomes "Ready," assign it a unique identifier (without the 'B' prefix) and move it to the ToDo folder, e.g.,
`001_implement-concurrent-execution-tests.md` or `002_add-long-output-validation.md`.
- <3 digit Id>_<short-description-separated-by-hyphens>

### Depth
001_top-level.md
001_001_second-level.md
001_001_001_third-level.md
001_002_second-level.md
002_top-level.md

## Definition of Ready

Before moving a task from Backlog to ToDo, ensure it meets these criteria:

### Library Feature
- [ ] Requirements and acceptance criteria defined clearly
- [ ] Impact on existing API surface understood
- [ ] Backward compatibility considered
- [ ] Dependencies identified and available

### Testing Enhancement  
- [ ] Test scenarios clearly defined
- [ ] Test data or commands identified
- [ ] Expected behaviors documented
- [ ] Integration with existing test suite planned

## Definition of Done

Tasks are considered complete when they meet the appropriate criteria:

### Library Feature

**Implementation:**
- [ ] *Core functionality implemented (required)
- [ ] *Public API additions/changes (required if applicable)
- [ ] Configuration options added (if applicable)
- [ ] Error handling implemented
- [ ] *Backward compatibility maintained (required)

**Testing:**
- [ ] *Integration tests added/updated (required)
- [ ] Edge case scenarios tested
- [ ] Performance impact validated (if applicable)
- [ ] Cross-platform compatibility verified

**Documentation:**
- [ ] *API documentation updated (required for public changes)
- [ ] Usage examples provided
- [ ] Architectural decisions documented (ADRs if significant)
- [ ] CLAUDE.md updated if needed

### Testing Enhancement

**Implementation:**
- [ ] *Test cases implemented (required)
- [ ] Test utilities created (if needed)
- [ ] *Test execution automated (required)

**Validation:**
- [ ] *Tests pass consistently (required)
- [ ] Tests provide meaningful coverage
- [ ] Tests catch intended edge cases
- [ ] Test performance acceptable

**Documentation:**
- [ ] Test purpose and coverage documented
- [ ] Test data/scenarios explained

### Documentation/Process

**Implementation:**
- [ ] *Documentation created/updated (required)
- [ ] Examples provided (if applicable)
- [ ] Cross-references updated

**Validation:**
- [ ] *Documentation accuracy verified (required)
- [ ] Examples tested and functional
- [ ] Stakeholder review completed (if applicable)

*Items marked with `*` are required. Others are optional based on feature needs.*

## Workflow

1. Create an item in the Backlog folder with a short description as the filename
2. When an item meets Definition of Ready criteria, assign it a unique identifier and move it to the ToDo folder
3. As you work on items, move them to the InProgress folder
4. When an item meets Definition of Done criteria, move it to the Done folder
5. Update Implementation notes as work is being done with pertinent information or references

## Task Examples

See `Kanban/TaskExamples/` for detailed examples of well-structured task specifications using narrative format with acceptance criteria.