# Changelog

All notable changes to the Mass Effect Legendary Launcher will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **BioWare Intro Video**: Plays the authentic BioWare logo intro (BWLogo1.bik) before game launch
  - Automatically detects intro video in Legendary Edition installations
  - Fullscreen playback with ESC key to skip
  - Can be disabled with `-nointro` command-line flag or in silent mode
  - Only plays for Legendary Edition games (not Original Trilogy)
- Command-line argument support for direct game launching without interactive menu
  - Legendary Edition: `-ME(1|2|3) -yes|-no LanguageCode [-silent] [-nointro]`
  - Original Trilogy: `-OLDME(1|2|3) [-silent]`
- Silent mode flag (`-silent`) to suppress console output during command-line launches
- Comprehensive language code mapping system supporting all BioWare locale codes
- Separate text and voice language selection in interactive mode
- Automatic detection of English voice-over codes (FE, GE, IE, RU, PL, etc.)
- Legacy language code compatibility with old launcher format
- Exit code support (0 for success, 1 for errors)

### Changed
- Improved game path detection for Legendary Edition installations
- Fixed menu navigation bug where down arrow key wasn't working
- Enhanced language selection UI to show native voice-over availability
- Updated launch arguments to include all required Mass Effect parameters

### Fixed
- Game detection now correctly identifies all three Legendary Edition games
- Menu selection index no longer resets on every key press
- Force feedback settings now properly applied to game launch
- Language codes correctly mapped between ME1 and ME2/ME3 formats

## [1.0.0] - Initial Release

### Added
- Interactive terminal UI using Spectre.Console
- Automatic game detection for Mass Effect Legendary Edition
- Manual game path configuration
- Admin elevation support for games requiring administrator privileges
- Configuration persistence using JSON
- Game launch with proper arguments and working directory
- Settings menu for launcher preferences
