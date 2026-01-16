# Terminal Mode Usage Guide

## Overview

The Mass Effect Legendary Launcher includes a persistent terminal mode that provides a command-line interface overlay. The terminal uses a centered panel design with the input box permanently anchored at the top and command output scrolling down below it.

## Activation

### Opening Terminal Mode
- Press **`~`** (tilde key) or **`F1`** from the main menu
- The main menu will be hidden
- A centered terminal panel will appear with input at the top
- Command history will scroll down below the input

### Closing Terminal Mode
- Press **`~`** (tilde key) or **`F1`** again to close
- Press **`ESC`** to abort current input and close terminal

## Visual Design

### Layout
- **Header**: Launcher branding (always visible)
- **Centered Terminal Panel**: 
  - Terminal header with title
  - Input box (permanently at top)
  - Horizontal separator
  - Scrolling command history/output (below input)
  - Footer with keyboard shortcuts

### Aesthetics
- Double border with highlight color (cyan/orange based on mode)
- Input box always visible at the top
- Command output scrolls down naturally
- Clean separation between input and output
- Centered panel (100 characters wide)

## Features

### Persistent Shell
- Terminal stays open after executing commands
- Command history scrolls down below input (last 8 entries)
- Input box remains at the top for easy access
- Color-coded output (green for success, red for errors, cyan for info)

### Command Execution
- Type any command and press **`ENTER`** to execute
- Commands are logged to the terminal history
- Results appear below the input box
- Empty commands are ignored
- Terminal stays open for next command

### Scrolling Output
- New output appears at the bottom
- Older entries scroll up
- Maximum 8 history entries displayed
- Automatic history management

## Available Commands

All commands from the command system are available in terminal mode:

### System Commands
- `help` - Display command list
- `help <command>` - Show detailed help for a specific command
- `version` - Show launcher version and system info
- `clear` - Clear terminal display
- `exit` - Exit the launcher

### Game Commands
- `list` - Show all detected games
- `launch <1|2|3>` - Launch Mass Effect 1, 2, or 3
- `scan` - Rescan for games

### Settings Commands
- `settings` - Open settings menu

## Keyboard Shortcuts

### In Terminal Mode
- **`ENTER`** - Execute command
- **`ESC`** - Abort input and close terminal
- **`~`** or **`F1`** - Close terminal
- **`BACKSPACE`** - Delete character

### In Normal Mode
- **`~`** or **`F1`** - Open terminal
- **`↑`/`↓`** - Navigate menu
- **`ENTER`** - Select menu item
- **`ESC`** - Go back

## Examples

### Launch a Game
```
CMD > launch 2
```
Output appears below showing launch sequence.

### Check Installed Games
```
CMD > list
```
Table of detected games appears below input.

### Get Help
```
CMD > help
```
Command list appears below input.

### View Version Info
```
CMD > version
```
Version details appear below input.

## Technical Details

### Terminal History
- Maximum of 8 entries displayed
- Older entries are automatically removed
- Each entry includes color coding
- Commands and results are both logged

### Input Handling
- Markup characters are automatically escaped
- Special characters are handled safely
- Unicode support for international characters
- Real-time input display with cursor

### Integration
- Terminal hides the main menu completely
- Settings and game configurations are shared
- Commands can trigger menu actions
- Seamless switching between modes

## Tips

1. **Quick Access**: Use `~` for fastest terminal access
2. **Command Discovery**: Type `help` to see all available commands
3. **Persistent Mode**: Terminal stays open for multiple commands
4. **Visual Feedback**: Watch output scroll down below input
5. **Safe Exit**: Use `ESC` to abort without executing

## Comparison: Terminal vs Command Mode

### Terminal Mode (`~` or `F1`)
- Persistent shell that stays open
- Input at top, output scrolls down
- Multiple commands in sequence
- Centered panel design
- Best for: Power users, multiple operations

### Command Mode (`Ctrl+F`)
- One-off command input
- Centered input box only
- Closes after single command
- No history display
- Best for: Quick single commands

Both modes share the same command system and execute the same commands, but provide different user experiences for different workflows.
