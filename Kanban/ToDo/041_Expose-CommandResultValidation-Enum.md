# 041 Expose CommandResultValidation Enum

## Description

Currently, TimeWarp.Amuru uses CliWrap's `CommandResultValidation` enum internally but doesn't expose it in the public API. Consumers need to reference CliWrap directly to use validation options like `CommandResultValidation.ZeroExitCode`. Since the library defaults to `None` (no validation), we only need to expose the `WithZeroExitCodeValidation()` method to provide access to strict validation.

**Issue**: There are inconsistencies in default validation behavior:
- `CommandOptions` comment claims default is `None` but actually uses CliWrap's default
- `RunBuilder` has `WithNoValidation()` but no `WithZeroExitCodeValidation()`
- Need to make defaults consistent and ensure `None` is truly the default

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

Option 3 (strongly-typed methods) is recommended for consistency with the existing API. The library already uses `WithNoValidation()` on `RunBuilder` and `CommandOptions`, so we should extend this pattern rather than introduce an enum. This maintains the fluent API design and provides a clean, discoverable interface without requiring consumers to reference CliWrap.

## Implementation Steps

1. **Establish `None` as the true default**: Update `CommandOptions.ApplyTo()` to explicitly set `CommandResultValidation.None` when no validation is specified
2. **Fix CommandOptions documentation**: Update the comment to accurately reflect that `None` is the default
3. **Add `WithZeroExitCodeValidation()` method to `CommandOptions.cs`**
4. **Add `WithZeroExitCodeValidation()` method to `RunBuilder.cs`**
5. **Update documentation and examples to show both validation options**
6. **Ensure both methods are discoverable in IntelliSense**
7. **Update tests to verify `None` is the default and `WithZeroExitCodeValidation()` works**

## Acceptance Criteria

- [ ] Consumers can use `WithZeroExitCodeValidation()` without referencing CliWrap
- [ ] Default behavior is `None` (no validation) for graceful error handling
- [ ] `WithNoValidation()` and `WithZeroExitCodeValidation()` both work correctly
- [ ] Existing code continues to work unchanged
- [ ] Documentation updated with validation method examples
- [ ] Tests verify default is `None` and `WithZeroExitCodeValidation()` enables strict validation

## Priority

Medium - Improves developer experience but not blocking functionality