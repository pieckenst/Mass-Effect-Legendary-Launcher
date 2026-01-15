# Bink SDK Integration Guide

The launcher now includes native Bink SDK integration for the best possible video playback experience. This is based on the official Bink SDK C++ example code and provides proper embedded video support.

## Integration Priority

The IntroPlayer now uses this priority order:

1. **Bink SDK** (Native integration - best quality and embedding)
2. **BinkPlay.exe** (External process - fullscreen only)
3. **FFmpeg** (Fallback - good embedding support)

## Bink SDK Advantages

### ✅ Native Integration
- No external processes to manage
- Direct memory access to video frames
- Perfect synchronization with application

### ✅ Proper Embedding Support
- True embedded playback in WinForms controls
- No window management issues
- No stuck video player windows

### ✅ Best Performance
- Hardware-accelerated decoding
- Minimal CPU overhead
- Optimal memory usage

### ✅ Complete Control
- Frame-by-frame processing
- Precise timing control
- Custom rendering options

## Requirements

### Required Files
- `binkw32.dll` - Bink SDK runtime library
- **Automatically copied from Mass Effect Legendary Edition game installation**

### Automatic DLL Management
The launcher now automatically finds and copies `binkw32.dll` from:

1. **Mass Effect Legendary Edition Game Directory** (Primary source)
   - Steam: `steamapps/common/Mass Effect Legendary Edition`
   - Origin/EA: `Origin Games/Mass Effect Legendary Edition`
   - Epic: `Epic Games/MassEffectLegendaryEdition`
   - Xbox Game Pass: Various WindowsApps locations

2. **RAD Video Tools Installation** (Fallback)
   - `C:\Program Files (x86)\RADVideo\`
   - `C:\Program Files\RAD Game Tools\`

3. **Launcher Directory** (Local copy)
   - Automatically maintained in launcher folder
   - No manual installation required

### Smart DLL Discovery
```csharp
// Automatically finds and copies binkw32.dll
bool available = BinkDLLManager.EnsureBinkDLL(gamePath);

// Searches these locations in the game directory:
// - Game/ME1/Binaries/Win64/binkw32.dll
// - Game/ME2/Binaries/Win64/binkw32.dll  
// - Game/ME3/Binaries/Win64/binkw32.dll
// - Game/Launcher/binkw32.dll
// - Binaries/Win64/binkw32.dll
// - Root directory recursive search
```

## Usage Examples

### Automatic Usage (Recommended)
```csharp
var introPlayer = new IntroPlayer();

// Automatically uses Bink SDK if available
bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true, parentControl);
```

### Direct Bink SDK Usage
```csharp
using var binkPlayer = new BinkSDKPlayer();

// Check if SDK is available
if (BinkSDKPlayer.IsSDKAvailable)
{
    // Play video with full control
    bool success = await binkPlayer.PlayVideoAsync("video.bik", parentControl, allowSkip: true);
}
```

### Manual Frame Processing
```csharp
using var binkPlayer = new BinkSDKPlayer();

if (binkPlayer.OpenVideo("video.bik", parentControl))
{
    binkPlayer.StartPlayback();
    
    while (binkPlayer.IsPlaying)
    {
        if (!binkPlayer.ProcessFrame())
        {
            break; // Video completed
        }
        
        // Custom rendering or processing here
        await Task.Delay(16); // ~60 FPS
    }
}
```

## Technical Implementation

### Based on Official C++ Example
The implementation follows the official Bink SDK C++ example:

```cpp
// C++ equivalent
BINK * Bink = BinkOpen("video.bik", BINKSNDTRACK|BINKNOFRAMEBUFFERS);
while (!BinkWait(Bink)) {
    BinkDoFrame(Bink);
    BinkNextFrame(Bink);
}
BinkClose(Bink);
```

### C# P/Invoke Wrapper
```csharp
[DllImport("binkw32.dll")]
private static extern IntPtr BinkOpen(string filename, uint flags);

[DllImport("binkw32.dll")]
private static extern int BinkWait(IntPtr bink);

[DllImport("binkw32.dll")]
private static extern void BinkDoFrame(IntPtr bink);
```

## Features

### ✅ Embedded Playback
- True embedding in WinForms controls
- Proper parent-child window relationships
- No external window management

### ✅ ESC Key Support
- Skip video with ESC key
- Invisible key handler form
- Non-blocking input processing

### ✅ Resource Management
- Automatic cleanup with `using` statements
- Proper disposal of native resources
- Memory leak prevention

### ✅ Error Handling
- Graceful fallback to other players
- Detailed error logging
- Exception safety

## Fallback Behavior

If Bink SDK is not available:

1. **BinkPlay.exe**: Uses external BinkPlay process (fullscreen only)
2. **FFmpeg**: Uses FFplay for embedded playback
3. **Skip**: Gracefully skips intro if no players available

## Deployment Notes

### For Distribution
- **No manual DLL management required!**
- Launcher automatically copies `binkw32.dll` from game installation
- Works out-of-the-box with any Mass Effect Legendary Edition installation
- No licensing concerns (using DLL from user's own game)

### For Development
- Launcher will find DLL from your game installation
- No need to manually copy files
- Automatic validation and copying

### Automatic DLL Management
```csharp
// The launcher handles this automatically:
BinkDLLManager.EnsureBinkDLL(gamePath);

// Provides these features:
// - Finds DLL in game installation
// - Copies to launcher directory  
// - Validates DLL integrity
// - Handles multiple installation paths
// - Fallback to RAD Video Tools if needed
```

## Troubleshooting

### "Bink SDK not available"
- **Usually auto-resolved**: Launcher finds DLL in game installation
- Verify Mass Effect Legendary Edition is installed
- Check game installation is complete and not corrupted
- Fallback: Install RAD Video Tools manually

### Video doesn't play
- Verify .bik file exists and is valid
- Check file permissions
- Ensure DirectSound is available
- Try running launcher as administrator

### DLL not found in game
- Game installation may be incomplete
- Try verifying game files through Steam/Origin/Epic
- Check if game is installed in non-standard location
- Manual fallback: Install RAD Video Tools

### Performance Tips
- DLL is copied once and reused
- No performance impact from DLL discovery
- Automatic validation prevents corrupted DLLs
- Local copy eliminates repeated searches

This native integration provides the best possible Bink video experience with proper embedding support and no external process management issues.