# Game Options Submenu - Usage Guide

## Overview
Task 9 has been successfully implemented, adding a complete game options submenu with launch flow integration.

## What Was Implemented

### Subtask 9.1: Create Game Options Menu
Added `BuildGameOptionsMenu()` method to `MenuSystem` class that:
- Displays game-specific locale selection (Legendary Edition only)
- Displays game-specific force feedback toggle (Legendary Edition only)
- Shows a launch button to start the game
- Provides navigation back to main menu
- Loads and saves game-specific settings from configuration

### Subtask 9.2: Wire Game Launch Flow
Added `LaunchGameWithFlow()` method to `MenuSystem` class that:
- Checks if administrator elevation is required before launch
- Prompts user for elevation if needed
- Requests UAC elevation and restarts the launcher if approved
- Handles elevation decline gracefully with clear error messages
- Launches the game with selected options using `GameLauncher`
- Displays success or error messages after launch attempt

## Usage Example

```csharp
// Initialize components
var configManager = new ConfigManager();
var menuSystem = new MenuSystem();
menuSystem.Initialize(configManager);

var adminElevator = new AdminElevator();
var gameLauncher = new GameLauncher();
var gameDetector = new GameDetector();

// Detect games
var detectedGames = gameDetector.ScanForGames();
var selectedGame = detectedGames[0]; // User selects a game

// Build game options menu
var gameOptionsMenu = menuSystem.BuildGameOptionsMenu(
    selectedGame,
    (locale, forceFeedback) => 
    {
        // This callback is invoked when user clicks "Launch Game"
        menuSystem.LaunchGameWithFlow(
            selectedGame, 
            locale, 
            forceFeedback, 
            adminElevator, 
            gameLauncher
        );
    }
);

// Set the menu items and render
menuSystem.SetMenuItems(gameOptionsMenu);
menuSystem.SetMenuState(MenuState.GameOptions);
menuSystem.Render();
```

## Key Features

### For Legendary Edition Games:
- **Locale Selection**: Choose from 8 languages (en, fr, de, es, it, ja, pl, ru)
- **Force Feedback Toggle**: Enable/disable controller force feedback
- **Settings Persistence**: Game-specific settings are saved to configuration

### For Original Trilogy Games:
- **Direct Launch**: No locale/force feedback options (not supported by original games)
- **Launch Button**: Immediately available

### Administrator Elevation:
- **Automatic Detection**: Checks if game requires admin privileges
- **User Prompt**: Clear explanation of why elevation is needed
- **UAC Integration**: Uses Windows UAC for secure elevation
- **Graceful Handling**: Allows user to decline with clear messaging

## Requirements Validated

This implementation satisfies:
- **Requirement 5.1**: Display locale and force feedback options for Legendary Edition
- **Requirement 5.2**: Remember last used settings for each game
- **Requirement 3.3**: Check elevation requirements before launch
- **Requirement 5.3**: Construct appropriate command-line arguments
- **Requirement 5.5**: Display error messages on launch failure
- **Requirement 6.2**: Display launch confirmation messages

## Integration Points

The game options submenu integrates with:
1. **ConfigManager**: Loads and saves game-specific settings
2. **AdminElevator**: Checks privileges and requests elevation
3. **GameLauncher**: Launches games with proper arguments
4. **MenuSystem**: Provides consistent UI rendering and navigation

## Next Steps

To complete the launcher implementation:
1. Implement task 10: Integrate all components in Program.cs
2. Build the main menu with detected games
3. Wire up navigation between main menu and game options submenu
4. Handle the application loop with keyboard input
