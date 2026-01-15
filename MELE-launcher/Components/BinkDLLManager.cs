using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Manages the Bink DLL by finding it in game installations and copying it to the launcher directory.
    /// This eliminates the need to install RAD Video Tools separately.
    /// </summary>
    public class BinkDLLManager
    {
        private static readonly string LauncherDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string LocalBinkDLL = Path.Combine(LauncherDirectory, "binkw32.dll");

        /// <summary>
        /// Ensures binkw32.dll is available in the launcher directory.
        /// Searches for it in game installations and copies it if found.
        /// </summary>
        /// <param name="gamePath">Path to the Mass Effect Legendary Edition installation.</param>
        /// <returns>True if binkw32.dll is available, false otherwise.</returns>
        public static bool EnsureBinkDLL(string gamePath = null)
        {
            try
            {
                // Check if we already have the DLL locally
                if (File.Exists(LocalBinkDLL))
                {
                    Console.WriteLine("✅ binkw32.dll already available in launcher directory");
                    return true;
                }

                // Try to find and copy from game installation
                string sourceDLL = FindBinkDLLInGame(gamePath);
                if (sourceDLL != null)
                {
                    Console.WriteLine($"📁 Found binkw32.dll in game: {sourceDLL}");
                    File.Copy(sourceDLL, LocalBinkDLL, overwrite: true);
                    Console.WriteLine("✅ binkw32.dll copied to launcher directory");
                    return true;
                }

                // Try to find in RAD Video Tools installation
                sourceDLL = FindBinkDLLInRADTools();
                if (sourceDLL != null)
                {
                    Console.WriteLine($"📁 Found binkw32.dll in RAD Tools: {sourceDLL}");
                    File.Copy(sourceDLL, LocalBinkDLL, overwrite: true);
                    Console.WriteLine("✅ binkw32.dll copied to launcher directory");
                    return true;
                }

                Console.WriteLine("⚠ binkw32.dll not found in any known locations");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ensuring binkw32.dll: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds binkw32.dll in Mass Effect Legendary Edition game installations.
        /// </summary>
        /// <param name="gamePath">Specific game path to check, or null to search common locations.</param>
        /// <returns>Path to binkw32.dll if found, null otherwise.</returns>
        private static string FindBinkDLLInGame(string gamePath = null)
        {
            var searchPaths = new List<string>();

            // If specific game path provided, check it first
            if (!string.IsNullOrEmpty(gamePath))
            {
                searchPaths.Add(gamePath);
            }

            // Common Mass Effect Legendary Edition installation paths
            searchPaths.AddRange(new[]
            {
                // Steam
                @"C:\Program Files (x86)\Steam\steamapps\common\Mass Effect Legendary Edition",
                @"D:\Steam\steamapps\common\Mass Effect Legendary Edition",
                @"E:\Steam\steamapps\common\Mass Effect Legendary Edition",
                
                // Origin/EA App
                @"C:\Program Files (x86)\Origin Games\Mass Effect Legendary Edition",
                @"C:\Program Files\Origin Games\Mass Effect Legendary Edition",
                @"D:\Origin Games\Mass Effect Legendary Edition",
                @"E:\Origin Games\Mass Effect Legendary Edition",
                
                // Epic Games Store
                @"C:\Program Files\Epic Games\MassEffectLegendaryEdition",
                @"D:\Epic Games\MassEffectLegendaryEdition",
                @"E:\Epic Games\MassEffectLegendaryEdition",
                
                // Xbox Game Pass
                @"C:\Program Files\WindowsApps\EAGames.MassEffectLegendaryEdition_1.0.0.0_x64__htphfmyq8952t",
                @"C:\XboxGames\Mass Effect Legendary Edition"
            });

            foreach (var basePath in searchPaths)
            {
                if (!Directory.Exists(basePath))
                    continue;

                // Common locations within the game directory where binkw32.dll might be found
                var dllPaths = new[]
                {
                    Path.Combine(basePath, "binkw32.dll"),
                    Path.Combine(basePath, "Game", "binkw32.dll"),
                    Path.Combine(basePath, "Game", "ME1", "Binaries", "Win64", "binkw32.dll"),
                    Path.Combine(basePath, "Game", "ME2", "Binaries", "Win64", "binkw32.dll"),
                    Path.Combine(basePath, "Game", "ME3", "Binaries", "Win64", "binkw32.dll"),
                    Path.Combine(basePath, "Game", "Launcher", "binkw32.dll"),
                    Path.Combine(basePath, "Binaries", "Win64", "binkw32.dll"),
                    Path.Combine(basePath, "Binaries", "binkw32.dll")
                };

                foreach (var dllPath in dllPaths)
                {
                    if (File.Exists(dllPath))
                    {
                        return dllPath;
                    }
                }

                // Search recursively in the game directory (but limit depth to avoid performance issues)
                try
                {
                    var foundDlls = Directory.GetFiles(basePath, "binkw32.dll", SearchOption.AllDirectories);
                    if (foundDlls.Length > 0)
                    {
                        return foundDlls[0]; // Return the first one found
                    }
                }
                catch
                {
                    // Ignore search errors (permissions, etc.)
                }
            }

            return null;
        }

        /// <summary>
        /// Finds binkw32.dll in RAD Video Tools installation.
        /// </summary>
        /// <returns>Path to binkw32.dll if found, null otherwise.</returns>
        private static string FindBinkDLLInRADTools()
        {
            var radToolsPaths = new[]
            {
                @"C:\Program Files (x86)\RADVideo\binkw32.dll",
                @"C:\Program Files\RADVideo\binkw32.dll",
                @"C:\Program Files (x86)\RAD Game Tools\binkw32.dll",
                @"C:\Program Files\RAD Game Tools\binkw32.dll",
                @"C:\Program Files (x86)\RADGameTools\binkw32.dll",
                @"C:\Program Files\RADGameTools\binkw32.dll"
            };

            return radToolsPaths.FirstOrDefault(File.Exists);
        }

        /// <summary>
        /// Gets the path to the local binkw32.dll in the launcher directory.
        /// </summary>
        /// <returns>Path to the local binkw32.dll.</returns>
        public static string GetLocalBinkDLLPath()
        {
            return LocalBinkDLL;
        }

        /// <summary>
        /// Checks if binkw32.dll is available locally.
        /// </summary>
        /// <returns>True if binkw32.dll exists in the launcher directory.</returns>
        public static bool IsBinkDLLAvailable()
        {
            return File.Exists(LocalBinkDLL);
        }

        /// <summary>
        /// Gets information about the available binkw32.dll.
        /// </summary>
        /// <returns>FileInfo for the DLL, or null if not available.</returns>
        public static FileInfo GetBinkDLLInfo()
        {
            if (File.Exists(LocalBinkDLL))
            {
                return new FileInfo(LocalBinkDLL);
            }
            return null;
        }

        /// <summary>
        /// Removes the local binkw32.dll if it exists.
        /// </summary>
        /// <returns>True if the file was removed or didn't exist, false if removal failed.</returns>
        public static bool RemoveBinkDLL()
        {
            try
            {
                if (File.Exists(LocalBinkDLL))
                {
                    File.Delete(LocalBinkDLL);
                    Console.WriteLine("🗑 Removed local binkw32.dll");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error removing binkw32.dll: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates that the binkw32.dll is a valid Bink library.
        /// </summary>
        /// <returns>True if the DLL appears to be valid.</returns>
        public static bool ValidateBinkDLL()
        {
            try
            {
                if (!File.Exists(LocalBinkDLL))
                    return false;

                var fileInfo = new FileInfo(LocalBinkDLL);
                
                // Basic validation - check file size (should be reasonable for a DLL)
                if (fileInfo.Length < 1024 || fileInfo.Length > 10 * 1024 * 1024) // 1KB to 10MB
                    return false;

                // Try to load the library to see if it's a valid DLL
                var handle = LoadLibrary(LocalBinkDLL);
                if (handle != IntPtr.Zero)
                {
                    FreeLibrary(handle);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}