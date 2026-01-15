# Requirements Document

## Introduction

This document defines the requirements for enhancing the Mass Effect Legendary Launcher terminal UI with a beautiful console menu system, automatic game folder detection, and administrator elevation support for games installed on protected drives (like C:/).

## Glossary

- **Launcher**: The console application that launches Mass Effect games
- **Game_Detector**: The component responsible for automatically finding Mass Effect game installations
- **Admin_Elevator**: The component that handles Windows administrator privilege elevation
- **Menu_System**: The interactive console menu with keyboard navigation
- **Config_Manager**: The component that persists and retrieves user settings

## Requirements

### Requirement 1: Beautiful Console Menu System

**User Story:** As a user, I want an attractive and easy-to-navigate console menu, so that I can select games and options without typing commands.

#### Acceptance Criteria

1. WHEN the Launcher starts, THE Menu_System SHALL display an ASCII art header with the Mass Effect branding
2. WHEN the main menu is displayed, THE Menu_System SHALL show a list of available games with visual highlighting for the selected item
3. WHEN the user presses arrow keys (Up/Down), THE Menu_System SHALL move the selection highlight accordingly
4. WHEN the user presses Enter, THE Menu_System SHALL execute the action for the currently selected menu item
5. WHEN the user presses Escape, THE Menu_System SHALL navigate back to the previous menu or exit if on main menu
6. THE Menu_System SHALL use colored text and box-drawing characters to create visually appealing borders and sections
7. WHEN a menu item is selected, THE Menu_System SHALL display a distinct visual indicator (color change or bracket highlight)

#### Visual Style Reference

The terminal UI should follow modern console application design patterns:

**Header Section:**
- Large ASCII art logo at the top using stylized block characters
- Title text in a contrasting color (cyan, yellow, or red on dark background)
- Version number and subtitle displayed below the logo
- Horizontal separator line using box-drawing characters (─, ═, or ━)

**Menu Layout:**
- Bordered menu panel using Unicode box-drawing characters (┌, ┐, └, ┘, │, ─)
- Menu items displayed as a vertical list with consistent padding
- Selected item highlighted with: inverted colors (white background, dark text), or bracket indicators like `[ > Option < ]`, or colored prefix arrow `►`
- Unselected items in neutral gray or dim white
- Menu sections separated by horizontal dividers

**Color Scheme:**
- Background: Black or dark terminal default
- Primary text: White or light gray
- Accent color: Cyan, red, or orange for highlights and borders
- Selected item: Bright white on colored background or bold colored text
- Status indicators: Green for success, Yellow for warnings, Red for errors

**Status Bar:**
- Bottom section showing current status (Admin/Standard mode)
- Keyboard shortcuts hint (e.g., "↑↓ Navigate | Enter Select | Esc Back")
- Detected games count or scan status

**Example Layout Structure:**
```
╔══════════════════════════════════════════════════════════════╗
║   __  __                   __  __        _                   ║
║  |  \/  |__ _ ______  ___ / _|/ _|___ __| |_                 ║
║  | |\/| / _` (_-<_-< / -_)  _|  _/ -_) _|  _|                ║
║  |_|  |_\__,_/__/__/ \___|_| |_| \___\__|\__|                ║
║                    LEGENDARY LAUNCHER v2.0                   ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║    ┌─────────────────────────────────────┐                   ║
║    │  ► Mass Effect 1 Legendary Edition  │  ← Selected       ║
║    │    Mass Effect 2 Legendary Edition  │                   ║
║    │    Mass Effect 3 Legendary Edition  │                   ║
║    │  ─────────────────────────────────  │                   ║
║    │    Mass Effect 1 (Original)         │                   ║
║    │    Mass Effect 2 (Original)         │                   ║
║    │    Mass Effect 3 (Original)         │                   ║
║    │  ─────────────────────────────────  │                   ║
║    │    ⚙ Settings                       │                   ║
║    │    ✕ Exit                           │                   ║
║    └─────────────────────────────────────┘                   ║
║                                                              ║
╠══════════════════════════════════════════════════════════════╣
║  [Admin Mode] │ 3 Games Detected │ ↑↓ Navigate │ Enter Select║
╚══════════════════════════════════════════════════════════════╝
```

### Requirement 2: Auto-Detection of Game Folders

**User Story:** As a user, I want the launcher to automatically find my Mass Effect game installations, so that I don't have to manually enter paths.

#### Acceptance Criteria

1. WHEN the Launcher starts, THE Game_Detector SHALL scan common installation locations for Mass Effect games
2. THE Game_Detector SHALL check Steam library folders by reading Steam's libraryfolders.vdf file
3. THE Game_Detector SHALL check EA App/Origin installation directories
4. THE Game_Detector SHALL check Windows Registry for installed game paths
5. THE Game_Detector SHALL check Program Files and Program Files (x86) directories
6. WHEN games are detected, THE Game_Detector SHALL validate the installation by checking for required executable files
7. WHEN multiple installations are found, THE Menu_System SHALL allow the user to select which installation to use
8. IF no games are detected, THEN THE Launcher SHALL prompt the user to manually enter the game path
9. WHEN a game path is manually entered, THE Config_Manager SHALL persist it for future launches

### Requirement 3: Administrator Elevation Support

**User Story:** As a user, I want the launcher to automatically request administrator privileges when needed, so that I can launch games installed on protected drives without manual elevation.

#### Acceptance Criteria

1. WHEN the Launcher starts, THE Admin_Elevator SHALL check if the application is running with administrator privileges
2. WHEN a game is located on a protected path (e.g., C:\Program Files), THE Admin_Elevator SHALL detect this condition
3. IF administrator privileges are required but not present, THEN THE Admin_Elevator SHALL prompt the user to restart with elevation
4. WHEN the user confirms elevation, THE Admin_Elevator SHALL restart the application with administrator privileges using Windows UAC
5. THE Menu_System SHALL display the current elevation status (Admin/Standard) in the UI
6. IF elevation fails or is declined, THEN THE Launcher SHALL display a clear error message explaining the limitation

### Requirement 4: Settings and Configuration Management

**User Story:** As a user, I want my preferences to be saved, so that I don't have to reconfigure the launcher each time.

#### Acceptance Criteria

1. THE Config_Manager SHALL store settings in a JSON configuration file
2. WHEN settings are modified, THE Config_Manager SHALL persist changes immediately
3. THE Config_Manager SHALL store detected game paths, preferred locale, and force feedback preferences
4. WHEN the configuration file is corrupted or missing, THE Config_Manager SHALL create a new default configuration
5. THE Menu_System SHALL provide a settings submenu to modify saved preferences

### Requirement 5: Game Launch Options

**User Story:** As a user, I want to configure game launch options through the menu, so that I can customize my gaming experience.

#### Acceptance Criteria

1. WHEN a Legendary Edition game is selected, THE Menu_System SHALL display options for force feedback and locale
2. THE Menu_System SHALL remember the last used settings for each game
3. WHEN launching a game, THE Launcher SHALL construct the appropriate command-line arguments based on selected options
4. THE Launcher SHALL support both Legendary Edition and Legacy (original trilogy) games
5. WHEN a game fails to launch, THE Launcher SHALL display a descriptive error message

### Requirement 6: Visual Feedback and Status Display

**User Story:** As a user, I want clear visual feedback about the launcher's status, so that I know what's happening.

#### Acceptance Criteria

1. WHEN scanning for games, THE Menu_System SHALL display a progress indicator
2. WHEN a game is launching, THE Menu_System SHALL display a launch confirmation message
3. THE Menu_System SHALL display detected games with their installation paths
4. IF a previously saved game path is no longer valid, THEN THE Launcher SHALL notify the user and offer to rescan