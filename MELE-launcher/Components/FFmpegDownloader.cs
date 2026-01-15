using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Handles downloading and setting up FFmpeg for video playback.
    /// </summary>
    public class FFmpegDownloader
    {
        private const string FFMPEG_VERSION = "6.1";
        private const string FFMPEG_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
        private static readonly string FFmpegDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
        private static readonly string FFmpegExecutable = Path.Combine(FFmpegDirectory, "bin", "ffmpeg.exe");

        /// <summary>
        /// Gets the path to the FFmpeg executable, downloading it if necessary.
        /// </summary>
        /// <returns>The path to ffmpeg.exe, or null if download failed.</returns>
        public async Task<string> EnsureFFmpegAsync()
        {
            try
            {
                // Check if FFmpeg already exists
                if (File.Exists(FFmpegExecutable))
                {
                    return FFmpegExecutable;
                }

                Console.WriteLine("📥 Downloading FFmpeg for video playback...");
                
                // Create ffmpeg directory
                Directory.CreateDirectory(FFmpegDirectory);

                // Download FFmpeg
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout

                var response = await httpClient.GetAsync(FFMPEG_URL);
                response.EnsureSuccessStatusCode();

                var zipPath = Path.Combine(FFmpegDirectory, "ffmpeg.zip");
                
                // Download to file
                using (var fileStream = File.Create(zipPath))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                Console.WriteLine("📦 Extracting FFmpeg...");

                // Extract the zip file
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // We need ffmpeg.exe, ffplay.exe, and ffprobe.exe
                        if (entry.FullName.EndsWith("ffmpeg.exe") || 
                            entry.FullName.EndsWith("ffplay.exe") || 
                            entry.FullName.EndsWith("ffprobe.exe"))
                        {
                            var destinationPath = Path.Combine(FFmpegDirectory, "bin", Path.GetFileName(entry.FullName));
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                            
                            entry.ExtractToFile(destinationPath, overwrite: true);
                            Console.WriteLine($"📦 Extracted: {Path.GetFileName(entry.FullName)}");
                        }
                    }
                }

                // Clean up zip file
                File.Delete(zipPath);

                if (File.Exists(FFmpegExecutable))
                {
                    // Check if all tools were extracted successfully
                    bool allToolsAvailable = AreAllToolsAvailable();
                    if (allToolsAvailable)
                    {
                        Console.WriteLine("✅ FFmpeg suite ready! (ffmpeg, ffplay, ffprobe)");
                    }
                    else
                    {
                        Console.WriteLine("⚠ FFmpeg partially ready (some tools missing)");
                    }
                    return FFmpegExecutable;
                }
                else
                {
                    Console.WriteLine("❌ FFmpeg extraction failed.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to download FFmpeg: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the path to ffplay.exe for video playback.
        /// </summary>
        /// <returns>The path to ffplay.exe, or null if not available.</returns>
        public string GetFFplayPath()
        {
            var ffplayPath = Path.Combine(FFmpegDirectory, "bin", "ffplay.exe");
            return File.Exists(ffplayPath) ? ffplayPath : null;
        }

        /// <summary>
        /// Gets the path to ffprobe.exe for video analysis.
        /// </summary>
        /// <returns>The path to ffprobe.exe, or null if not available.</returns>
        public string GetFFprobePath()
        {
            var ffprobePath = Path.Combine(FFmpegDirectory, "bin", "ffprobe.exe");
            return File.Exists(ffprobePath) ? ffprobePath : null;
        }

        /// <summary>
        /// Checks if all required FFmpeg tools are available.
        /// </summary>
        /// <returns>True if ffmpeg, ffplay, and ffprobe are all available.</returns>
        public bool AreAllToolsAvailable()
        {
            var ffmpegPath = Path.Combine(FFmpegDirectory, "bin", "ffmpeg.exe");
            var ffplayPath = Path.Combine(FFmpegDirectory, "bin", "ffplay.exe");
            var ffprobePath = Path.Combine(FFmpegDirectory, "bin", "ffprobe.exe");

            return File.Exists(ffmpegPath) && File.Exists(ffplayPath) && File.Exists(ffprobePath);
        }
    }
}