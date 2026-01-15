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

### 🎮 **BinkPlay.exe** (Priority 2 - Hands-Off Mode)
- **When**: RAD Video Tools installed, Bink SDK not available
- **Fullscreen**: BinkPlay handles its own window - **NO custom overlays**
- **Embedded**: **Not supported** - falls back to fullscreen mode
- **ESC Support**: BinkPlay handles ESC internally - **NO custom handling**
- **Process Management**: Simple process start and wait - **NO custom forms**
- **Key Point**: **Completely hands-off to prevent crashes**

### 🔄 **FFmpeg** (Priority 3 - Fallback)
- **When**: Neither Bink SDK nor BinkPlay available
- **Fullscreen**: Custom overlay form with ESC handling
- **Embedded**: Window embedding with parent control
- **ESC Support**: Custom ESC key handling
- **Process Management**: Full custom control and embedding

## Usage Examples

### Basic Fullscreen Playback (Current Usage)
```csharp
var introPlayer = new IntroPlayer();
bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true);

// Behavior:
// 1. Tries Bink SDK first (native integration)
// 2. Falls back to BinkPlay (hands-off mode - no custom controls)
// 3. Falls back to FFmpeg (custom overlay with ESC handling)
```

### Embedded Playback in a WinForms Control
```csharp
var introPlayer = new IntroPlayer();
var videoPanel = new Panel { Dock = DockStyle.Fill };
parentForm.Controls.Add(videoPanel);

bool success = await introPlayer.PlayBioWareIntroAsync(gamePath, allowSkip: true, videoPanel);

// Behavior:
// 1. Tries Bink SDK first (true embedding)
// 2. BinkPlay doesn't support embedding - uses fullscreen instead (hands-off)
// 3. Falls back to FFmpeg (window embedding with custom controls)
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

- **Fullscreen Mode**: Always uses BinkPlay for best .bik file quality
- **Embedded Mode**: Always uses FFmpeg for reliable embedding
- **WPF Integration**: Use `WindowsFormsHost` to wrap a WinForms Panel
- **Process Cleanup**: All video processes are terminated before game launch
- **No More Stuck Windows**: FFmpeg embedding is much more reliable than BinkPlay
- **Quality Trade-off**: Embedded mode uses FFmpeg instead of BinkPlay, but quality is still excellent