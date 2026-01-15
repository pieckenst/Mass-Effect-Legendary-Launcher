# Implementation Plan: Enhanced Terminal UI

## Overview

This plan implements the enhanced terminal UI for the Mass Effect Legendary Launcher using C# with .NET 6+ and Spectre.Console for rich terminal rendering. Tasks are ordered to build incrementally, with core infrastructure first, then features, then integration.

## Tasks

- [x] 1. Project Setup and Dependencies
  - Add Spectre.Console NuGet package for rich terminal UI
  - Add System.Text.Json for configuration serialization
  - Add FsCheck NuGet package for property-based testing
  - Update project to .NET 6.0 or later if needed
  - Create folder structure for new components
  - _Requirements: 1.1, 1.6_

- [x] 2. Implement Configuration Management
  - [x] 2.1 Create data models (LauncherConfig, GameConfig)
    - Define LauncherConfig class with Games list, DefaultLocale, DefaultForceFeedback, LastScanDate
    - Define GameConfig class with Type, Edition, Path, Locale, ForceFeedback
    - Define GameType and GameEdition enums
    - _Requirements: 4.1, 4.3_

  - [x] 2.2 Implement ConfigManager class
    - Implement Load() method to read JSON config from file
    - Implement Save() method to write JSON config to file
    - Implement GetConfigPath() to return config file location
    - Handle missing/corrupted config by creating defaults
    - _Requirements: 4.1, 4.2, 4.4_

  - [ ]* 2.3 Write property test for configuration round-trip
    - **Property 3: Configuration Round-Trip Consistency**
    - **Validates: Requirements 4.1, 4.3, 2.9, 5.2**

- [x] 3. Implement Administrator Elevation
  - [x] 3.1 Create AdminElevator class
    - Implement IsRunningAsAdmin() using WindowsIdentity/WindowsPrincipal
    - Implement IsProtectedPath() to check for Program Files, Windows directories
    - Implement RequiresElevation() combining path check with current privileges
    - Implement RequestElevation() to restart with runas verb
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [ ]* 3.2 Write property test for protected path detection
    - **Property 4: Protected Path Detection**
    - **Validates: Requirements 3.2**

- [x] 4. Implement Game Detection
  - [x] 4.1 Create DetectedGame model and GamePaths constants
    - Define DetectedGame class with Name, Path, ExecutablePath, Type, Edition, IsValid, RequiresAdmin
    - Define static GamePaths class with LegendaryPaths, OriginalPaths, CommonDirectories
    - _Requirements: 2.1, 2.6_

  - [x] 4.2 Implement GameDetector class
    - Implement GetSteamLibraryPaths() to parse libraryfolders.vdf
    - Implement GetEAAppPaths() to check EA/Origin directories
    - Implement GetRegistryPaths() to read game paths from Windows Registry
    - Implement GetCommonPaths() to return standard installation directories
    - Implement ValidateInstallation() to check executable existence
    - Implement ScanForGames() to aggregate all sources and validate
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

  - [ ]* 4.3 Write property test for installation validation
    - **Property 2: Installation Validation Requires Executable**
    - **Validates: Requirements 2.6**

- [x] 5. Implement Game Launcher
  - [x] 5.1 Create LaunchOptions and LaunchResult models
    - Define LaunchOptions class with Locale, ForceFeedback, Silent
    - Define LaunchResult class with Success, ErrorMessage
    - _Requirements: 5.1, 5.3_

  - [x] 5.2 Implement GameLauncher class
    - Implement BuildArguments() to construct command-line args based on game edition and options
    - Implement CreateStartInfo() to configure ProcessStartInfo with elevation if needed
    - Implement Launch() to start the game process and return result
    - _Requirements: 5.3, 5.4, 5.5_

  - [ ]* 5.3 Write property test for argument construction
    - **Property 5: Launch Argument Construction**
    - **Validates: Requirements 5.3**

- [ ] 6. Checkpoint - Core Components
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Implement Menu System
  - [x] 7.1 Create MenuItem model and menu enums
    - Define MenuItem class with Title, Description, OnSelect, IsEnabled, Type
    - Define MenuItemType enum (Game, Setting, Action, Separator)
    - Define MessageType enum (Info, Success, Warning, Error)
    - Define MenuState enum (Main, GameOptions, Settings, Confirmation)
    - _Requirements: 1.2, 1.7_

  - [x] 7.2 Implement MenuSystem class - Core rendering
    - Implement RenderHeader() to display ASCII art logo using Spectre.Console
    - Implement RenderMenu() to display menu items with box borders
    - Implement RenderStatusBar() to show admin status and keyboard hints
    - Implement Render() to compose full screen layout
    - _Requirements: 1.1, 1.2, 1.6, 3.5_

  - [x] 7.3 Implement MenuSystem class - Navigation
    - Implement HandleInput() to process arrow keys, Enter, Escape
    - Implement navigation with wrapping (last→first, first→last)
    - Implement menu state transitions
    - _Requirements: 1.3, 1.4, 1.5_

  - [ ]* 7.4 Write property test for menu navigation
    - **Property 1: Menu Navigation Wraps Correctly**
    - **Validates: Requirements 1.3**

  - [x] 7.5 Implement MenuSystem class - Feedback
    - Implement ShowMessage() to display colored messages
    - Implement ShowProgress() to display spinner during operations
    - Implement ShowConfirmation() for yes/no prompts
    - _Requirements: 6.1, 6.2_

- [x] 8. Implement Settings Submenu
  - [x] 8.1 Create settings menu items
    - Add menu items for default locale selection
    - Add menu items for default force feedback toggle
    - Add menu item to rescan for games
    - Add menu item to manually add game path
    - _Requirements: 4.5, 5.1, 2.8_

  - [x] 8.2 Implement settings persistence
    - Wire settings changes to ConfigManager.Save()
    - Load saved settings on startup
    - _Requirements: 4.2, 5.2_

- [x] 9. Implement Game Options Submenu
  - [x] 9.1 Create game options menu
    - Display locale selection for Legendary Edition games
    - Display force feedback toggle for Legendary Edition games
    - Display launch button
    - _Requirements: 5.1, 5.2_

  - [x] 9.2 Wire game launch flow
    - Check if elevation is required before launch
    - Prompt for elevation if needed
    - Launch game with selected options
    - Display success/error message
    - _Requirements: 3.3, 5.3, 5.5, 6.2_

- [x] 10. Integrate All Components in Program.cs
  - [x] 10.1 Refactor Program.cs entry point
    - Initialize ConfigManager and load settings
    - Initialize AdminElevator and check current status
    - Initialize GameDetector and scan for games
    - Initialize MenuSystem with detected games
    - _Requirements: 1.1, 2.1, 3.1_

  - [x] 10.2 Implement main application loop
    - Render menu on each iteration
    - Handle keyboard input
    - Process menu selections
    - Handle exit gracefully
    - _Requirements: 1.3, 1.4, 1.5_

  - [x] 10.3 Handle edge cases
    - Display message when no games detected
    - Handle invalid saved paths with rescan offer
    - Handle elevation decline gracefully
    - _Requirements: 2.8, 3.6, 6.4_

- [ ] 11. Final Checkpoint
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- Uses Spectre.Console for rich terminal rendering
- Uses FsCheck for property-based testing in C#
