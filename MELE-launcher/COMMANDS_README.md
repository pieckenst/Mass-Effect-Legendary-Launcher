# Custom Commands Configuration

## Quick Start

1. Copy `commands.json.example` to `commands.json`
2. Edit `commands.json` to add your custom commands
3. Restart the launcher
4. Press **Ctrl+F** and type `help` to see your commands

## File Location

Place `commands.json` in the same directory as the launcher executable:
```
MassEffectLauncher.exe
commands.json          <-- Your custom commands
commands.json.example  <-- Example template
```

## Basic Structure

```json
{
  "version": "1.0",
  "commands": [
    {
      "name": "commandname",
      "aliases": ["alias1", "alias2"],
      "description": "What this command does",
      "usage": "commandname [arguments]",
      "category": "Category Name",
      "enabled": true,
      "requiresAdmin": false,
      "actionType": "handler_name",
      "parameters": {}
    }
  ]
}
```

## Built-in Action Types

These action types are already implemented and ready to use:

| Action Type | Description | Example |
|-------------|-------------|---------|
| `help` | Show help information | `help`, `help scan` |
| `list_games` | List detected games | `list` |
| `rescan` | Rescan for games | `scan` |
| `settings` | Open settings menu | `settings` |
| `exit` | Exit launcher | `exit` |
| `clear` | Clear screen | `clear` |
| `version` | Show version info | `version` |

## Creating Custom Commands

### Example 1: Simple Alias

Create a shorter alias for an existing command:

```json
{
  "name": "g",
  "aliases": ["games"],
  "description": "Quick alias for listing games",
  "usage": "g",
  "category": "Game",
  "enabled": true,
  "actionType": "list_games"
}
```

### Example 2: Custom Command (Requires Implementation)

Define a new command that needs a custom handler:

```json
{
  "name": "launch",
  "aliases": ["play", "start"],
  "description": "Quick launch a game by number",
  "usage": "launch <1|2|3>",
  "category": "Game",
  "enabled": false,
  "requiresAdmin": false,
  "actionType": "launch_game",
  "parameters": {
    "note": "Requires implementation in CommandExecutor.cs"
  }
}
```

To implement this command, add a handler in `CommandExecutor.cs`:

```csharp
_builtInHandlers["launch_game"] = (args, cmd) =>
{
    // Your implementation here
};
```

### Example 3: Admin-Required Command

```json
{
  "name": "cleanup",
  "description": "Clean temporary files",
  "usage": "cleanup",
  "category": "Maintenance",
  "enabled": true,
  "requiresAdmin": true,
  "actionType": "cleanup_temp"
}
```

## Field Reference

### Required Fields

- **name**: Primary command name (lowercase recommended)
- **description**: Brief description of what the command does
- **actionType**: Handler identifier

### Optional Fields

- **aliases**: Array of alternative names
- **usage**: Syntax example (e.g., "command [arg1] [arg2]")
- **category**: Group name for organization
- **enabled**: Set to `false` to disable without removing
- **requiresAdmin**: Set to `true` if admin privileges needed
- **parameters**: Custom key-value pairs for your handler

## Categories

Organize commands into logical groups:

- **System**: Core launcher functions
- **Game**: Game-related operations
- **Settings**: Configuration commands
- **Utility**: Helper tools
- **Maintenance**: Cleanup and repair
- **Mods**: Mod management (if implemented)
- **Debug**: Development/testing commands

## Tips

1. **Use descriptive names**: Make commands self-explanatory
2. **Provide aliases**: Add shortcuts for frequently used commands
3. **Set enabled: false**: For commands under development
4. **Use categories**: Keep related commands together
5. **Document usage**: Include syntax examples
6. **Test thoroughly**: Verify commands work as expected

## Validation

The launcher will:
- ✓ Load valid commands from `commands.json`
- ✓ Merge with built-in commands
- ✓ Allow custom commands to override built-ins
- ✗ Silently skip invalid JSON
- ✗ Ignore commands with missing required fields

## Troubleshooting

### Command not appearing

1. Check JSON syntax (use a JSON validator)
2. Verify file is named exactly `commands.json`
3. Ensure file is in the correct directory
4. Restart the launcher
5. Type `help` to see if command is listed

### Command shows but doesn't work

1. Check if `enabled` is set to `true`
2. Verify `actionType` matches a registered handler
3. Check console for error messages
4. Ensure required parameters are provided

### JSON syntax errors

Common mistakes:
- Missing commas between objects
- Trailing commas after last item
- Unquoted keys or values
- Mismatched brackets/braces

Use a JSON validator or editor with syntax highlighting.

## Examples

See `commands.json.example` for complete examples of:
- Game launch shortcuts
- Save backup commands
- Mod management commands
- Custom utility commands

## See Also

- [Command System Documentation](Components/COMMAND_SYSTEM.md)
- [Command Mode Usage](Components/COMMAND_MODE_USAGE.md)
