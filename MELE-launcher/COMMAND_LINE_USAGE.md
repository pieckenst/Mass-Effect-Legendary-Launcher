# Command-Line Usage

The Mass Effect Legendary Launcher supports command-line arguments for direct game launching without showing the interactive menu. This is useful for integration with other launchers, shortcuts, or automation tools.

## Legendary Edition Games

### Format
```
MassEffectLauncher.exe -ME(1|2|3) -yes|-no LanguageCode [-silent]
```

### Parameters
- `-ME1`, `-ME2`, or `-ME3`: Specifies which Legendary Edition game to launch
- `-yes` or `-no`: Enable or disable force feedback (required)
- `LanguageCode`: Language code for text and voice-over (see below)
- `-silent`: Optional flag to suppress console output

### Examples
```bash
# Launch ME1 with force feedback enabled, Russian voice-over
MassEffectLauncher.exe -ME1 -yes RA

# Launch ME2 with force feedback disabled, French text with English voice-over, silent mode
MassEffectLauncher.exe -ME2 -no FRE -silent

# Launch ME3 with force feedback disabled, German voice-over
MassEffectLauncher.exe -ME3 -no DEU
```

## Original Trilogy Games

### Format
```
MassEffectLauncher.exe -OLDME(1|2|3) [-silent]
```

### Parameters
- `-OLDME1`, `-OLDME2`, or `-OLDME3`: Specifies which Original Trilogy game to launch
- `-silent`: Optional flag to suppress console output

### Examples
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
- `FR` - French voice-over
- `FE` - French text, English voice-over
- `DE` - German voice-over
- `GE` - German text, English voice-over
- `ES` - Spanish text, English voice-over
- `IT` - Italian voice-over
- `IE` - Italian text, English voice-over
- `RA` - Russian voice-over
- `RU` - Russian text, English voice-over
- `PLPC` - Polish voice-over
- `PL` - Polish text, English voice-over
- `JA` - Japanese text, English voice-over

### Mass Effect 2 & 3 Codes
- `FRA` - French voice-over
- `FRE` - French text, English voice-over
- `DEU` - German voice-over
- `DEE` - German text, English voice-over
- `ESN` - Spanish text, English voice-over
- `ITA` - Italian voice-over
- `ITE` - Italian text, English voice-over
- `RUS` - Russian text, English voice-over (no Russian VO in ME2/ME3)
- `POL` - Polish voice-over (ME2 only) / Polish text, English voice-over (ME3)
- `POE` - Polish text, English voice-over (ME2 only)
- `JPN` - Japanese text, English voice-over

## Exit Codes
- `0` - Success: Game launched successfully
- `1` - Error: Game not found, invalid arguments, or launch failed

## Notes
- Games must be detected and configured before using command-line launch
- If a game is not found, run the launcher without arguments to configure paths
- The launcher will automatically handle admin elevation if required by the game
- Language codes are case-insensitive
- Invalid or unknown language codes will default to English (INT)

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
