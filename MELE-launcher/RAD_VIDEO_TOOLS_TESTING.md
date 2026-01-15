# RAD Video Tools Testing Guide

This document explains how to test the RAD Video Tools downloader functionality in the Mass Effect Legendary Launcher.

## Overview

The launcher now supports playing the authentic BioWare intro video (`BWLogo1.bik`) before launching Mass Effect games. This requires RAD Video Tools' BinkPlay.exe for optimal Bink video file support.

## Testing the RAD Downloader

### Command Line Test
Run the launcher with the test flag:
```
MassEffectLauncher.exe -test-rad
```

This will:
1. Check if BinkPlay.exe is already available
2. If not, attempt to download RAD Video Tools from the official site
3. Extract the password-protected 7z archive
4. Install RAD Tools (if installer is found)
5. Locate and set up BinkPlay.exe
6. Test the intro player with a non-existent path (should handle gracefully)

### What the Test Does

#### RAD Video Tools Download Process:
1. **Download**: Downloads `RADTools.7z` from `https://www.radgametools.com/down/Bink/RADTools.7z`
2. **Extract**: Attempts extraction using:
   - System 7-Zip installation (if available)
   - PowerShell with 7Zip4PowerShell module (if available)
   - Standalone 7za.exe download as fallback
3. **Install**: Runs any found installer executable with silent flags
4. **Locate**: Searches for BinkPlay.exe in:
   - Standard installation paths (`Program Files\RADGameTools\`)
   - Local extracted files
   - Recursive search in extraction directory

#### Intro Player Test:
- Tests graceful handling of missing video files
- Verifies error handling and cleanup

## Expected Results

### Success Case:
```
🧪 Testing RAD Video Tools Downloader...
✅ BinkPlay.exe is already available!

🧪 Testing Intro Player...
✅ Intro player correctly handled missing video file

✅ All tests completed!
```

### Download Case:
```
🧪 Testing RAD Video Tools Downloader...
📥 BinkPlay.exe not found, attempting download...
📥 Downloading RAD Video Tools...
📦 Extracting RAD Tools (password protected)...
🔧 Installing RAD Video Tools...
✅ RAD Bink Player ready!

🧪 Testing Intro Player...
✅ Intro player correctly handled missing video file

✅ All tests completed!
```

### Failure Cases:
The test may fail due to:
- **Network issues**: Cannot download RAD Tools
- **Missing 7-Zip**: Cannot extract password-protected archive
- **Antivirus blocking**: Download or extraction blocked
- **Server unavailable**: RAD Game Tools server down
- **Permission issues**: Cannot write to application directory

## Fallback Behavior

If RAD Video Tools fails to download/install:
1. The launcher will fall back to FFmpeg for video playback
2. FFmpeg will be automatically downloaded if needed
3. Video playback may have reduced quality/compatibility with Bink files

## Manual Installation

If automatic download fails, you can manually:
1. Download RAD Video Tools from: https://www.radgametools.com/down/Bink/RADTools.7z
2. Extract with password: `RAD.SHA1`
3. Install RAD Tools
4. Copy `BinkPlay.exe` to: `[launcher directory]/radtools/BinkPlay.exe`

## Integration with Game Launch

When launching a Mass Effect Legendary Edition game:
1. If intro is enabled (default), the launcher will:
   - Look for `BWLogo1.bik` in the game's `Game/Launcher/Content/` directory
   - Download/setup RAD Tools if needed (first time only)
   - Play the intro video fullscreen
   - Allow skipping with ESC key
   - Launch the game after intro completes

## Command Line Options

- `-nointro`: Skip intro video playback
- `-silent`: Run without console output (also skips intro)

## Troubleshooting

### "Could not extract RAD Tools - 7-Zip extraction failed"
- Install 7-Zip from https://www.7-zip.org/
- Or manually extract the RAD Tools archive

### "RAD Tools installation may have failed"
- The installer may require administrator privileges
- Try running the launcher as administrator
- Or manually install RAD Tools

### "Bink Player not found after installation"
- RAD Tools may have installed to a non-standard location
- Check `Program Files\RAD Game Tools\` for BinkPlay.exe
- Manually copy to `[launcher directory]/radtools/BinkPlay.exe`

### Video playback issues
- Ensure the Mass Effect Legendary Edition is properly installed
- Check that `BWLogo1.bik` exists in `Game/Launcher/Content/`
- Try running as administrator if video player fails to start