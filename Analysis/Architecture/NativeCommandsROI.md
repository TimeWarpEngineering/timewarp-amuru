# Native Commands ROI-Based Prioritization

## Overview

This document defines the prioritization strategy for implementing native commands based on Return on Investment (ROI). Commands are organized by use case rather than namespace, focusing on real-world scripting scenarios.

## Use Case Priority Matrix

### Tier 1: Build & CI/CD (Highest ROI)
**Why**: Every project needs build scripts. These run frequently and benefit most from in-process execution.

| Command | Bash/PS Equivalent | ROI Score | Rationale |
|---------|-------------------|-----------|-----------|
| CopyItem | cp | 10 | Used in every build to stage artifacts |
| RemoveItem | rm -rf | 10 | Clean operations are universal |
| TestPath | test -e | 10 | Conditional logic in every script |
| SelectString | grep | 10 | Search code, logs, find patterns |
| ReplaceInFiles | sed -i | 9 | Version bumping, config updates |
| CompressArchive | tar/zip | 9 | Package releases, create artifacts |
| GetItemProperty | stat/ls -la | 8 | File metadata for build decisions |
| FindItem | find | 8 | Locate files with patterns |
| ConvertTo/FromJson | jq | 8 | Parse package.json, settings |

### Tier 2: Developer Workflow (High ROI)
**Why**: Improves developer experience and productivity significantly.

| Command | Bash/PS Equivalent | ROI Score | Rationale |
|---------|-------------------|-----------|-----------|
| SelectItem | fzf | 9 | Interactive file/option selection |
| GetEnvironmentVariable | env/echo $ | 8 | Read build configurations |
| SetEnvironmentVariable | export | 8 | Configure build environment |
| Which | which/where | 7 | Find tool locations |
| ResolvePath | realpath | 7 | Handle relative paths correctly |
| ReadHost | read | 6 | User input for scripts |
| ConfirmAction | read -p | 6 | Safety prompts |

### Tier 3: Data Processing (Medium ROI)
**Why**: Common in ETL, log processing, and data manipulation scripts.

| Command | Bash/PS Equivalent | ROI Score | Rationale |
|---------|-------------------|-----------|-----------|
| SortObject | sort | 7 | Order results, deduplicate |
| SelectObject | head/tail | 7 | First/last N items |
| MeasureObject | wc | 6 | Count lines, words |
| SplitString | cut/awk | 6 | Parse delimited data |
| JoinString | paste | 5 | Combine data |
| ConvertToCsv | - | 5 | Structure data for export |

### Tier 4: System Administration (Medium ROI)
**Why**: Useful for DevOps but less frequent in typical development.

| Command | Bash/PS Equivalent | ROI Score | Rationale |
|---------|-------------------|-----------|-----------|
| GetProcess | ps | 6 | Monitor running processes |
| StopProcess | kill | 6 | Clean up hung processes |
| GetSystemInfo | uname | 5 | Detect platform/architecture |
| TestConnection | ping | 4 | Network connectivity checks |
| GetHash | md5sum/sha256sum | 4 | Verify file integrity |

### Tier 5: Advanced/Specialized (Lower ROI)
**Why**: Nice to have but not critical for most scripts.

| Command | Bash/PS Equivalent | ROI Score | Rationale |
|---------|-------------------|-----------|-----------|
| WriteProgress | - | 3 | Progress bars for long operations |
| WriteHost (colored) | echo with colors | 3 | Pretty output |
| ShowMenu | select | 3 | Menu-driven interfaces |
| ExpandArchive | untar/unzip | 3 | Less common than compress |

## Implementation Phases

### Phase 1: Essential Build Commands (Sprint 1)
Focus on commands that every build script needs.

**File Operations**
- CopyItem, MoveItem, RemoveItem
- TestPath, NewItem (mkdir/touch)
- GetItemProperty (basic file info)

**Text Essentials**
- SelectString (grep) - pattern matching
- ReplaceInFiles - in-place replacement
- Simple head/tail operations

**Process Basics**
- InvokeCommand - run with exit code checking
- GetProcessOutput - capture stdout/stderr

### Phase 2: Configuration & Packaging (Sprint 2)
Commands for managing configurations and creating packages.

**Data Formats**
- ConvertTo/FromJson
- ConvertTo/FromXml (if needed)
- Basic CSV operations

**Archive Operations**
- CompressArchive (zip/tar)
- Support for common formats

**Environment**
- Get/SetEnvironmentVariable
- Platform detection helpers

### Phase 3: Interactive & Developer UX (Sprint 3)
Enhance developer experience with interactive features.

**Selection & Input**
- SelectItem (fzf-like fuzzy finder)
- ReadHost with validation
- ConfirmAction prompts

**Path Operations**
- Which (find executables)
- ResolvePath (absolute paths)
- Glob pattern matching

### Phase 4: Advanced Processing (Sprint 4)
More sophisticated data manipulation.

**Text Processing**
- Advanced regex with groups
- Awk-like field processing
- Stream processing for large files

**System Information**
- Process management
- System resource info
- Network utilities

## ROI Calculation Factors

### High ROI Indicators
1. **Frequency**: Used in >80% of scripts
2. **Performance**: >10x faster than external process
3. **Cross-platform**: Eliminates platform-specific code
4. **Composability**: Works well in pipelines
5. **Complexity Reduction**: Replaces multi-step operations

### Low ROI Indicators
1. **Rare Usage**: <10% of scripts need it
2. **Minimal Performance Gain**: External is already fast
3. **Platform Specific**: Only works on one OS
4. **Alternative Exists**: .NET already has good API
5. **Maintenance Burden**: Complex implementation for little gain

## Success Metrics

Track adoption through:
- Download stats for commands used
- GitHub issues requesting specific commands
- Performance benchmarks vs external tools
- User feedback on missing commands
- Migration stories from bash/PS to Amuru

## Cake Build Inspiration

Key patterns from Cake that apply to our native commands:

1. **Globbing**: `GetFiles("**/*.cs")` is incredibly useful
2. **Tool Resolution**: `FindToolInPath("dotnet")`
3. **Platform Abstraction**: `IsRunningOnWindows()`
4. **Path Manipulation**: `MakeAbsolute(FilePath)`
5. **Process Helpers**: `StartProcess` with settings

## Not Implementing (Low ROI)

These have low ROI for our use cases:
- Obscure Unix commands (tac, nl, fold)
- Database clients (mysql, psql)
- Complex TUI components (better as separate library)
- Network servers (nc listener mode)
- File watching (better with dedicated library)

## Conclusion

By focusing on high-ROI commands first, we can make TimeWarp.Amuru immediately valuable for:
1. Build automation scripts
2. CI/CD pipelines
3. Developer productivity tools
4. Cross-platform scripting
5. Data processing pipelines

The phased approach ensures each release adds significant value while maintaining quality and consistency.