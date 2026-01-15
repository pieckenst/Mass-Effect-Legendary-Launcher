using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Native Bink SDK integration for proper embedded video playback.
    /// Based on the Bink SDK C++ example code.
    /// </summary>
    public class BinkSDKPlayer : IDisposable
    {
        #region Bink SDK P/Invoke Declarations
        
        // Bink SDK constants
        private const uint BINKSNDTRACK = 0x00004000;
        private const uint BINKNOFRAMEBUFFERS = 0x00000400;
        
        // Bink SDK structures
        [StructLayout(LayoutKind.Sequential)]
        private struct BINK
        {
            public uint Width;
            public uint Height;
            public uint Frames;
            public uint FrameNum;
            // ... other fields as needed
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BINKFRAMEBUFFERS
        {
            // Frame buffer information
            public IntPtr YABufferPtr;
            public IntPtr cRBufferPtr;
            public IntPtr cBBufferPtr;
            public IntPtr ABufferPtr;
            // ... other fields as needed
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BINKTEXTURESET
        {
            public BINKFRAMEBUFFERS bink_buffers;
            // ... other texture fields
        }

        // Bink SDK function imports
        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr BinkOpen([MarshalAs(UnmanagedType.LPStr)] string filename, uint flags);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkClose(IntPtr bink);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BinkWait(IntPtr bink);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkDoFrame(IntPtr bink);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkNextFrame(IntPtr bink);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BinkShouldSkip(IntPtr bink);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkPause(IntPtr bink, int pause);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkSoundUseDirectSound(IntPtr directSound);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkRegisterFrameBuffers(IntPtr bink, ref BINKFRAMEBUFFERS frameBuffers);

        [DllImport("binkw32.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BinkGetFrameBuffersInfo(IntPtr bink, ref BINKFRAMEBUFFERS frameBuffers);

        #endregion

        private IntPtr _binkHandle = IntPtr.Zero;
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private bool _disposed = false;
        private Control _parentControl;
        private BINKTEXTURESET _textureSet;

        /// <summary>
        /// Gets whether the Bink SDK is available (binkw32.dll exists).
        /// </summary>
        public static bool IsSDKAvailable
        {
            get
            {
                try
                {
                    // First check if we already have it locally
                    if (BinkDLLManager.IsBinkDLLAvailable())
                    {
                        return BinkDLLManager.ValidateBinkDLL();
                    }

                    // Try to find and copy from game installation
                    return BinkDLLManager.EnsureBinkDLL();
                }
                catch
                {
                    return false;
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Initializes a new instance of the BinkSDKPlayer.
        /// </summary>
        public BinkSDKPlayer()
        {
            if (!IsSDKAvailable)
            {
                throw new InvalidOperationException("Bink SDK (binkw32.dll) is not available. Please install RAD Video Tools or place binkw32.dll in the application directory.");
            }
        }

        /// <summary>
        /// Opens and prepares a Bink video file for playback.
        /// </summary>
        /// <param name="videoPath">Path to the .bik video file.</param>
        /// <param name="parentControl">Optional parent control for embedded playback.</param>
        /// <returns>True if the video was opened successfully.</returns>
        public bool OpenVideo(string videoPath, Control parentControl = null)
        {
            try
            {
                if (_binkHandle != IntPtr.Zero)
                {
                    CloseVideo();
                }

                _parentControl = parentControl;

                // Initialize DirectSound (pass null for default)
                BinkSoundUseDirectSound(IntPtr.Zero);

                // Open the Bink file
                _binkHandle = BinkOpen(videoPath, BINKSNDTRACK | BINKNOFRAMEBUFFERS);
                
                if (_binkHandle == IntPtr.Zero)
                {
                    return false;
                }

                // Get frame buffer information
                BinkGetFrameBuffersInfo(_binkHandle, ref _textureSet.bink_buffers);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening Bink video: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Starts video playback.
        /// </summary>
        /// <returns>True if playback started successfully.</returns>
        public bool StartPlayback()
        {
            if (_binkHandle == IntPtr.Zero)
            {
                return false;
            }

            _isPlaying = true;
            _isPaused = false;

            return true;
        }

        /// <summary>
        /// Processes a single frame of video. Call this in your main loop.
        /// </summary>
        /// <returns>True if there are more frames to process, false if video is complete.</returns>
        public bool ProcessFrame()
        {
            if (_binkHandle == IntPtr.Zero || !_isPlaying || _isPaused)
            {
                return _isPlaying;
            }

            try
            {
                // Check if we need to wait
                if (BinkWait(_binkHandle) != 0)
                {
                    return true; // Still playing, but waiting
                }

                // Register frame buffers and decompress frame
                BinkRegisterFrameBuffers(_binkHandle, ref _textureSet.bink_buffers);
                BinkDoFrame(_binkHandle);

                // Skip frames if necessary
                while (BinkShouldSkip(_binkHandle) != 0)
                {
                    BinkNextFrame(_binkHandle);
                    BinkDoFrame(_binkHandle);
                }

                // Move to next frame
                BinkNextFrame(_binkHandle);

                // Check if we've reached the end
                var bink = Marshal.PtrToStructure<BINK>(_binkHandle);
                if (bink.FrameNum >= bink.Frames)
                {
                    _isPlaying = false;
                    return false; // Video complete
                }

                return true; // Continue playing
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing Bink frame: {ex.Message}");
                _isPlaying = false;
                return false;
            }
        }

        /// <summary>
        /// Pauses or resumes video playback.
        /// </summary>
        /// <param name="pause">True to pause, false to resume.</param>
        public void Pause(bool pause)
        {
            if (_binkHandle != IntPtr.Zero)
            {
                BinkPause(_binkHandle, pause ? 1 : 0);
                _isPaused = pause;
            }
        }

        /// <summary>
        /// Stops video playback.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
        }

        /// <summary>
        /// Closes the current video and releases resources.
        /// </summary>
        public void CloseVideo()
        {
            if (_binkHandle != IntPtr.Zero)
            {
                BinkClose(_binkHandle);
                _binkHandle = IntPtr.Zero;
            }
            
            _isPlaying = false;
            _isPaused = false;
        }

        /// <summary>
        /// Gets whether a video is currently playing.
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Gets whether the video is currently paused.
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Gets the video dimensions if a video is loaded.
        /// </summary>
        public Size? VideoDimensions
        {
            get
            {
                if (_binkHandle != IntPtr.Zero)
                {
                    try
                    {
                        var bink = Marshal.PtrToStructure<BINK>(_binkHandle);
                        return new Size((int)bink.Width, (int)bink.Height);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Plays a Bink video file with simple playback loop.
        /// This is a high-level method that handles the entire playback process.
        /// </summary>
        /// <param name="videoPath">Path to the .bik video file.</param>
        /// <param name="parentControl">Optional parent control for embedded playback.</param>
        /// <param name="allowSkip">Whether to allow skipping with ESC key.</param>
        /// <returns>Task that completes when video finishes or is skipped.</returns>
        public async Task<bool> PlayVideoAsync(string videoPath, Control parentControl = null, bool allowSkip = true)
        {
            try
            {
                if (!OpenVideo(videoPath, parentControl))
                {
                    return false;
                }

                if (!StartPlayback())
                {
                    return false;
                }

                bool skipRequested = false;
                Form keyForm = null;

                // Create invisible form for key handling if skip is allowed
                if (allowSkip)
                {
                    keyForm = new Form
                    {
                        WindowState = FormWindowState.Minimized,
                        ShowInTaskbar = false,
                        KeyPreview = true,
                        Opacity = 0
                    };

                    keyForm.KeyDown += (sender, e) =>
                    {
                        if (e.KeyCode == Keys.Escape)
                        {
                            skipRequested = true;
                        }
                    };

                    keyForm.Show();
                }

                // Main playback loop
                await Task.Run(() =>
                {
                    while (IsPlaying && !skipRequested)
                    {
                        if (!ProcessFrame())
                        {
                            break; // Video completed
                        }

                        // Small delay to prevent excessive CPU usage
                        System.Threading.Thread.Sleep(1);
                        
                        // Process Windows messages
                        Application.DoEvents();
                    }
                });

                // Cleanup
                keyForm?.Close();
                CloseVideo();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Bink SDK playback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disposes of the BinkSDKPlayer and releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                CloseVideo();
                _disposed = true;
            }
        }
    }
}