# Command System Documentation

## Overview

The Mass Effect Legendary Launcher features a powerful command system that allows users to execute commands via a text-based interface. The system is extensible and supports both built-in commands and custom commands defined via JSON configuration.

## Built-in Commands

### System Commands

#### `help` (aliases: `?`, `h`)
Display help information about available commands.

**Usage:**
```
help              # Show all commands
help <command>    # Show detailed help for a specific command
```

**Examples:**
```
help
help scan
? list
```

#### `version` (aliases: `ver`, `about`)
Display version information about the launcher.

**Usage:**
```
version
```

#### `clear` (aliases: `cls`)
Clear the screen.

**Usage:**
```
clear
```

#### `exit` (aliases: `quit`, `q`)
Exit the launcher.

**Usage:**
```
exit
```

#### `settings` (aliases: `config`, `preferences`)
Open the settings menu.

**Usage:**
```
settings
```

### Game Commands

#### `launch` (aliases: `play`, `start`) ✨ NEW
Quick launch a Mass Effect game by number.

**Usage:**
```
launch <1|2|3>
```

**Arguments:**
- `1` - Launch Mass Effect 1 Legendary Edition
- `2` - Launch Mass Effect 2 Legendary Edition
- `3` - Launch Mass Effect 3 Legendary Edition

**Examples:**
```
launch 1          # Launch ME1
play 2            # Launch ME2 (using alias)
start 3           # Launch ME3 (using alias)
```

**Features:**
- Uses your configured language settings
- Applies force feedback preferences
- Respects skip intro setting
- Handles admin elevation automatically
- Shows detailed launch information

**Error Handling:**
- Validates game number (1-3)
- Checks if game is installed
- Verifies game installation is valid
- Provides helpful error messages

#### `list` (aliases: `ls`, `games`)
List all detected Mass Effect games with their installation status.

**Usage:**
```
list
```

**Output:**
- Game name
- Edition (Legendary/Original)
- Installation status (✓ Installed / ✗ Missing)

#### `scan` (aliases: `rescan`, `refresh`)
Rescan for Mass Effect game installations.

**Usage:**
```
scan
```

## Custom Commands

### Configuration File

Custom commands can be defined in a `commands.json` file placed in the launcher's directory. The file follows this structure:

```json
{
  "version": "1.0",
  "commands": [
    {
      "name": "mycommand",
      "aliases": ["mc", "cmd"],
      "description": "Description of what the command does",
      "usage": "mycommand [arguments]",
      "category": "Custom",
      "enabled": true,
      "requiresAdmin": false,
      "actionType": "custom_action",
      "parameters": {
        "key1": "value1",
        "key2": "value2"
      }
    }
  ]
}
```

### Command Definition Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Primary command name |
| `aliases` | array | No | Alternative names for the command |
| `description` | string | Yes | What the command does |
| `usage` | string | No | Usage syntax |
| `category` | string | No | Category for grouping (e.g., "Game", "System", "Utility") |
| `enabled` | boolean | No | Whether the command is active (default: true) |
| `requiresAdmin` | boolean | No | Whether admin privileges are needed (default: false) |
| `actionType` | string | Yes | Handler identifier for the command |
| `parameters` | object | No | Additional configuration parameters |

### Example Custom Commands

See `commands.json.example` for sample custom command definitions:

```json
{
  "version": "1.0",
  "commands": [
    {
      "name": "launch",
      "aliases": ["play", "start"],
      "description": "Quick launch a game by number (1, 2, or 3)",
      "usage": "launch <1|2|3>",
      "category": "Game",
      "enabled": false,
      "requiresAdmin": false,
      "actionType": "launch_game"
    },
    {
      "name": "backup",
      "aliases": ["save"],
      "description": "Backup game saves",
      "usage": "backup <game_number>",
      "category": "Utility",
      "enabled": false,
      "requiresAdmin": false,
      "actionType": "backup_saves"
    }
  ]
}
```

## Implementing Custom Command Handlers

To implement a custom command handler, you need to:

1. **Define the command** in `commands.json`
2. **Register a handler** in `CommandExecutor.cs`

### Example Handler Registration

```csharp
// In CommandExecutor.RegisterBuiltInHandlers()
_builtInHandlers["launch_game"] = (args, cmd) =>
{
    if (args.Length == 0)
    {
        AnsiConsole.MarkupLine("[red]Error: Game number required (1, 2, or 3)[/]");
        return;
    }

    if (int.TryParse(args[0], out int gameNum) && gameNum >= 1 && gameNum <= 3)
    {
        // Launch logic here
        AnsiConsole.MarkupLine($"[green]Launching Mass Effect {gameNum}...[/]");
    }
    else
    {
        AnsiConsole.MarkupLine("[red]Invalid game number. Use 1, 2, or 3.[/]");
    }
    
    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
    Console.ReadKey(true);
};
```

## Command Parsing

The command system supports:

- **Simple commands**: `help`
- **Commands with arguments**: `help scan`
- **Quoted arguments**: `launch "Mass Effect 1"`
- **Multiple arguments**: `backup 1 "C:\Backups"`

### Parsing Rules

1. Commands are case-insensitive
2. Arguments are separated by spaces
3. Use quotes to include spaces in arguments
4. Aliases are treated identically to primary command names

## Command Execution Flow

1. User presses **Ctrl+F** to enter Command Mode
2. User types command and presses **Enter**
3. Command is parsed into command name and arguments
4. System looks up command definition (including aliases)
5. Command is validated (enabled, admin requirements)
6. Appropriate handler is executed
7. Result is displayed to user
8. User returns to normal UI

## Error Handling

The command system provides clear error messages for:

- **Unknown commands**: "Unknown command: 'xyz'. Type 'help' for available commands."
- **Disabled commands**: "Command 'xyz' is currently disabled."
- **Missing handlers**: "Command 'xyz' has no handler defined."
- **Execution errors**: "Error executing command: [error message]"

## Best Practices

### For Users

1. Use `help` to discover available commands
2. Use `help <command>` for detailed command information
3. Use command aliases for faster typing
4. Check command categories to find related commands

### For Developers

1. **Keep commands focused**: Each command should do one thing well
2. **Provide clear descriptions**: Users should understand what a command does
3. **Include usage examples**: Show the correct syntax
4. **Handle errors gracefully**: Provide helpful error messages
5. **Use categories**: Group related commands together
6. **Test thoroughly**: Ensure commands work in all scenarios
7. **Document parameters**: Explain what each parameter does

## Integration with Launcher

The command system integrates with the launcher through callbacks:

```csharp
menuSystem.RegisterCommandCallbacks(
    onRescan: () => ScanForGames(),
    onExit: () => _shouldExit = true,
    onSettings: () => ShowSettings(),
    getDetectedGames: () => _detectedGames
);
```

This allows commands to:
- Trigger game rescans
- Exit the application
- Open settings
- Access detected game information

## Future Enhancements

Potential future additions to the command system:

1. **Command history**: Navigate previous commands with up/down arrows
2. **Tab completion**: Auto-complete command names
3. **Command chaining**: Execute multiple commands in sequence
4. **Scripting support**: Run command scripts from files
5. **Output redirection**: Save command output to files
6. **Variables**: Store and reuse values
7. **Conditional execution**: Execute commands based on conditions
8. **Remote commands**: Execute commands on remote instances

## Troubleshooting

### Commands not loading

- Check that `commands.json` is in the launcher directory
- Verify JSON syntax is valid
- Check file permissions
- Look for error messages in console output

### Command not executing

- Verify command is enabled (`"enabled": true`)
- Check if admin privileges are required
- Ensure handler is registered for the `actionType`
- Check for typos in command name

### Custom command not appearing

- Verify command is in `commands.json`
- Check that JSON is valid
- Restart the launcher
- Use `help` to see if command is listed

## See Also

- [Command Mode Usage](COMMAND_MODE_USAGE.md)
- [Menu System Improvements](MENUSYSTEM_IMPROVEMENTS.md)
- [Game Options Usage](GAME_OPTIONS_USAGE.md)
