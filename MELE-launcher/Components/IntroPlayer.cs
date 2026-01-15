using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Handles playing the BioWare intro video before game launch.
    /// </summary>
    public class IntroPlayer
    {
        private Process _videoProcess;
        private bool _skipRequested = false;
        private Form _videoForm;
        private Panel _videoPanel;
        private readonly RadVideoToolsDownloader _radDownloader;
        private readonly FFmpegDownloader _ffmpegDownloader;

        // Windows API for embedding external processes
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const int SW_MAXIMIZE = 3;

        public IntroPlayer()
        {
            _radDownloader = new RadVideoToolsDownloader();
            _ffmpegDownloader = new FFmpegDownloader();
            
            // Initialize Windows Forms application if not already done
            if (!System.Windows.Forms.Application.MessageLoop)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            }
        }

        /// <summary>
        /// Plays the BioWare intro video if it exists.
        /// </summary>
        /// <param name="gamePath">The base path of the Mass Effect Legendary Edition installation.</param>
        /// <param name="allowSkip">Whether to allow skipping the intro with ESC key.</param>
        /// <param name="parentControl">Optional parent control to embed the video player into.</param>
        /// <returns>True if the intro was played successfully, false otherwise.</returns>
        public async Task<bool> PlayBioWareIntroAsync(string gamePath, bool allowSkip = true, Control parentControl = null)
        {
            try
            {
                string introPath = Path.Combine(gamePath, "Game", "Launcher", "Content", "BWLogo1.bik");
                
                if (!File.Exists(introPath))
                {
                    Console.WriteLine("⚠ BioWare intro video not found.");
                    return false;
                }

                Console.WriteLine("🎬 Preparing BioWare intro...");

                // Step 1: Try Bink SDK first (best option - native integration)
                if (BinkSDKPlayer.IsSDKAvailable)
                {
                    Console.WriteLine("🎯 Using Bink SDK for native video playback...");
                    return await PlayWithBinkSDKAsync(introPath, allowSkip, parentControl);
                }

                // Step 2: Check FFmpeg compatibility before trying RAD Video Tools
                Console.WriteLine("🔍 Checking FFmpeg compatibility with .bik file...");
                var videoInfo = await VideoCompatibilityChecker.CheckFFmpegCompatibilityAsync(introPath, _ffmpegDownloader);
                
                if (videoInfo.IsDecodable)
                {
                    Console.WriteLine($"✅ FFmpeg can decode .bik file ({videoInfo.Width}x{videoInfo.Height})");
                    string ffplayPath = _ffmpegDownloader.GetFFplayPath();
                    if (ffplayPath != null)
                    {
                        return await PlayWithFFplayAsync(introPath, ffplayPath, allowSkip, parentControl);
                    }
                }
                else
                {
                    Console.WriteLine($"❌ FFmpeg cannot decode .bik file: {videoInfo.ErrorMessage}");
                }

                // Step 3: Fallback to RAD Video Tools with proper aspect-fit scaling
                string binkPlayerPath = GetInstalledBinkPlayerPath();
                if (binkPlayerPath != null)
                {
                    Console.WriteLine("🎮 Using RAD Video Tools with aspect-fit scaling...");
                    return await PlayWithBinkPlayerAsync(introPath, binkPlayerPath, allowSkip, parentControl, videoInfo);
                }

                // Step 4: Try to ensure RAD Video Tools via downloader
                binkPlayerPath = await _radDownloader.EnsureBinkPlayerAsync();
                if (binkPlayerPath != null)
                {
                    return await PlayWithBinkPlayerAsync(introPath, binkPlayerPath, allowSkip, parentControl, videoInfo);
                }

                Console.WriteLine("⚠ Could not set up video player. Skipping intro.");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing intro: {ex.Message}");
                Console.WriteLine("⚠ Could not play intro video.");
                return false;
            }
        }

        /// <summary>
        /// Plays video using the native Bink SDK (best option).
        /// </summary>
        private async Task<bool> PlayWithBinkSDKAsync(string videoPath, bool allowSkip, Control parentControl = null)
        {
            try
            {
                // Ensure we have the Bink DLL available (copy from game if needed)
                string gameDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(videoPath))); // Go up from Game/Launcher/Content to root
                BinkDLLManager.EnsureBinkDLL(gameDirectory);

                using var binkPlayer = new BinkSDKPlayer();
                
                if (parentControl != null)
                {
                    Console.WriteLine("🎬 Playing BioWare intro with Bink SDK (embedded mode)...");
                }
                else
                {
                    Console.WriteLine("🎬 Playing BioWare intro with Bink SDK (fullscreen mode)...");
                }

                if (allowSkip)
                {
                    Console.WriteLine("Press ESC to skip intro");
                }

                bool success = await binkPlayer.PlayVideoAsync(videoPath, parentControl, allowSkip);

                if (success)
                {
                    Console.WriteLine("✓ Intro completed with Bink SDK.");
                }
                else
                {
                    Console.WriteLine("⚠ Bink SDK playback failed.");
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing video with Bink SDK: {ex.Message}");
                Console.WriteLine("⚠ Could not play intro with Bink SDK.");
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the installed RAD Video Tools Bink Player.
        /// </summary>
        /// <returns>Path to binkplay.exe if found, null otherwise.</returns>
        private string GetInstalledBinkPlayerPath()
        {
            // Check the default installation path first
            var defaultPath = @"C:\Program Files (x86)\RADVideo\binkplay.exe";
            if (File.Exists(defaultPath))
            {
                return defaultPath;
            }

            // Check other common installation paths
            var possiblePaths = new[]
            {
                @"C:\Program Files\RADVideo\binkplay.exe",
                @"C:\Program Files (x86)\RAD Game Tools\binkplay.exe",
                @"C:\Program Files\RAD Game Tools\binkplay.exe",
                @"C:\Program Files (x86)\RADGameTools\binkplay.exe",
                @"C:\Program Files\RADGameTools\binkplay.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Plays the video using RAD Video Tools Bink Player (best option for .bik files).
        /// BinkPlay handles everything itself - we just start it and wait for completion.
        /// </summary>
        private async Task<bool> PlayWithBinkPlayerAsync(string videoPath, string binkPlayerPath, bool allowSkip, Control parentControl = null, VideoCompatibilityChecker.VideoInfo videoInfo = null)
        {
            try
            {
                Console.WriteLine("🎬 Playing BioWare intro with RAD Bink Player...");
                if (allowSkip)
                {
                    Console.WriteLine("BinkPlay handles ESC key internally - no custom overlay needed");
                }

                // BinkPlay.exe doesn't support proper embedding, so we skip custom video player entirely
                if (parentControl != null)
                {
                    Console.WriteLine("ℹ BinkPlay doesn't support true embedding - using windowed mode sized to parent control");
                }

                // Get the optimal BinkPlay executable (prefer binkplay.exe for compatibility)
                string optimalPlayerPath = GetOptimalBinkPlayPath(binkPlayerPath);

                // Build BinkPlay arguments with aspect-fit scaling
                string arguments = BuildBinkPlayArguments(videoPath, parentControl, optimalPlayerPath, videoInfo);

                // Start BinkPlay and let it handle everything - NO custom forms or overlays
                var startInfo = new ProcessStartInfo
                {
                    FileName = optimalPlayerPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                _videoProcess = Process.Start(startInfo);

                if (_videoProcess != null)
                {
                    Console.WriteLine("🎮 BinkPlay started - waiting for completion (no custom controls)");
                    
                    // Simply wait for BinkPlay to finish - it handles its own window, ESC key, everything
                    await Task.Run(() =>
                    {
                        _videoProcess.WaitForExit();
                    });

                    // BinkPlay has finished - no cleanup needed since we didn't create any custom forms
                    Console.WriteLine("✓ BinkPlay completed");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing video with Bink Player: {ex.Message}");
                Console.WriteLine("⚠ Could not play intro with Bink Player.");
                return false;
            }
        }

        /// <summary>
        /// Builds command-line arguments for BinkPlay using aspect-fit scaling to prevent stretching.
        /// Calculates proper dimensions based on video resolution and target display.
        /// </summary>
        /// <param name="videoPath">Path to the video file.</param>
        /// <param name="parentControl">Parent control (for determining if embedded mode was requested).</param>
        /// <param name="executablePath">Path to the executable being used.</param>
        /// <param name="videoInfo">Video information from compatibility check (optional).</param>
        /// <returns>Command-line arguments string for BinkPlay.</returns>
        private string BuildBinkPlayArguments(string videoPath, Control parentControl, string executablePath, VideoCompatibilityChecker.VideoInfo videoInfo = null)
        {
            var args = new List<string>();
            var executableName = Path.GetFileName(executablePath).ToLowerInvariant();

            // Add the video file path (quoted) - this is always first
            args.Add($"\"{videoPath}\"");

            // Different executables might have different syntax
            if (executableName.Contains("radvideo"))
            {
                // radvideo64.exe and radvideo32.exe might need different syntax
                Console.WriteLine($"🎮 Using minimal arguments for {executableName}");
                // For now, just pass the video file and see if it works
            }
            else
            {
                // binkplay.exe - use aspect-fit scaling to prevent stretching
                Size targetSize;
                
                if (parentControl == null)
                {
                    // Fullscreen mode - use screen dimensions
                    targetSize = VideoCompatibilityChecker.GetScreenDimensions();
                    Console.WriteLine($"🖥 Fullscreen target: {targetSize.Width}x{targetSize.Height}");
                }
                else
                {
                    // Windowed mode - use parent control dimensions
                    targetSize = new Size(parentControl.Width, parentControl.Height);
                    Console.WriteLine($"🖼 Windowed target: {targetSize.Width}x{targetSize.Height}");
                }

                // Calculate aspect-fit dimensions if we have video info
                if (videoInfo != null && videoInfo.IsDecodable && videoInfo.Width > 0 && videoInfo.Height > 0)
                {
                    var aspectFitSize = VideoCompatibilityChecker.CalculateAspectFitSize(
                        videoInfo.Width, videoInfo.Height, 
                        targetSize.Width, targetSize.Height);
                    
                    Console.WriteLine($"📐 Video: {videoInfo.Width}x{videoInfo.Height} → Aspect-fit: {aspectFitSize.Width}x{aspectFitSize.Height}");
                    
                    args.Add($"/W{aspectFitSize.Width}");
                    args.Add($"/H{aspectFitSize.Height}");
                    args.Add("/R"); // Fill with black background for letterboxing
                }
                else
                {
                    // Fallback: use target dimensions with letterboxing
                    Console.WriteLine("📐 No video info available - using target dimensions with letterboxing");
                    args.Add($"/W{targetSize.Width}");
                    args.Add($"/H{targetSize.Height}");
                    args.Add("/R"); // Fill with black background for letterboxing
                }
            }
            
            Console.WriteLine($"🎮 {executableName} arguments: {string.Join(" ", args)}");
            return string.Join(" ", args);
        }

        /// <summary>
        /// Gets the optimal BinkPlay executable path, preferring newer versions.
        /// </summary>
        /// <param name="basePath">Base RAD Video Tools installation path.</param>
        /// <returns>Path to the best BinkPlay executable.</returns>
        private string GetOptimalBinkPlayPath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                return null;

            var directory = Path.GetDirectoryName(basePath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return basePath;

            // For now, let's prefer the original binkplay.exe since it has more predictable command-line syntax
            // radvideo64.exe might have different syntax that's causing the "Unknown command" error
            
            var binkplay = Path.Combine(directory, "binkplay.exe");
            if (File.Exists(binkplay))
            {
                Console.WriteLine("🎯 Using binkplay.exe (most compatible command-line syntax)");
                return binkplay;
            }

            // Fallback to radvideo32.exe
            var radvideo32 = Path.Combine(directory, "radvideo32.exe");
            if (File.Exists(radvideo32))
            {
                Console.WriteLine("🎯 Using radvideo32.exe");
                return radvideo32;
            }

            // Last resort: radvideo64.exe (might have different syntax)
            var radvideo64 = Path.Combine(directory, "radvideo64.exe");
            if (File.Exists(radvideo64))
            {
                Console.WriteLine("🎯 Using radvideo64.exe (may have different command syntax)");
                return radvideo64;
            }

            // Use the original path as final fallback
            Console.WriteLine("🎯 Using original path");
            return basePath;
        }

        /// <summary>
        /// Plays the video using FFplay (fallback option).
        /// </summary>
        private async Task<bool> PlayWithFFplayAsync(string videoPath, string ffplayPath, bool allowSkip, Control parentControl = null)
        {
            try
            {
                Console.WriteLine("🎬 Playing BioWare intro with FFplay...");
                if (allowSkip)
                {
                    Console.WriteLine("Press ESC to skip intro");
                }

                if (parentControl != null)
                {
                    // Embedded mode - FFplay supports window embedding better
                    return await PlayEmbeddedFFplayAsync(videoPath, ffplayPath, allowSkip, parentControl);
                }
                else
                {
                    // Fullscreen mode
                    return await PlayFullscreenFFplayAsync(videoPath, ffplayPath, allowSkip);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing video with FFplay: {ex.Message}");
                Console.WriteLine("⚠ Could not play intro with FFplay.");
                await CleanupVideoPlayerAsync();
                return false;
            }
        }

        /// <summary>
        /// Plays video with FFplay in embedded mode.
        /// </summary>
        private async Task<bool> PlayEmbeddedFFplayAsync(string videoPath, string ffplayPath, bool allowSkip, Control parentControl)
        {
            try
            {
                // Create a panel to host the video player
                _videoPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };

                // Add panel to parent control
                if (parentControl.InvokeRequired)
                {
                    parentControl.Invoke(new Action(() => parentControl.Controls.Add(_videoPanel)));
                }
                else
                {
                    parentControl.Controls.Add(_videoPanel);
                }

                // Create a form to handle key events
                if (allowSkip)
                {
                    _videoForm = new Form
                    {
                        WindowState = FormWindowState.Minimized,
                        ShowInTaskbar = false,
                        KeyPreview = true,
                        Opacity = 0
                    };

                    _videoForm.KeyDown += (sender, e) =>
                    {
                        if (e.KeyCode == Keys.Escape)
                        {
                            _skipRequested = true;
                            StopIntro();
                        }
                    };

                    _videoForm.Show();
                    _videoForm.WindowState = FormWindowState.Minimized;
                }

                // Start FFplay with specific window positioning and no decorations
                // FFplay supports better window control than BinkPlay
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffplayPath,
                    Arguments = $"-autoexit -loglevel quiet -noborder -x {_videoPanel.Width} -y {_videoPanel.Height} \"{videoPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                _videoProcess = Process.Start(startInfo);

                if (_videoProcess != null)
                {
                    // Wait for the process to initialize and get its window handle
                    int attempts = 0;
                    while (attempts < 50 && (_videoProcess.MainWindowHandle == IntPtr.Zero || _videoProcess.HasExited))
                    {
                        await Task.Delay(100);
                        _videoProcess.Refresh();
                        attempts++;
                    }

                    // If we have a valid window handle, embed it
                    if (!_videoProcess.HasExited && _videoProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        // Set the video player as child of our panel
                        SetParent(_videoProcess.MainWindowHandle, _videoPanel.Handle);
                        
                        // Resize and position the video player to fill the panel
                        SetWindowPos(_videoProcess.MainWindowHandle, IntPtr.Zero, 0, 0, 
                                   _videoPanel.Width, _videoPanel.Height, SWP_NOZORDER | SWP_NOACTIVATE);
                    }
                    else
                    {
                        Console.WriteLine("⚠ Could not get FFplay window handle for embedding");
                    }

                    // Wait for the video to finish or be skipped
                    await Task.Run(() =>
                    {
                        while (!_videoProcess.HasExited && !_skipRequested)
                        {
                            Thread.Sleep(100);
                            Application.DoEvents();
                        }
                    });

                    // Clean up
                    await CleanupVideoPlayerAsync();

                    if (_skipRequested)
                    {
                        Console.WriteLine("✓ Intro skipped.");
                    }
                    else
                    {
                        Console.WriteLine("✓ Intro completed.");
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in embedded FFplay playback: {ex.Message}");
                await CleanupVideoPlayerAsync();
                return false;
            }
        }

        /// <summary>
        /// Plays video with FFplay in fullscreen mode.
        /// </summary>
        private async Task<bool> PlayFullscreenFFplayAsync(string videoPath, string ffplayPath, bool allowSkip)
        {
            try
            {
                // Create a transparent overlay form for ESC key handling
                if (allowSkip)
                {
                    _videoForm = new Form
                    {
                        WindowState = FormWindowState.Maximized,
                        FormBorderStyle = FormBorderStyle.None,
                        TopMost = true,
                        BackColor = Color.Black,
                        Opacity = 0.01, // Nearly transparent
                        ShowInTaskbar = false,
                        KeyPreview = true,
                        StartPosition = FormStartPosition.Manual,
                        Location = new Point(0, 0)
                    };

                    // Handle ESC key for skipping
                    _videoForm.KeyDown += (sender, e) =>
                    {
                        if (e.KeyCode == Keys.Escape)
                        {
                            _skipRequested = true;
                            StopIntro();
                        }
                    };

                    // Handle form closing
                    _videoForm.FormClosed += (sender, e) =>
                    {
                        _skipRequested = true;
                    };

                    _videoForm.Show();
                    _videoForm.Focus();
                }

                // Start FFplay with fullscreen and no window decorations
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffplayPath,
                    Arguments = $"-fs -autoexit -loglevel quiet \"{videoPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _videoProcess = Process.Start(startInfo);

                if (_videoProcess != null)
                {
                    // Wait for the video to finish or be skipped
                    await Task.Run(() =>
                    {
                        while (!_videoProcess.HasExited && !_skipRequested)
                        {
                            Thread.Sleep(100);
                            
                            // Process Windows messages to keep the form responsive
                            if (_videoForm != null && !_videoForm.IsDisposed)
                            {
                                if (_videoForm.InvokeRequired)
                                {
                                    _videoForm.Invoke(new Action(() => Application.DoEvents()));
                                }
                                else
                                {
                                    Application.DoEvents();
                                }
                            }
                        }
                    });

                    // Clean up
                    await CleanupVideoPlayerAsync();

                    if (_skipRequested)
                    {
                        Console.WriteLine("✓ Intro skipped.");
                    }
                    else
                    {
                        Console.WriteLine("✓ Intro completed.");
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in fullscreen FFplay playback: {ex.Message}");
                await CleanupVideoPlayerAsync();
                return false;
            }
        }

        /// <summary>
        /// Cleans up video player resources.
        /// </summary>
        private async Task CleanupVideoPlayerAsync()
        {
            try
            {
                // Ensure video player process is closed FIRST and COMPLETELY
                if (_videoProcess != null && !_videoProcess.HasExited)
                {
                    try
                    {
                        // For embedded mode, we need to be more aggressive about closing
                        // because the process might be stuck in embedded state
                        
                        // First try to restore the window to normal state before closing
                        if (_videoProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            // Remove from parent to restore normal window behavior
                            SetParent(_videoProcess.MainWindowHandle, IntPtr.Zero);
                        }
                        
                        // Try graceful close first
                        _videoProcess.CloseMainWindow();
                        
                        // Wait a shorter time for graceful close since we need to be quick
                        if (!_videoProcess.WaitForExit(500))
                        {
                            // Force kill if it doesn't close gracefully
                            _videoProcess.Kill();
                            
                            // Wait to ensure it's really dead
                            _videoProcess.WaitForExit(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error during process cleanup: {ex.Message}");
                        // Try one more time with just kill
                        try
                        {
                            if (!_videoProcess.HasExited)
                            {
                                _videoProcess.Kill();
                                _videoProcess.WaitForExit(1000);
                            }
                        }
                        catch
                        {
                            // Final attempt failed, but continue with other cleanup
                        }
                    }
                }

                // Clean up overlay form
                if (_videoForm != null && !_videoForm.IsDisposed)
                {
                    try
                    {
                        if (_videoForm.InvokeRequired)
                        {
                            _videoForm.Invoke(new Action(() => _videoForm.Close()));
                        }
                        else
                        {
                            _videoForm.Close();
                        }
                    }
                    catch
                    {
                        // Ignore errors when closing the form
                    }
                }

                // Clean up video panel
                if (_videoPanel != null && !_videoPanel.IsDisposed)
                {
                    try
                    {
                        if (_videoPanel.InvokeRequired)
                        {
                            _videoPanel.Invoke(new Action(() =>
                            {
                                _videoPanel.Parent?.Controls.Remove(_videoPanel);
                                _videoPanel.Dispose();
                            }));
                        }
                        else
                        {
                            _videoPanel.Parent?.Controls.Remove(_videoPanel);
                            _videoPanel.Dispose();
                        }
                    }
                    catch
                    {
                        // Ignore errors when disposing the panel
                    }
                }

                // Reset state
                _videoProcess = null;
                _videoForm = null;
                _videoPanel = null;

                // Give a moment for all cleanup to complete before continuing
                await Task.Delay(200);
                
                // Force garbage collection to clean up any remaining handles
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
                // Continue anyway - we don't want cleanup errors to prevent game launch
            }
        }

        /// <summary>
        /// Stops the currently playing intro video and ensures cleanup.
        /// </summary>
        public void StopIntro()
        {
            _skipRequested = true;
            
            // For BinkPlay, we just need to kill the process - no custom forms to clean up
            // For FFmpeg, we may need to clean up embedded windows and forms
            if (_videoProcess != null && !_videoProcess.HasExited)
            {
                try
                {
                    _videoProcess.Kill();
                    _videoProcess.WaitForExit(1000);
                }
                catch
                {
                    // Ignore errors when killing the process
                }
            }
            
            // Use async cleanup for FFmpeg components (forms, panels) but don't wait
            Task.Run(async () => await CleanupVideoPlayerAsync());
        }

        /// <summary>
        /// Ensures all video player processes are completely terminated.
        /// Call this before launching the game to prevent conflicts.
        /// </summary>
        public async Task EnsureVideoPlayersClosed()
        {
            try
            {
                // Stop our own video player first
                StopIntro();
                
                // Wait for our cleanup to complete
                await Task.Delay(500);
                
                // Kill any remaining binkplay.exe processes that might be running
                var binkProcesses = Process.GetProcessesByName("binkplay");
                foreach (var process in binkProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore errors when killing external processes
                    }
                }

                // Also kill any radvideo processes
                var radVideoProcesses = Process.GetProcessesByName("radvideo32")
                    .Concat(Process.GetProcessesByName("radvideo64"))
                    .ToArray();
                    
                foreach (var process in radVideoProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore errors when killing external processes
                    }
                }

                // Kill any remaining ffplay processes
                var ffplayProcesses = Process.GetProcessesByName("ffplay");
                foreach (var process in ffplayProcesses)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                        process.Dispose();
                    }
                    catch
                    {
                        // Ignore errors when killing external processes
                    }
                }

                // Note: Bink SDK integration doesn't create external processes,
                // so no additional cleanup needed for that

                Console.WriteLine("✓ All video player processes terminated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring video players closed: {ex.Message}");
                // Don't throw - we want game launch to continue even if cleanup has issues
            }
        }
    }
}