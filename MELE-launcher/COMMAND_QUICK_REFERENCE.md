# Command Quick Reference

## Accessing Command Mode

Press **Ctrl+F** from anywhere in the launcher to enter Command Mode.

## Essential Commands

| Command | Aliases | Description |
|---------|---------|-------------|
| `help` | `?`, `h` | Show all commands or help for specific command |
| `launch <1\|2\|3>` | `play`, `start` | Quick launch ME1, ME2, or ME3 |
| `list` | `ls`, `games` | List all detected games |
| `scan` | `rescan`, `refresh` | Rescan for game installations |
| `settings` | `config`, `preferences` | Open settings menu |
| `clear` | `cls` | Clear the screen |
| `version` | `ver`, `about` | Show version information |
| `exit` | `quit`, `q` | Exit the launcher |

## Quick Examples

### Launch Games
```
launch 1          # Launch Mass Effect 1
play 2            # Launch Mass Effect 2
start 3           # Launch Mass Effect 3
```

### Get Information
```
help              # Show all commands
help launch       # Show launch command details
list              # List detected games
version           # Show launcher version
```

### Manage Games
```
scan              # Rescan for games
settings          # Open settings
```

### Navigation
```
clear             # Clear screen
exit              # Exit launcher
```

## Command Features

- **Case Insensitive**: `LAUNCH 1`, `launch 1`, and `Launch 1` all work
- **Aliases**: Use shorter versions like `?` instead of `help`
- **Arguments**: Some commands accept parameters (e.g., `launch 1`)
- **Error Messages**: Helpful feedback if something goes wrong
- **ESC to Exit**: Press ESC to leave command mode

## Tips

1. **Use Tab**: Type partial commands and use aliases for speed
2. **Check Help**: Use `help <command>` for detailed information
3. **List First**: Run `list` to see what games are available
4. **Quick Launch**: `launch` is the fastest way to start a game
5. **Scan if Missing**: If games aren't showing, run `scan`

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Ctrl+F** | Enter Command Mode |
| **Enter** | Execute command |
| **ESC** | Exit Command Mode |
| **Backspace** | Delete character |

## Common Workflows

### Quick Launch Workflow
```
Ctrl+F → launch 1 → Enter
```

### Check and Launch
```
Ctrl+F → list → Enter
Ctrl+F → launch 2 → Enter
```

### Rescan and Launch
```
Ctrl+F → scan → Enter
Ctrl+F → launch 3 → Enter
```

### Get Help
```
Ctrl+F → help → Enter
Ctrl+F → help launch → Enter
```

## Error Messages

| Error | Meaning | Solution |
|-------|---------|----------|
| "Unknown command" | Command not recognized | Type `help` to see available commands |
| "Game number required" | Missing argument | Provide game number: `launch 1` |
| "Invalid game number" | Wrong number | Use 1, 2, or 3 |
| "Game not found" | Game not detected | Run `scan` to search for games |
| "Installation is invalid" | Game files missing | Verify game installation |

## See Also

- [Full Command System Documentation](Components/COMMAND_SYSTEM.md)
- [Command Mode Usage Guide](Components/COMMAND_MODE_USAGE.md)
- [Custom Commands Configuration](COMMANDS_README.md)
