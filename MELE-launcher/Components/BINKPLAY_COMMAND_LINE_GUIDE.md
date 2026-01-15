# BinkPlay Modern Command-Line Arguments Guide

The launcher now uses the latest BinkPlay v2025.05 command-line arguments for optimal video playback with proper scaling and automatic operation.

## Modern BinkPlay v2025.05 Syntax

### ✅ **Resolution & Scaling**
- **`/W=-1`**: Fullscreen mode (replaces old `-1`)
- **`/W=width`**: Set specific window width in pixels (replaces old `-w`)
- **`/H=height`**: Set specific window height in pixels (replaces old `-h`)

### ✅ **Window Style**
- **`/I=0`**: Normal window with caption
- **`/I=1`**: No caption (cleaner look)
- **`/I=2`**: No window decorations at all
- **`/I=100`**: Always on top

### ✅ **Automation & Control**
- **`/P`**: Preload entire video into memory (smoother playback)
- **`/L=0`**: Don't loop (play once and stop)
- **`/U=4`**: Disable controls and quit on completion (replaces old `/#`)
- **`/G`**: Don't fill read buffer when opening (faster startup)

### ✅ **Quality & Performance**
- **`/N`**: Never skip frames when falling behind (better quality)
- **`/C`**: Hide cursor when in the Bink window
- **`/Z=0`**: DirectSound output (most compatible)
- **`/A=0`**: Normal display options

## Implementation in Launcher

### **Fullscreen Mode Arguments**
```bash
radvideo64.exe "BWLogo1.bik" /W=-1 /P /L=0 /U=4 /G /N /Z=0 /A=0
```

### **Windowed Mode Arguments** 
```bash
radvideo64.exe "BWLogo1.bik" /W=800 /H=600 /I=1 /C /P /L=0 /U=4 /G /N /Z=0 /A=0
```

## Key Improvements from Old Syntax

### ✅ **Modern Syntax**
- **Old**: `-1`, `-w 800`, `-h 600`, `/#`
- **New**: `/W=-1`, `/W=800`, `/H=600`, `/U=4`

### ✅ **Better Automation**
- **`/U=4`**: More reliable than old `/#` for auto-quit
- **`/P`**: Preload for smoother playback
- **`/G`**: Faster startup

### ✅ **Enhanced Quality**
- **`/N`**: Never skip frames (better quality)
- **`/Z=0`**: DirectSound for compatibility
- **`/A=0`**: Optimal display settings

### ✅ **Cleaner Windows**
- **`/I=1`**: No caption for embedded-like appearance
- **`/C`**: Hide cursor for cleaner playback

## Complete Modern Command Reference

### **Basic Playback**
- `/P` - Preload entire video into memory
- `/L=#` - Loop animation (0=no loop, blank=infinite)
- `/F=#` - Use specific frame rate

### **Window Control**
- `/W=#` - Width in pixels (-1 for fullscreen)
- `/H=#` - Height in pixels  
- `/X=#` - X coordinate of playback
- `/Y=#` - Y coordinate of playback
- `/I=#` - Window style (0=normal, 1=no caption, 2=nothing, 100=always-on-top)

### **Audio**
- `/Z=#` - Sound output (0=DirectSound, 1=waveOut, 2=XAudio)
- `/S` - Show playback summary when finished
- `/M=#` - Simulate read speed (bytes/second)

### **Display & Quality**
- `/A=#` - Display options (0=normal, 100=grayscale, 1000=flip luma/alpha, 10000=turn off alpha)
- `/N` - Never skip frames when falling behind
- `/Q` - Show run-time playback statistics
- `/C` - Hide cursor when in Bink window

### **Control & Automation**
- `/U=#` - Disable controls (1=pause, 2=skip, 4=quit, 8=slider)
- `/G` - Don't fill read buffer when opening
- `/R` - Do not use multi-threaded device reading

## Usage in Launcher Code

```csharp
private string BuildBinkPlayArguments(string videoPath, Control parentControl)
{
    var args = new List<string> { $"\"{videoPath}\"" };
    
    if (parentControl != null)
    {
        // Windowed mode for "embedded" experience
        args.Add($"/W={parentControl.Width}");
        args.Add($"/H={parentControl.Height}");
        args.Add("/I=1");  // No caption
        args.Add("/C");    // Hide cursor
    }
    else
    {
        // True fullscreen
        args.Add("/W=-1");
    }
    
    // Essential automation and quality
    args.Add("/P");     // Preload
    args.Add("/L=0");   // No loop
    args.Add("/U=4");   // Auto-quit
    args.Add("/G");     // Fast start
    args.Add("/N");     // No frame skip
    args.Add("/Z=0");   // DirectSound
    args.Add("/A=0");   // Normal display
    
    return string.Join(" ", args);
}
```

## Benefits of Modern Syntax

### ✅ **More Reliable**
- `/U=4` is more reliable than old `/#` for auto-closing
- `/P` preloading prevents stuttering
- `/G` provides faster startup

### ✅ **Better Quality**
- `/N` ensures no frames are skipped
- `/Z=0` uses most compatible audio output
- `/A=0` optimal display settings

### ✅ **Cleaner Integration**
- `/I=1` removes window caption for cleaner look
- `/C` hides cursor during playback
- Better windowed mode for "embedded" scenarios

This modern implementation provides the most reliable and highest quality BinkPlay integration possible.