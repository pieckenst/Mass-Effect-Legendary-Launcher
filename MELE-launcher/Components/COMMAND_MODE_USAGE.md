# Command Mode Usage Guide

## Overview

The Mass Effect Legendary Launcher now features an advanced **Command Mode** that provides a focused, distraction-free interface for text input and future extensibility.

## Activating Command Mode

Press **Ctrl+F** from the main menu to enter Command Mode.

## Command Mode Interface

When Command Mode is activated:

1. **Body Section Hidden** - The navigation and context panels disappear
2. **Centered Input Box** - A large, focused command input dialog appears in the center
3. **Visual Distinction** - Uses highlight colors and double border for emphasis
4. **Clear Instructions** - Shows "COMMAND MODE" title with input prompt

### Visual Layout

```
┌─────────────────────────────────────────────────────────┐
│ MASS EFFECT LAUNCHER v2.1    user@hostname             │
│ SYSTEMS ALLIANCE NETWORK                                │
└─────────────────────────────────────────────────────────┘




        ╔═══════════════ INPUT ═══════════════╗
        ║                                     ║
        ║        COMMAND MODE                 ║
        ║                                     ║
        ║  COMMAND > your_text_here_          ║
        ║                                     ║
        ║                                     ║
        ║  Press ENTER to execute, ESC to    ║
        ║  cancel                             ║
        ║                                     ║
        ╚═════════════════════════════════════╝
```

## Using Command Mode

### Input Controls

- **Type** - Enter text normally
- **Backspace** - Delete the last character
- **Enter** - Execute the command (currently shows "not implemented" message)
- **ESC** - Cancel and return to normal UI

### Important Notes

- **ESC Behavior**: When in Command Mode, pressing ESC will exit Command Mode and return to the normal UI. It will NOT trigger the application exit confirmation.
- **Input Buffer**: The command buffer is cleared when you exit Command Mode with ESC
- **Selection Preserved**: Your menu selection is maintained when returning from Command Mode

## Exiting Command Mode

Press **ESC** to exit Command Mode and return to the normal menu interface. The Body section (navigation and context panels) will reappear.

## Future Extensibility

Command Mode is designed to support future features such as:

- **Search**: Quick search through games and settings
- **Filters**: Filter game list by criteria
- **Custom Commands**: Execute launcher commands via text
- **Quick Actions**: Shortcut commands for common operations
- **Path Input**: Manual game path entry (already implemented)

## Technical Details

### Implementation

- Command Mode uses a separate render path that hides the Body layout
- Input is handled through a StringBuilder buffer
- The `IsInInputMode` property prevents ESC from triggering exit confirmation
- Centered layout uses Padder with vertical offset for visual centering

### Code Integration

```csharp
// Activate Command Mode
menuSystem.ActivateInputMode("COMMAND >", (command) => 
{
    // Handle command execution
    ShowMessage($"Command '{command}' not implemented yet.", MessageType.Info);
});

// Check if in input mode
if (menuSystem.IsInInputMode)
{
    // Special handling when in command mode
}
```

## Keyboard Shortcuts Summary

| Key | Action |
|-----|--------|
| **Ctrl+F** | Enter Command Mode |
| **ESC** (in Command Mode) | Exit Command Mode |
| **ESC** (in Main Menu) | Show exit confirmation |
| **Enter** (in Command Mode) | Execute command |
| **Backspace** (in Command Mode) | Delete character |

## See Also

- [Menu System Improvements](MENUSYSTEM_IMPROVEMENTS.md)
- [Game Options Usage](GAME_OPTIONS_USAGE.md)
