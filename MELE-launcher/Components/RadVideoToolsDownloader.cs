using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MELE_launcher.Utilities;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Handles downloading and setting up RAD Video Tools for Bink video playback.
    /// </summary>
    public class RadVideoToolsDownloader
    {
        private const string RAD_TOOLS_URL = "https://www.radgametools.com/down/Bink/RADTools.7z";
        // NOTE: This is the public archive password published by RAD Game Tools, not a secret.
        private const string RAD_TOOLS_PASSWORD = "RAD";
        private const string SEVENZA_URL = "https://www.7-zip.org/a/7za920.zip";
        // Pinned SHA-256 of 7za920.zip so a tampered/MITM'd download is never executed.
        private const string SEVENZA_SHA256 = "2a3afe19c180f8373fa02ff00254d5394fec0349f5804e0ad2f6067854ff28ac";
        private static readonly string RadToolsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "radtools");
        private static readonly string BinkPlayerExecutable = Path.Combine(RadToolsDirectory, "BinkPlay.exe");
        private static readonly string RadToolsInstaller = Path.Combine(RadToolsDirectory, "radtools.exe");

        /// <summary>
        /// Ensures a download URL uses HTTPS before it is requested, preventing accidental
        /// plaintext (downgradeable / MITM-able) retrieval of executable payloads.
        /// </summary>
        private static void EnsureHttps(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Refusing to download from non-HTTPS URL: {url}");
            }
        }

        /// <summary>
        /// Verifies that a file matches an expected SHA-256 hash.
        /// </summary>
        private static bool VerifyFileHash(string filePath, string expectedSha256)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var sha = SHA256.Create();
                var hash = Convert.ToHexString(sha.ComputeHash(stream));
                return string.Equals(hash, expectedSha256, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the Bink player executable, checking installed version first, then downloading if necessary.
        /// </summary>
        /// <returns>The path to BinkPlay.exe, or null if not found and download/install failed.</returns>
        public async Task<string> EnsureBinkPlayerAsync()
        {
            try
            {
                // First check if RAD Video Tools is already installed in the default location
                var installedPath = GetInstalledBinkPlayerPath();
                if (installedPath != null)
                {
                    Console.WriteLine("✅ Using installed RAD Video Tools Bink Player.");
                    return installedPath;
                }

                // Check if Bink player already exists in our local directory
                if (File.Exists(BinkPlayerExecutable))
                {
                    return BinkPlayerExecutable;
                }

                Console.WriteLine("📥 Downloading RAD Video Tools...");
                
                // Create rad tools directory
                Directory.CreateDirectory(RadToolsDirectory);

                // Download RAD Video Tools 7z file
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout

                // Add user agent to avoid blocking
                httpClient.DefaultRequestHeaders.Add("User-Agent", 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                EnsureHttps(RAD_TOOLS_URL);
                var response = await httpClient.GetAsync(RAD_TOOLS_URL);
                response.EnsureSuccessStatusCode();

                var sevenZipPath = Path.Combine(RadToolsDirectory, "RADTools.7z");
                await FileDownloader.DownloadToFileAsync(
                    RAD_TOOLS_URL,
                    sevenZipPath,
                    userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                Console.WriteLine("📦 Extracting RAD Tools (password protected)...");

                // Try to extract using 7-Zip if available, otherwise try built-in methods
                bool extracted = await TryExtractWith7ZipAsync(sevenZipPath, RadToolsDirectory, RAD_TOOLS_PASSWORD);
                
                if (!extracted)
                {
                    // Fallback: Try to use PowerShell with 7-Zip module
                    extracted = await TryExtractWithPowerShellAsync(sevenZipPath, RadToolsDirectory, RAD_TOOLS_PASSWORD);
                }

                if (!extracted)
                {
                    Console.WriteLine("❌ Could not extract RAD Tools - 7-Zip extraction failed.");
                    Console.WriteLine("💡 You may need to install 7-Zip or extract manually.");
                    return null;
                }

                // Clean up 7z file
                File.Delete(sevenZipPath);

                // Check if installer was extracted and run it
                var installerFiles = Directory.GetFiles(RadToolsDirectory, "*.exe", SearchOption.AllDirectories);
                string installerPath = null;
                
                // Look for the RAD Tools installer
                foreach (var file in installerFiles)
                {
                    var fileName = Path.GetFileName(file).ToLowerInvariant();
                    if (fileName.Contains("radtools") || fileName.Contains("setup") || fileName.Contains("install"))
                    {
                        installerPath = file;
                        break;
                    }
                }

                if (installerPath != null && File.Exists(installerPath))
                {
                    Console.WriteLine("🔧 Installing RAD Video Tools...");
                    
                    // Run the installer silently
                    var installResult = await RunRadToolsInstallerAsync(installerPath);
                    if (!installResult)
                    {
                        Console.WriteLine("⚠ RAD Tools installation may have failed, checking for BinkPlay.exe...");
                    }
                }
                else
                {
                    Console.WriteLine("⚠ No RAD Tools installer found, checking for BinkPlay.exe...");
                }

                // Look for BinkPlay.exe in common installation locations and extracted files
                var possiblePaths = new[]
                {
                    BinkPlayerExecutable,
                    // Standard installation paths
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RADGameTools", "BinkPlay.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "RADGameTools", "BinkPlay.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RAD Game Tools", "BinkPlay.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "RAD Game Tools", "BinkPlay.exe"),
                    // Local extracted paths
                    Path.Combine(RadToolsDirectory, "BinkPlay.exe"),
                    Path.Combine(RadToolsDirectory, "bin", "BinkPlay.exe"),
                    Path.Combine(RadToolsDirectory, "Tools", "BinkPlay.exe")
                };

                // Also search recursively in the extracted directory
                try
                {
                    var foundFiles = Directory.GetFiles(RadToolsDirectory, "BinkPlay.exe", SearchOption.AllDirectories);
                    var allPaths = new string[possiblePaths.Length + foundFiles.Length];
                    possiblePaths.CopyTo(allPaths, 0);
                    foundFiles.CopyTo(allPaths, possiblePaths.Length);
                    possiblePaths = allPaths;
                }
                catch
                {
                    // Ignore search errors
                }

                foreach (var path in possiblePaths)
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        // Copy to our standard location if needed
                        if (path != BinkPlayerExecutable)
                        {
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(BinkPlayerExecutable));
                                File.Copy(path, BinkPlayerExecutable, overwrite: true);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠ Could not copy BinkPlay.exe: {ex.Message}");
                                // Use the original path if copy fails
                                Console.WriteLine("✅ RAD Bink Player ready!");
                                return path;
                            }
                        }
                        
                        Console.WriteLine("✅ RAD Bink Player ready!");
                        return BinkPlayerExecutable;
                    }
                }

                Console.WriteLine("❌ Bink Player not found after installation.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to set up RAD Video Tools: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the path to the installed RAD Video Tools Bink Player.
        /// </summary>
        /// <returns>Path to binkplay.exe if found, null otherwise.</returns>
        private string GetInstalledBinkPlayerPath()
        {
            return RadToolsLocator.FindInstalledFile("binkplay.exe");
        }

        /// <summary>
        /// Attempts to extract the 7z file using system 7-Zip installation.
        /// </summary>
        private async Task<bool> TryExtractWith7ZipAsync(string sevenZipPath, string extractPath, string password)
        {
            try
            {
                // Common 7-Zip installation paths
                var sevenZipPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip", "7z.exe"),
                    "7z.exe" // In case it's in PATH
                };

                foreach (var sevenZipExe in sevenZipPaths)
                {
                    if (File.Exists(sevenZipExe) || sevenZipExe == "7z.exe")
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = sevenZipExe,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        // Pass each token separately so paths/password are never re-parsed by a shell.
                        startInfo.ArgumentList.Add("x");
                        startInfo.ArgumentList.Add(sevenZipPath);
                        startInfo.ArgumentList.Add($"-o{extractPath}");
                        startInfo.ArgumentList.Add($"-p{password}");
                        startInfo.ArgumentList.Add("-y");

                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            await process.WaitForExitAsync();
                            return process.ExitCode == 0;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to extract using PowerShell with 7-Zip4PowerShell module or built-in Expand-Archive.
        /// </summary>
        private async Task<bool> TryExtractWithPowerShellAsync(string sevenZipPath, string extractPath, string password)
        {
            try
            {
                // Escape single quotes so a path containing them cannot break out of the
                // PowerShell string literals and inject arbitrary commands.
                string EscapePs(string value) => value.Replace("'", "''");

                // First try with 7-Zip4PowerShell module if available
                var psScript7Zip = $@"
                    try {{
                        Import-Module 7Zip4PowerShell -ErrorAction Stop
                        Expand-7Zip -ArchiveFileName '{EscapePs(sevenZipPath)}' -TargetPath '{EscapePs(extractPath)}' -Password '{EscapePs(password)}'
                        Write-Host 'Extraction successful with 7Zip4PowerShell'
                        exit 0
                    }} catch {{
                        Write-Host 'Failed with 7Zip4PowerShell: ' + $_.Exception.Message
                        exit 1
                    }}
                ";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                // Use ArgumentList so the script is passed as a single, un-reparsed argument.
                startInfo.ArgumentList.Add("-NoProfile");
                startInfo.ArgumentList.Add("-ExecutionPolicy");
                startInfo.ArgumentList.Add("Bypass");
                startInfo.ArgumentList.Add("-Command");
                startInfo.ArgumentList.Add(psScript7Zip);

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode == 0)
                    {
                        return true;
                    }
                }

                // If 7Zip4PowerShell failed, try downloading and using 7za.exe (standalone 7-Zip)
                return await TryDownloadAndUse7zaAsync(sevenZipPath, extractPath, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads the standalone 7za.exe and uses it to extract the archive.
        /// </summary>
        private async Task<bool> TryDownloadAndUse7zaAsync(string sevenZipPath, string extractPath, string password)
        {
            try
            {
                Console.WriteLine("📦 Downloading 7-Zip standalone extractor...");
                
                var sevenZaPath = Path.Combine(RadToolsDirectory, "7za.exe");
                
                // Download 7za.exe from official 7-Zip site
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2);
                
                // 7za.exe is the standalone console version of 7-Zip
                EnsureHttps(SEVENZA_URL);
                var response = await httpClient.GetAsync(SEVENZA_URL);
                response.EnsureSuccessStatusCode();

                // 7za.exe is the standalone console version of 7-Zip
                var tempZipPath = Path.Combine(RadToolsDirectory, "7za.zip");
                await FileDownloader.DownloadToFileAsync(
                    "https://www.7-zip.org/a/7za920.zip",
                    tempZipPath,
                    timeout: TimeSpan.FromMinutes(2));

                // Reject a tampered/MITM'd archive before extracting and executing 7za.exe.
                if (!VerifyFileHash(tempZipPath, SEVENZA_SHA256))
                {
                    Console.WriteLine("❌ 7za download failed integrity check (unexpected SHA-256). Aborting.");
                    try { File.Delete(tempZipPath); } catch { /* ignore cleanup errors */ }
                    return false;
                }

                // Extract 7za.exe from the zip
                using (var archive = System.IO.Compression.ZipFile.OpenRead(tempZipPath))
                {
                    var entry = archive.GetEntry("7za.exe");
                    if (entry != null)
                    {
                        entry.ExtractToFile(sevenZaPath, overwrite: true);
                    }
                }

                // Clean up temp zip
                File.Delete(tempZipPath);

                if (!File.Exists(sevenZaPath))
                {
                    Console.WriteLine("❌ Failed to extract 7za.exe");
                    return false;
                }

                Console.WriteLine("🔧 Using 7za.exe to extract RAD Tools...");

                // Use 7za.exe to extract the password-protected archive
                var startInfo = new ProcessStartInfo
                {
                    FileName = sevenZaPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                startInfo.ArgumentList.Add("x");
                startInfo.ArgumentList.Add(sevenZipPath);
                startInfo.ArgumentList.Add($"-o{extractPath}");
                startInfo.ArgumentList.Add($"-p{password}");
                startInfo.ArgumentList.Add("-y");

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();

                    // Clean up 7za.exe after use
                    try
                    {
                        File.Delete(sevenZaPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }

                    return process.ExitCode == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to use 7za.exe: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Runs the RAD Tools installer.
        /// </summary>
        private async Task<bool> RunRadToolsInstallerAsync(string installerPath)
        {
            try
            {
                if (!File.Exists(installerPath))
                {
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/S", // Silent installation
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    // Give installer more time to complete
                    await process.WaitForExitAsync();
                    
                    // Some installers return non-zero even on success, so we'll check for BinkPlay.exe later
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Installer execution failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if RAD Video Tools is available (either installed or in local directory).
        /// </summary>
        /// <returns>True if BinkPlay.exe exists and is ready to use.</returns>
        public bool IsBinkPlayerAvailable()
        {
            // Check installed version first
            var installedPath = GetInstalledBinkPlayerPath();
            if (installedPath != null)
            {
                return true;
            }

            // Check local version
            return File.Exists(BinkPlayerExecutable);
        }

        /// <summary>
        /// Gets the best available Bink Player path without downloading.
        /// </summary>
        /// <returns>Path to BinkPlay.exe if available, null otherwise.</returns>
        public string GetAvailableBinkPlayerPath()
        {
            // Check installed version first
            var installedPath = GetInstalledBinkPlayerPath();
            if (installedPath != null)
            {
                return installedPath;
            }

            // Check local version
            if (File.Exists(BinkPlayerExecutable))
            {
                return BinkPlayerExecutable;
            }

            return null;
        }
    }
}