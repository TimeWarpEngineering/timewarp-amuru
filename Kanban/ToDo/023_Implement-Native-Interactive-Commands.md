# 023 Implement Native Interactive Commands

## Description

Implement native interactive user interface commands in the Native/Interactive namespace using PowerShell-inspired naming with bash aliases. These commands will provide in-process interactive selection, input prompts, and colored output without requiring external tools like fzf.

## Requirements

- Create interactive UI commands with both Commands and Direct APIs
- Follow PowerShell verb-noun naming convention
- Provide bash aliases for familiar command names
- Build FZF-like fuzzy finder functionality natively
- Support colored and formatted console output
- Maintain consistency with existing implementations

## Checklist

### Interactive Namespace Implementation
- [ ] Create Native/Interactive/Commands.cs
  - [ ] Static class Commands with shell semantics (returns CommandOutput)
  - [ ] Implement SelectItem(string[] items, string? prompt = null) - fuzzy selection
  - [ ] Implement ReadHost(string prompt, bool secure = false) - user input
  - [ ] Implement WriteHost(string message, ConsoleColor? color = null) - colored output
  - [ ] Implement WriteProgress(int percent, string? message = null) - progress bar
  - [ ] Implement ConfirmAction(string message) - yes/no prompt
  - [ ] Implement SelectMultiple(string[] items) - multi-select list
  - [ ] Implement ShowMenu(Dictionary<string, string> options) - menu selection
  - [ ] Handle cancellation with proper exit codes
  - [ ] No exceptions thrown - shell semantics

- [ ] Create Native/Interactive/Direct.cs
  - [ ] Static class Direct with C# semantics (strongly typed, can throw)
  - [ ] Implement SelectItem<T> returning T?
  - [ ] Implement ReadHost returning string (or SecureString)
  - [ ] Implement WriteHost with color and formatting options
  - [ ] Implement WriteProgress with IProgress<T> support
  - [ ] Implement ConfirmAction returning bool
  - [ ] Implement SelectMultiple<T> returning IEnumerable<T>
  - [ ] Support async operations and cancellation tokens
  - [ ] LINQ-composable operations

### Fuzzy Finder Implementation
- [ ] Create Native/Interactive/FuzzyFinder.cs
  - [ ] Implement fuzzy matching algorithm
  - [ ] Support case-insensitive search
  - [ ] Highlight matched characters
  - [ ] Support arrow key navigation
  - [ ] Support vim-style navigation (j/k)
  - [ ] Real-time filtering as user types
  - [ ] Preview pane support (optional)
  - [ ] Multi-select with space/tab

### Alias System Updates
- [ ] Update Native/Aliases/Bash.cs
  - [ ] Fzf(items) => Interactive.Commands.SelectItem()
  - [ ] Read(prompt) => Interactive.Commands.ReadHost()
  - [ ] Echo(message, color?) => Interactive.Commands.WriteHost()
  - [ ] Select(items) => Interactive.Commands.SelectItem()
  - [ ] Confirm(message) => Interactive.Commands.ConfirmAction()
  - [ ] Menu(options) => Interactive.Commands.ShowMenu()

### Data Types
- [ ] Create SelectionResult class
  - [ ] SelectedItem property
  - [ ] SelectedIndex property
  - [ ] SearchQuery property
  - [ ] Cancelled property

- [ ] Create SelectionOptions class
  - [ ] Prompt property
  - [ ] AllowMultiple property
  - [ ] CaseSensitive property
  - [ ] PreviewCommand property
  - [ ] Height property (lines to display)

- [ ] Create ProgressInfo class
  - [ ] Percent property
  - [ ] Message property
  - [ ] ElapsedTime property
  - [ ] EstimatedRemaining property

### Testing
- [ ] Test SelectItem with various item lists
- [ ] Test fuzzy matching algorithm accuracy
- [ ] Test keyboard navigation (arrows, vim keys)
- [ ] Test multi-select functionality
- [ ] Test ReadHost with normal and secure input
- [ ] Test WriteHost with different colors
- [ ] Test WriteProgress updates
- [ ] Test ConfirmAction yes/no responses
- [ ] Test cancellation (Ctrl+C/ESC)
- [ ] Test both Commands and Direct APIs
- [ ] Test bash aliases work correctly

## Implementation Notes

### Example Usage

```csharp
// PowerShell-style
var selected = SelectItem(new[] { "option1", "option2", "option3" });
var userName = ReadHost("Enter username: ");
WriteHost("Success!", ConsoleColor.Green);

// Bash-style aliases
var file = Fzf(Directory.GetFiles("."));
var password = Read("Password: ", secure: true);
var proceed = Confirm("Continue with operation?");

// Direct API for rich interactions
var item = await Direct.SelectItem(
    items: await GetAvailableOptions(),
    options: new SelectionOptions 
    { 
        Prompt = "Choose an option:",
        PreviewCommand = item => GetPreview(item),
        AllowMultiple = false
    }
);

// Progress reporting
await Direct.WriteProgress(async progress =>
{
    for (int i = 0; i <= 100; i++)
    {
        progress.Report(new ProgressInfo(i, $"Processing... {i}%"));
        await Task.Delay(50);
    }
});
```

### Fuzzy Matching Algorithm

```csharp
// Simple fuzzy match scoring
// "fb" matches "FooBar" with higher score than "FuzzBall"
// Consecutive matches score higher
// Early matches score higher
```

### Design Considerations

1. **Terminal Capabilities**: Detect and adapt to terminal features
2. **Color Support**: Handle terminals without color support
3. **Input Handling**: Raw mode for immediate key response
4. **Unicode Support**: Handle emoji and international characters
5. **Performance**: Fast filtering for large item lists (10k+ items)
6. **Accessibility**: Support screen readers where possible

### Terminal Interaction

- **Raw Mode**: Use Console.ReadKey for immediate response
- **Cursor Control**: ANSI escape sequences for positioning
- **Color Codes**: ANSI color codes with fallback
- **Terminal Size**: Adapt to terminal width/height
- **Input Buffer**: Handle paste and rapid typing

## Dependencies

- Task 019 (Native namespace structure) must be complete
- Builds on CommandOutput and dual API pattern
- System.Console for terminal interaction
- Consider Spectre.Console for rich terminal UI (optional)

## References

- fzf (fuzzy finder) behavior and features
- PowerShell Read-Host, Write-Host cmdlets
- Unix read, select, dialog commands
- Terminal UI libraries (Spectre.Console, Terminal.Gui)