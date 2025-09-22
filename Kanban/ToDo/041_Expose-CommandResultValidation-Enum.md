# 041 Expose CommandResultValidation Enum

## Description

Currently, TimeWarp.Amuru uses CliWrap's `CommandResultValidation` enum internally but doesn't expose it in the public API. Consumers need to reference CliWrap directly to use validation options like `CommandResultValidation.None`. We need to expose this enum without requiring consumers to add a third-party dependency.

## Requirements

- Allow consumers to set validation behavior without referencing CliWrap
- Maintain backward compatibility
- Provide a clean, intuitive API
- Consider future extensibility

## Options

### Option 1: Re-export CliWrap enum (Global Using)
Add to `GlobalUsings.cs`:
```csharp
global using CommandResultValidation = CliWrap.CommandResultValidation;
```

**Pros:**
- Simple, minimal change
- Full compatibility with CliWrap API
- No maintenance overhead

**Cons:**
- Global usings don't expose types to library consumers
- Consumers still need to reference CliWrap

### Option 2: Create TimeWarp.Amuru enum
Create a new enum in TimeWarp.Amuru namespace that mirrors CliWrap's values:
```csharp
namespace TimeWarp.Amuru;

public enum CommandValidation
{
    None,
    ZeroExitCode
}
```

Then update `CommandOptions` to use this enum and convert internally.

**Pros:**
- Clean public API without CliWrap dependency
- Full control over the API surface
- Can add TimeWarp-specific validation options later

**Cons:**
- Maintenance burden if CliWrap adds new values
- Conversion logic needed
- Potential for drift between enums

### Option 3: Strongly-typed methods
Instead of exposing the enum, add convenience methods:
```csharp
public CommandOptions WithNoValidation() => // sets to None
public CommandOptions WithZeroExitCodeValidation() => // sets to ZeroExitCode
```

**Pros:**
- No enum exposure needed
- Very simple API
- Easy to extend

**Cons:**
- Less flexible for advanced users
- Can't set arbitrary validation modes
- Doesn't match CliWrap's API patterns

## Recommendation

Option 2 (create our own enum) provides the best balance of usability, maintainability, and future extensibility. It gives consumers a clean API while insulating them from CliWrap changes.

## Implementation Steps

1. Create `CommandValidation.cs` in TimeWarp.Amuru namespace
2. Update `CommandOptions.cs` to use the new enum
3. Add conversion logic in `ApplyTo()` method
4. Update documentation and examples
5. Consider adding implicit conversions for backward compatibility

## Acceptance Criteria

- [ ] Consumers can use `CommandValidation.None` without referencing CliWrap
- [ ] Existing code continues to work
- [ ] All validation behaviors work correctly
- [ ] Documentation updated with new API
- [ ] Tests cover the new enum usage

## Priority

Medium - Improves developer experience but not blocking functionality