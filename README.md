# Mass Effect Legendary Launcher

A modern, feature-rich launcher for Mass Effect Legendary Edition and the Original Trilogy with support for advanced language options, command-line launching, and more.

## Features

- **Automatic Game Detection**: Automatically scans for Mass Effect games on your system
- **Separate Text and Voice Language Selection**: Choose different languages for subtitles and voice-overs
- **Force Feedback Control**: Enable or disable controller force feedback
- **Skip BioWare Intro**: Option to skip the BioWare intro video (enabled by default)
- **BioWare Intro Video**: Plays the authentic BioWare logo intro before game launch with proper .bik support
  - Prioritizes native Bink SDK and RAD Video Tools for perfect playback
  - FFmpeg used only as last resort (has known issues with .bik files)
- **Command-Line Support**: Launch games directly without the interactive menu
- **Admin Elevation**: Automatically handles games that require administrator privileges
- **Interactive Terminal UI**: Beautiful, easy-to-navigate menu system using Spectre.Console

## Installation

1. Download the latest release from the [Releases](https://github.com/yourusername/Mass-Effect-Legendary-Launcher/releases) page
2. Extract the files to a folder of your choice
3. Run `MassEffectLauncher.exe`

## Usage

### Interactive Mode

Simply run `MassEffectLauncher.exe` without arguments to launch the interactive menu:

1. The launcher will automatically scan for installed Mass Effect games
2. Select a game from the main menu
3. Configure language and force feedback options
4. Launch the game

### Command-Line Mode

For direct game launching without the interactive menu, use command-line arguments:

#### Legendary Edition Games

```bash
MassEffectLauncher.exe -ME(1|2|3) -yes|-no LanguageCode [-silent] [-nointro]
```

**Parameters:**
- `-ME1`, `-ME2`, or `-ME3`: Specifies which Legendary Edition game to launch
- `-yes` or `-no`: Enable or disable force feedback
- `LanguageCode`: Language code for text and voice-over (see Language Codes section)
- `-silent`: Optional flag to suppress console output
- `-nointro`: Optional flag to skip the BioWare intro video

**Examples:**
```bash
# Launch ME1 with force feedback enabled, Russian voice-over
MassEffectLauncher.exe -ME1 -yes RA

# Launch ME2 with force feedback disabled, French text with English voice-over
MassEffectLauncher.exe -ME2 -no FRE -silent

# Launch ME3 with German voice-over, skip intro
MassEffectLauncher.exe -ME3 -no DEU -nointro
```

#### Original Trilogy Games

```bash
MassEffectLauncher.exe -OLDME(1|2|3) [-silent]
```

**Parameters:**
- `-OLDME1`, `-OLDME2`, or `-OLDME3`: Specifies which Original Trilogy game to launch
- `-silent`: Optional flag to suppress console output

**Examples:**
```bash
# Launch original ME1
MassEffectLauncher.exe -OLDME1

# Launch original ME2 in silent mode
MassEffectLauncher.exe -OLDME2 -silent
```

## Language Codes

### Universal Code (All Games)
- `INT` - English (text and voice-over)

### Mass Effect 1 Codes
| Code | Description |
|------|-------------|
| `FR` | French voice-over |
| `FE` | French text, English voice-over |
| `DE` | German voice-over |
| `GE` | German text, English voice-over |
| `ES` | Spanish text, English voice-over |
| `IT` | Italian voice-over |
| `IE` | Italian text, English voice-over |
| `RA` | Russian voice-over |
| `RU` | Russian text, English voice-over |
| `PLPC` | Polish voice-over |
| `PL` | Polish text, English voice-over |
| `JA` | Japanese text, English voice-over |

### Mass Effect 2 & 3 Codes
| Code | Description |
|------|-------------|
| `FRA` | French voice-over |
| `FRE` | French text, English voice-over |
| `DEU` | German voice-over |
| `DEE` | German text, English voice-over |
| `ESN` | Spanish text, English voice-over |
| `ITA` | Italian voice-over |
| `ITE` | Italian text, English voice-over |
| `RUS` | Russian text, English voice-over |
| `POL` | Polish voice-over (ME2) / Polish text, English VO (ME3) |
| `POE` | Polish text, English voice-over (ME2 only) |
| `JPN` | Japanese text, English voice-over |

**Note:** Language codes are case-insensitive. Invalid or unknown codes will default to English (INT).

## Integration Examples

### Windows Shortcut
Create a shortcut with target:
```
"C:\Path\To\MassEffectLauncher.exe" -ME1 -yes INT
```

### Batch Script
```batch
@echo off
"C:\Path\To\MassEffectLauncher.exe" -ME2 -no FRA -silent
if %errorlevel% neq 0 (
    echo Failed to launch game
    pause
)
```

### PowerShell Script
```powershell
$launcherPath = "C:\Path\To\MassEffectLauncher.exe"
$args = @("-ME3", "-no", "DEU", "-silent")
$process = Start-Process -FilePath $launcherPath -ArgumentList $args -Wait -PassThru
if ($process.ExitCode -ne 0) {
    Write-Error "Failed to launch game"
}
```

## Exit Codes

- `0` - Success: Game launched successfully
- `1` - Error: Game not found, invalid arguments, or launch failed

## Requirements

- Windows 10 or later
- .NET 6.0 Runtime
- Mass Effect Legendary Edition and/or Original Trilogy installed

## Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/Mass-Effect-Legendary-Launcher.git
   ```

2. Open the solution in Visual Studio 2022 or later

3. Build the solution:
   ```bash
   dotnet build MELE-launcher.sln
   ```

4. Run the launcher:
   ```bash
   dotnet run --project MELE-launcher
   ```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the Mozilla Public License Version 2.0 - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Spectre.Console](https://spectreconsole.net/) for the beautiful terminal UI
- Inspired by the original Mass Effect Legendary Edition launcher
- Thanks to the Mass Effect modding community

## Troubleshooting

### Games Not Detected
If your games aren't automatically detected:
1. Run the launcher in interactive mode
2. Go to Settings
3. Use "Add Game Path Manually" to specify your game installation directory

### Admin Elevation Issues
Some games may require administrator privileges. The launcher will automatically request elevation when needed. If you encounter issues:
1. Try running the launcher as administrator
2. Check that your game executable has the correct permissions

### Language Not Working
If language settings aren't being applied:
1. Verify you're using the correct language code for your game version
2. Check that the language files are installed in your game directory
3. Some languages may not have voice-over support in certain games

### Broken or Corrupted BioWare Intro Video
If the BioWare intro video appears corrupted, garbled, or shows visual artifacts:
1. This indicates FFmpeg is being used (has known issues with .bik files)
2. **Solution**: Install [RAD Video Tools](http://www.radgametools.com/bnkdown.htm) for proper .bik playback
3. The launcher will automatically detect and use RAD Video Tools
4. Alternatively, enable "Skip BioWare Intro" in Settings to bypass the video entirely
5. For best quality, the launcher prioritizes: Bink SDK > RAD Video Tools > FFmpeg

## Support

For issues, questions, or suggestions, please [open an issue](https://github.com/pieckenst/Mass-Effect-Legendary-Launcher/issues) on GitHub.
