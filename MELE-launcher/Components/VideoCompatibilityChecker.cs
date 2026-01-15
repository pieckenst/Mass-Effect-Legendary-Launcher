using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Checks video file compatibility and provides proper scaling calculations.
    /// </summary>
    public class VideoCompatibilityChecker
    {
        /// <summary>
        /// Video information extracted from ffprobe.
        /// </summary>
        public class VideoInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string CodecName { get; set; }
            public double Duration { get; set; }
            public bool IsDecodable { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Probes a video file using ffprobe to check if FFmpeg can decode it.
        /// </summary>
        /// <param name="videoPath">Path to the video file.</param>
        /// <param name="ffprobePath">Path to ffprobe executable.</param>
        /// <returns>VideoInfo with compatibility and metadata information.</returns>
        public static async Task<VideoInfo> ProbeVideoAsync(string videoPath, string ffprobePath)
        {
            var videoInfo = new VideoInfo();

            try
            {
                if (!File.Exists(ffprobePath))
                {
                    videoInfo.ErrorMessage = "ffprobe not found";
                    return videoInfo;
                }

                if (!File.Exists(videoPath))
                {
                    videoInfo.ErrorMessage = "Video file not found";
                    return videoInfo;
                }

                // Use ffprobe to get video information in JSON format
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{videoPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    videoInfo.ErrorMessage = "Failed to start ffprobe process";
                    return videoInfo;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    videoInfo.ErrorMessage = $"ffprobe failed: {error}";
                    return videoInfo;
                }

                // Parse JSON output
                var jsonDoc = JsonDocument.Parse(output);
                var root = jsonDoc.RootElement;

                // Find video stream
                if (root.TryGetProperty("streams", out var streams))
                {
                    foreach (var stream in streams.EnumerateArray())
                    {
                        if (stream.TryGetProperty("codec_type", out var codecType) && 
                            codecType.GetString() == "video")
                        {
                            // Extract video information
                            if (stream.TryGetProperty("width", out var width))
                                videoInfo.Width = width.GetInt32();

                            if (stream.TryGetProperty("height", out var height))
                                videoInfo.Height = height.GetInt32();

                            if (stream.TryGetProperty("codec_name", out var codecName))
                                videoInfo.CodecName = codecName.GetString();

                            if (stream.TryGetProperty("duration", out var duration))
                            {
                                if (double.TryParse(duration.GetString(), out var durationValue))
                                {
                                    videoInfo.Duration = durationValue;
                                }
                            }

                            videoInfo.IsDecodable = true;
                            break;
                        }
                    }
                }

                if (!videoInfo.IsDecodable)
                {
                    videoInfo.ErrorMessage = "No video stream found or not decodable";
                }

                return videoInfo;
            }
            catch (Exception ex)
            {
                videoInfo.ErrorMessage = $"Error probing video: {ex.Message}";
                return videoInfo;
            }
        }

        /// <summary>
        /// Calculates aspect-fit dimensions for a video to fit within target dimensions without stretching.
        /// </summary>
        /// <param name="videoWidth">Original video width.</param>
        /// <param name="videoHeight">Original video height.</param>
        /// <param name="targetWidth">Target container width.</param>
        /// <param name="targetHeight">Target container height.</param>
        /// <returns>Size with calculated width and height that maintains aspect ratio.</returns>
        public static Size CalculateAspectFitSize(int videoWidth, int videoHeight, int targetWidth, int targetHeight)
        {
            if (videoWidth <= 0 || videoHeight <= 0 || targetWidth <= 0 || targetHeight <= 0)
            {
                return new Size(targetWidth, targetHeight);
            }

            // Calculate aspect ratios
            double videoAspect = (double)videoWidth / videoHeight;
            double targetAspect = (double)targetWidth / targetHeight;

            int fitWidth, fitHeight;

            if (videoAspect > targetAspect)
            {
                // Video is wider than target - fit to width
                fitWidth = targetWidth;
                fitHeight = (int)(targetWidth / videoAspect);
            }
            else
            {
                // Video is taller than target - fit to height
                fitHeight = targetHeight;
                fitWidth = (int)(targetHeight * videoAspect);
            }

            return new Size(fitWidth, fitHeight);
        }

        /// <summary>
        /// Gets the screen dimensions for fullscreen aspect-fit calculations.
        /// </summary>
        /// <returns>Size representing the primary screen dimensions.</returns>
        public static Size GetScreenDimensions()
        {
            try
            {
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                return screen?.Bounds.Size ?? new Size(1920, 1080);
            }
            catch
            {
                // Fallback to common 1080p resolution
                return new Size(1920, 1080);
            }
        }

        /// <summary>
        /// Checks if FFmpeg can handle the video file and provides compatibility information.
        /// </summary>
        /// <param name="videoPath">Path to the video file.</param>
        /// <param name="ffmpegDownloader">FFmpeg downloader instance.</param>
        /// <returns>VideoInfo with compatibility and scaling information.</returns>
        public static async Task<VideoInfo> CheckFFmpegCompatibilityAsync(string videoPath, FFmpegDownloader ffmpegDownloader)
        {
            try
            {
                // Ensure FFmpeg is available (this will download ffmpeg, ffplay, and ffprobe)
                string ffmpegPath = await ffmpegDownloader.EnsureFFmpegAsync();
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    return new VideoInfo { ErrorMessage = "FFmpeg not available" };
                }

                // Get ffprobe path from the downloader
                string ffprobePath = ffmpegDownloader.GetFFprobePath();
                if (string.IsNullOrEmpty(ffprobePath))
                {
                    return new VideoInfo { ErrorMessage = "ffprobe not found after FFmpeg download" };
                }

                // Probe the video file
                var videoInfo = await ProbeVideoAsync(videoPath, ffprobePath);
                
                if (videoInfo.IsDecodable)
                {
                    Console.WriteLine($"✅ FFmpeg can decode video: {videoInfo.Width}x{videoInfo.Height} ({videoInfo.CodecName})");
                }
                else
                {
                    Console.WriteLine($"❌ FFmpeg cannot decode video: {videoInfo.ErrorMessage}");
                }

                return videoInfo;
            }
            catch (Exception ex)
            {
                return new VideoInfo { ErrorMessage = $"Compatibility check failed: {ex.Message}" };
            }
        }
    }
}