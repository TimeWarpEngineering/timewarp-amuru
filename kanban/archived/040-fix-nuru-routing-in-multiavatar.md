# 040 Fix Nuru Routing in multiavatar.cs

## Description

The multiavatar.cs command doesn't accept input due to how Nuru handles route patterns with optional flags. When flags are included in the route pattern, they become required for the route to match.

## Problem

Current route pattern:
```csharp
"{input|Text to generate avatar from} " +
"--output,-o {file?|Save SVG to file} " +
"--no-env|Generate without environment circle " +
"--output-hash|Display hash details"
```

This pattern REQUIRES all flags to be present for the route to match. Running `multiavatar test@example.com` fails with "No matching command found".

## Root Cause

In Nuru's route pattern syntax:
- When you include option flags in the pattern, they become REQUIRED parts of the pattern
- The `?` in `{file?}` only makes the VALUE optional, not the FLAG itself
- There's no syntax for making flag groups optional

## Potential Solutions

### Option 1: Multiple Routes
Create separate routes for different flag combinations:
- Base route: `{input}`
- With output: `{input} --output,-o {file}`
- With no-env: `{input} --no-env`
- etc.

**Pros:** Works with current Nuru
**Cons:** Exponential route explosion with multiple flags

### Option 2: Fix Nuru
Enhance Nuru to support optional flag groups, perhaps with syntax like:
- `{input} [--output,-o {file?}]` (brackets for optional group)
- Or make flags optional by default when they have optional values

**Pros:** Clean, maintainable route definitions
**Cons:** Requires changes to Nuru library

### Option 3: Custom Argument Parser
Use a single route `{args*}` and parse arguments manually.

**Pros:** Full control
**Cons:** Loses Nuru's benefits

## Recommendation

This appears to be a design limitation in Nuru that affects usability of commands with optional flags. The best long-term solution is Option 2 - enhancing Nuru to properly support optional flags.

## Acceptance Criteria

- [ ] `multiavatar test@example.com` works (generates SVG to stdout)
- [ ] `multiavatar test@example.com -o file.svg` works
- [ ] `multiavatar test@example.com --no-env` works
- [ ] `multiavatar test@example.com --output-hash` works
- [ ] All flag combinations work as expected
- [ ] Help text accurately reflects available options

## Related Files

- exe/multiavatar.cs
- exe/generate-avatar.cs (may have same issue)
- Tests/exe/multiavatar.Tests.cs

## Priority

High - This breaks a user-facing command that was previously working