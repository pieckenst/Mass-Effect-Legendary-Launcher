# IntroPlayer Usage Guide

The IntroPlayer component now uses a **"hands-off" approach** for RAD Video Tools BinkPlay to prevent crashes and conflicts. When BinkPlay is used, **no custom video player controls are created** - BinkPlay handles everything itself.

## Key Features

1. **Automatic RAD Video Tools Detection**: Checks for installed RAD Video Tools at `C:\Program Files (x86)\RADVideo\binkplay.exe` first
2. **Smart Player Selection**: Uses Bink SDK for embedding, BinkPlay for fullscreen (hands-off), FFmpeg for fallback embedding
3. **Hands-Off BinkPlay**: When using BinkPlay, no custom forms or overlays are created - prevents crashes
4. **Game Launch Protection**: Automatically terminates all video processes before game launch to prevent conflicts
5. **Automatic Bink DLL Management**: Copies `binkw32.dll` from game installation automatically

## Player Selection Logic & Behavior

### 🎯 **Bink SDK** (Priority 1 - Best Option)
- **When**: `binkw32.dll` is available (auto-copied from game)
- **Fullscreen**: Native integration with perfect quality
- **Embedded**: True embedded playback in WinForms controls
- **ESC Support**: Custom ESC key handling
- **Process Management**: No external processes
- **Quality**: Perfect native .bik playback

### 🎮 **RAD Video Tools / BinkPlay** (Priority 2 - Optimal for .bik files)
- **When**: RAD Video Tools installed, Bink SDK not available
- **Fullscreen**: BinkPlay handles its own window - **NO custom overlays**
- **Embedded**: **Not supported** - falls back to fullscreen mode
- **ESC Support**: BinkPlay handles ESC internally - **NO custom handling**
- **Process Management**: Simple process start and wait - **NO custom forms**
- **Key Point**: **Completely hands-off to prevent crashes**
- **Quality**: Excellent native .bik playback (better than FFmpeg)

### 🔄 **FFmpeg** (Priority 3 - Last Resort Fallback)
- **When**: Neither Bink SDK nor RAD Video Tools available
- **Fullscreen**: Custom overlay form with ESC handling
- **Embedded**: Window embedding with parent control
- **ESC Support**: Custom ESC key handling
- **Process Management**: Full custom control and embedding
- **Quality**: ⚠️ **Known issues with .bik playback** - may show broken/corrupted video
- **Note**: FFmpeg has limited Bink video codec support and often produces broken playback

## Usage Examples

### Basic Fullscreen Playback (Current Usage)
```csharp
var introPlayer = new IntroPlayer();
bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true);

// Behavior:
// 1. Tries Bink SDK first (native integration - best quality)
// 2. Falls back to RAD Video Tools/BinkPlay (hands-off mode - excellent quality)
// 3. Last resort: FFmpeg (custom overlay - may have broken playback for .bik files)
```

### Embedded Playback in a WinForms Control
```csharp
var introPlayer = new IntroPlayer();
var videoPanel = new Panel { Dock = DockStyle.Fill };
parentForm.Controls.Add(videoPanel);

bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true, videoPanel);

// Behavior:
// 1. Tries Bink SDK first (true embedding - best quality)
// 2. RAD Video Tools doesn't support embedding - uses fullscreen instead (excellent quality)
// 3. Falls back to FFmpeg (window embedding - may have broken playback)
```

### Integration with WPF (using WindowsFormsHost)
```csharp
// In WPF window
var winFormsHost = new WindowsFormsHost();
var videoPanel = new System.Windows.Forms.Panel { Dock = DockStyle.Fill };
winFormsHost.Child = videoPanel;
wpfGrid.Children.Add(winFormsHost);

var introPlayer = new IntroPlayer();
bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true, videoPanel);

// Uses Bink SDK for true embedding, or FFmpeg if SDK not available
// BinkPlay will ignore the parent control and go fullscreen
```

## Why RAD Video Tools is Prioritized Over FFmpeg

### ❌ FFmpeg Issues with .bik Files
- FFmpeg has **limited Bink video codec support**
- Often produces **broken/corrupted playback** with visual artifacts
- May show black screen or garbled video
- Not designed for proprietary Bink format

### ✅ RAD Video Tools Advantages
- **Native .bik file support** - designed specifically for Bink videos
- **Perfect playback quality** - no corruption or artifacts
- **Hands-off approach** - BinkPlay manages everything itself
- **Built-in ESC handling** - no custom code needed
- **Reliable and stable** - industry-standard tool

### Priority Order Rationale
1. **Bink SDK**: Best option - native integration with perfect quality
2. **RAD Video Tools**: Second best - native .bik support, excellent quality
3. **FFmpeg**: Last resort - known issues with .bik playback

## Why BinkPlay Uses "Hands-Off" Mode

### ❌ Previous Issues (Fixed)
- BinkPlay would open its own window first
- Custom overlay forms would conflict with BinkPlay
- Window embedding attempts would fail and cause crashes
- Process management conflicts between custom code and BinkPlay
- Launcher would crash when trying to manage BinkPlay's window

### ✅ New "Hands-Off" Approach
- **No custom forms created** when using BinkPlay
- **No overlay windows** - BinkPlay handles its own display
- **No ESC key handling** - BinkPlay handles ESC internally
- **Simple process management** - just start and wait for completion
- **No window embedding attempts** - prevents crashes
- **Clean process termination** - just kill the process when done

```csharp
// BinkPlay "hands-off" approach:
_videoProcess = Process.Start(binkPlayerPath, videoPath);
await Task.Run(() => _videoProcess.WaitForExit()); // Just wait, no custom handling
// No cleanup needed - we didn't create any custom forms
```

## Game Launch Protection

The GameLauncher now includes automatic video player cleanup:

```csharp
// This is automatically called in GameLauncher.LaunchAsync()
await _introPlayer.EnsureVideoPlayersClosed();
```

This ensures:
- All `binkplay.exe` processes are terminated
- All `radvideo32.exe` and `radvideo64.exe` processes are terminated  
- All `ffplay.exe` processes are terminated
- No video player processes interfere with game launch
- Proper cleanup even if video was skipped or interrupted

## Technical Details

- **Smart Player Selection**: Automatically chooses best player for the mode
- **FFmpeg Embedding**: Uses `-noborder -x width -y height` for proper window control
- **Window API Integration**: Uses `SetParent` and `SetWindowPos` for FFmpeg embedding
- **Aggressive Cleanup**: Terminates all video processes before game launch
- **Thread-Safe Operations**: Proper cross-thread form handling
- **Process Management**: Handles both graceful and forced termination

## Notes

- **Fullscreen Mode**: Prioritizes RAD Video Tools for best .bik file quality
- **Embedded Mode**: Uses Bink SDK if available, otherwise RAD Video Tools in fullscreen
- **FFmpeg Fallback**: Only used as last resort due to known .bik playback issues
- **WPF Integration**: Use `WindowsFormsHost` to wrap a WinForms Panel
- **Process Cleanup**: All video processes are terminated before game launch
- **Quality Priority**: Native .bik players (Bink SDK, RAD Tools) always preferred over FFmpeg
- **Broken Playback Warning**: If you see corrupted video, FFmpeg is being used - install RAD Video Tools for proper playback