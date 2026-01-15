# Design Document: Enhanced Terminal UI

## Overview

This design describes the architecture for enhancing the Mass Effect Legendary Launcher with a modern, visually appealing terminal UI, automatic game detection, and administrator elevation support. The solution uses C# with .NET 6+ and leverages the Spectre.Console library for rich terminal rendering.

## Architecture

The application follows a modular architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      Program.cs (Entry Point)                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ MenuSystem  │  │ GameDetector│  │   AdminElevator     │  │
│  │             │  │             │  │                     │  │
│  │ - Render()  │  │ - Scan()    │  │ - IsElevated()      │  │
│  │ - Navigate()│  │ - Validate()│  │ - RequestElevation()│  │
│  │ - Select()  │  │             │  │                     │  │
│  └──────┬──────┘  └──────┬──────┘  └──────────┬──────────┘  │
│         │                │                     │             │
│         └────────────────┼─────────────────────┘             │
│                          │                                   │
│                  ┌───────┴───────┐                           │
│                  │ ConfigManager │                           │
│                  │               │                           │
│                  │ - Load()      │                           │
│                  │ - Save()      │                           │
│                  └───────┬───────┘                           │
│                          │                                   │
│                  ┌───────┴───────┐                           │
│                  │ GameLauncher  │                           │
│                  │               │                           │
│                  │ - Launch()    │                           │
│                  └───────────────┘                           │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### 1. MenuSystem

Responsible for rendering the terminal UI and handling user input.

```csharp
public class MenuSystem
{
    private int _selectedIndex;
    private List<MenuItem> _menuItems;
    private MenuState _currentState;
    
    public void Render();
    public void HandleInput(ConsoleKeyInfo key);
    public MenuItem GetSelectedItem();
    public void SetMenuItems(List<MenuItem> items);
    public void ShowMessage(string message, MessageType type);
    public void ShowProgress(string task, Action action);
}

public class MenuItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Action OnSelect { get; set; }
    public bool IsEnabled { get; set; }
    public MenuItemType Type { get; set; }
}

public enum MenuItemType { Game, Setting, Action, Separator }
public enum MessageType { Info, Success, Warning, Error }
public enum MenuState { Main, GameOptions, Settings, Confirmation }
```

### 2. GameDetector

Scans for Mass Effect game installations across multiple sources.

```csharp
public class GameDetector
{
    public List<DetectedGame> ScanForGames();
    public bool ValidateInstallation(string path, GameType gameType);
    
    private List<string> GetSteamLibraryPaths();
    private List<string> GetEAAppPaths();
    private List<string> GetRegistryPaths();
    private List<string> GetCommonPaths();
}

public class DetectedGame
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string ExecutablePath { get; set; }
    public GameType Type { get; set; }
    public GameEdition Edition { get; set; }
    public bool IsValid { get; set; }
    public bool RequiresAdmin { get; set; }
}

public enum GameType { ME1, ME2, ME3 }
public enum GameEdition { Legendary, Original }
```

### 3. AdminElevator

Handles Windows administrator privilege detection and elevation.

```csharp
public class AdminElevator
{
    public bool IsRunningAsAdmin();
    public bool RequiresElevation(string path);
    public bool RequestElevation(string[] args);
    
    private bool IsProtectedPath(string path);
}
```

### 4. ConfigManager

Manages persistent configuration using JSON serialization.

```csharp
public class ConfigManager
{
    private const string ConfigFileName = "launcher-config.json";
    
    public LauncherConfig Load();
    public void Save(LauncherConfig config);
    public string GetConfigPath();
}

public class LauncherConfig
{
    public List<GameConfig> Games { get; set; }
    public string DefaultLocale { get; set; }
    public bool DefaultForceFeedback { get; set; }
    public DateTime LastScanDate { get; set; }
}

public class GameConfig
{
    public GameType Type { get; set; }
    public GameEdition Edition { get; set; }
    public string Path { get; set; }
    public string Locale { get; set; }
    public bool ForceFeedback { get; set; }
}
```

### 5. GameLauncher

Handles the actual game launching with appropriate arguments.

```csharp
public class GameLauncher
{
    public LaunchResult Launch(DetectedGame game, LaunchOptions options);
    
    private string BuildArguments(DetectedGame game, LaunchOptions options);
    private ProcessStartInfo CreateStartInfo(DetectedGame game, string arguments);
}

public class LaunchOptions
{
    public string Locale { get; set; }
    public bool ForceFeedback { get; set; }
    public bool Silent { get; set; }
}

public class LaunchResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
```

## Data Models

### Game Detection Paths

```csharp
public static class GamePaths
{
    // Legendary Edition paths relative to installation root
    public static readonly Dictionary<GameType, string> LegendaryPaths = new()
    {
        { GameType.ME1, "Game/ME1/Binaries/Win64/MassEffect1.exe" },
        { GameType.ME2, "Game/ME2/Binaries/Win64/MassEffect2.exe" },
        { GameType.ME3, "Game/ME3/Binaries/Win64/MassEffect3.exe" }
    };
    
    // Original trilogy paths relative to installation root
    public static readonly Dictionary<GameType, string> OriginalPaths = new()
    {
        { GameType.ME1, "Binaries/Win32/MassEffect.exe" },
        { GameType.ME2, "Binaries/Win32/MassEffect2.exe" },
        { GameType.ME3, "Binaries/Win32/MassEffect3.exe" }
    };
    
    // Common installation directories to scan
    public static readonly string[] CommonDirectories = 
    {
        @"C:\Program Files\EA Games",
        @"C:\Program Files (x86)\EA Games",
        @"C:\Program Files\Origin Games",
        @"C:\Program Files (x86)\Origin Games",
        @"C:\Program Files\Steam\steamapps\common",
        @"C:\Program Files (x86)\Steam\steamapps\common"
    };
}
```

## Error Handling

### Error Categories

1. **Configuration Errors**: Invalid or corrupted config file
   - Recovery: Create default configuration, notify user

2. **Detection Errors**: Unable to access registry or file system
   - Recovery: Skip inaccessible sources, continue with available data

3. **Elevation Errors**: UAC declined or failed
   - Recovery: Display warning, allow limited functionality

4. **Launch Errors**: Game executable not found or access denied
   - Recovery: Display error message, offer to rescan or manually configure

### Error Display

```csharp
public class ErrorHandler
{
    public void HandleError(Exception ex, ErrorContext context);
    public void DisplayError(string message, string suggestion);
}

public enum ErrorContext { Configuration, Detection, Elevation, Launch }
```

## Testing Strategy

### Unit Tests

Unit tests will verify individual component behavior:
- ConfigManager serialization/deserialization
- GameDetector path validation
- AdminElevator privilege detection
- MenuSystem navigation logic

### Property-Based Tests

Property-based tests will verify universal properties across all inputs using a PBT library (FsCheck for C#).

### Integration Tests

Integration tests will verify component interactions:
- Full game detection workflow
- Configuration persistence across sessions
- Menu navigation flows

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Menu Navigation Wraps Correctly

*For any* menu with N items (N > 0) and any starting selected index, pressing Down from the last item should wrap to the first item, and pressing Up from the first item should wrap to the last item. The selected index should always remain within bounds [0, N-1].

**Validates: Requirements 1.3**

### Property 2: Installation Validation Requires Executable

*For any* detected game path and game type, the validation function should return true if and only if the expected executable file exists at the correct relative path within the installation directory.

**Validates: Requirements 2.6**

### Property 3: Configuration Round-Trip Consistency

*For any* valid LauncherConfig object containing game paths, locale settings, and force feedback preferences, serializing to JSON and then deserializing should produce an equivalent configuration object with all fields preserved.

**Validates: Requirements 4.1, 4.3, 2.9, 5.2**

### Property 4: Protected Path Detection

*For any* file path string, the IsProtectedPath function should return true if and only if the path starts with a Windows protected directory (Program Files, Program Files (x86), Windows, or the root of the system drive with restricted permissions).

**Validates: Requirements 3.2**

### Property 5: Launch Argument Construction

*For any* valid combination of game type, edition, locale string, and force feedback boolean, the argument builder should produce a command-line string that:
- Contains the locale value when provided
- Contains "-NOFORCEFEEDBACK" if and only if force feedback is disabled
- Contains all required base arguments for the game edition

**Validates: Requirements 5.3**
