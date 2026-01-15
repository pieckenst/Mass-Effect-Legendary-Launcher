using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using MELE_launcher.Models;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Detects Mass Effect game installations from various sources.
    /// </summary>
    public class GameDetector
    {
        private readonly AdminElevator _adminElevator;

        public GameDetector(AdminElevator adminElevator)
        {
            _adminElevator = adminElevator ?? throw new ArgumentNullException(nameof(adminElevator));
        }

        /// <summary>
        /// Scans all sources for Mass Effect game installations.
        /// </summary>
        /// <returns>List of detected games with validation status.</returns>
        public List<DetectedGame> ScanForGames()
        {
            var detectedGames = new List<DetectedGame>();
            var searchPaths = new HashSet<string>();

            // Aggregate all potential installation paths
            searchPaths.UnionWith(GetSteamLibraryPaths());
            searchPaths.UnionWith(GetEAAppPaths());
            searchPaths.UnionWith(GetRegistryPaths());
            searchPaths.UnionWith(GetCommonPaths());

            // Search each path for game installations
            foreach (var basePath in searchPaths)
            {
                if (!Directory.Exists(basePath))
                    continue;

                // Check for Legendary Edition games
                detectedGames.AddRange(DetectLegendaryGames(basePath));

                // Check for Original trilogy games
                detectedGames.AddRange(DetectOriginalGames(basePath));
            }

            // Remove duplicates based on executable path
            return detectedGames
                .GroupBy(g => g.ExecutablePath?.ToLowerInvariant())
                .Select(g => g.First())
                .Where(g => g.ExecutablePath != null)
                .ToList();
        }

        /// <summary>
        /// Validates a game installation by checking for the executable.
        /// </summary>
        /// <param name="path">Installation directory path.</param>
        /// <param name="gameType">Type of game to validate.</param>
        /// <param name="edition">Edition of the game.</param>
        /// <returns>True if the installation is valid.</returns>
        public bool ValidateInstallation(string path, GameType gameType, GameEdition edition)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            var relativePath = edition == GameEdition.Legendary
                ? GamePaths.LegendaryPaths[gameType]
                : GamePaths.OriginalPaths[gameType];

            var executablePath = Path.Combine(path, relativePath);
            return File.Exists(executablePath);
        }

        /// <summary>
        /// Gets Steam library paths by parsing libraryfolders.vdf.
        /// </summary>
        private List<string> GetSteamLibraryPaths()
        {
            var paths = new List<string>();

            try
            {
                // Common Steam installation locations
                var steamPaths = new[]
                {
                    @"C:\Program Files (x86)\Steam",
                    @"C:\Program Files\Steam",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
                };

                foreach (var steamPath in steamPaths.Distinct())
                {
                    var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                    if (!File.Exists(vdfPath))
                        continue;

                    var vdfContent = File.ReadAllText(vdfPath);
                    
                    // Parse VDF format to extract library paths
                    // Format: "path"		"D:\\SteamLibrary"
                    var pathMatches = Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""", RegexOptions.IgnoreCase);
                    
                    foreach (Match match in pathMatches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            var libraryPath = match.Groups[1].Value.Replace(@"\\", @"\");
                            var steamAppsPath = Path.Combine(libraryPath, "steamapps", "common");
                            if (Directory.Exists(steamAppsPath))
                            {
                                paths.Add(steamAppsPath);
                            }
                        }
                    }

                    // Also add the default steamapps/common from this Steam installation
                    var defaultCommon = Path.Combine(steamPath, "steamapps", "common");
                    if (Directory.Exists(defaultCommon))
                    {
                        paths.Add(defaultCommon);
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't access Steam paths
            }

            return paths;
        }

        /// <summary>
        /// Gets EA App and Origin installation directories.
        /// </summary>
        private List<string> GetEAAppPaths()
        {
            var paths = new List<string>();

            try
            {
                // Common EA/Origin paths
                var eaPaths = new[]
                {
                    @"C:\Program Files\EA Games",
                    @"C:\Program Files (x86)\EA Games",
                    @"C:\Program Files\Origin Games",
                    @"C:\Program Files (x86)\Origin Games",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "EA Games"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "EA Games"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Origin Games"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Origin Games")
                };

                paths.AddRange(eaPaths.Distinct().Where(Directory.Exists));
            }
            catch (Exception)
            {
                // Silently fail if we can't access EA paths
            }

            return paths;
        }

        /// <summary>
        /// Gets game paths from Windows Registry.
        /// </summary>
        private List<string> GetRegistryPaths()
        {
            var paths = new List<string>();

            try
            {
                // Registry keys where games might be registered
                var registryKeys = new[]
                {
                    @"SOFTWARE\BioWare\Mass Effect",
                    @"SOFTWARE\BioWare\Mass Effect 2",
                    @"SOFTWARE\BioWare\Mass Effect 3",
                    @"SOFTWARE\WOW6432Node\BioWare\Mass Effect",
                    @"SOFTWARE\WOW6432Node\BioWare\Mass Effect 2",
                    @"SOFTWARE\WOW6432Node\BioWare\Mass Effect 3"
                };

                foreach (var keyPath in registryKeys)
                {
                    try
                    {
                        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                        if (key != null)
                        {
                            var installPath = key.GetValue("Path") as string ?? key.GetValue("Install Dir") as string;
                            if (!string.IsNullOrEmpty(installPath) && Directory.Exists(installPath))
                            {
                                paths.Add(installPath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip inaccessible registry keys
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't access registry
            }

            return paths;
        }

        /// <summary>
        /// Gets common installation directories.
        /// </summary>
        private List<string> GetCommonPaths()
        {
            return GamePaths.CommonDirectories
                .Where(Directory.Exists)
                .ToList();
        }

        /// <summary>
        /// Detects Legendary Edition games in a directory.
        /// </summary>
        private List<DetectedGame> DetectLegendaryGames(string basePath)
        {
            var games = new List<DetectedGame>();

            // Check if basePath itself is the Legendary Edition root (has Game folder)
            var pathsToCheck = new List<string>();
            
            // First, check if basePath itself is the LE installation root
            if (Directory.Exists(Path.Combine(basePath, "Game")))
            {
                pathsToCheck.Add(basePath);
            }
            
            // Also check for LE as a subfolder
            var legendaryFolders = new[]
            {
                "Mass Effect Legendary Edition",
                "MassEffectLegendaryEdition"
            };

            foreach (var folder in legendaryFolders)
            {
                var legendaryPath = Path.Combine(basePath, folder);
                if (Directory.Exists(legendaryPath) && Directory.Exists(Path.Combine(legendaryPath, "Game")))
                {
                    pathsToCheck.Add(legendaryPath);
                }
            }

            // Check each potential LE installation for all three games
            foreach (var legendaryPath in pathsToCheck)
            {
                foreach (var gameType in new[] { GameType.ME1, GameType.ME2, GameType.ME3 })
                {
                    var relativePath = GamePaths.LegendaryPaths[gameType];
                    var executablePath = Path.Combine(legendaryPath, relativePath);

                    if (File.Exists(executablePath))
                    {
                        games.Add(new DetectedGame
                        {
                            Name = $"Mass Effect {(int)gameType + 1} Legendary Edition",
                            Path = legendaryPath,
                            ExecutablePath = executablePath,
                            Type = gameType,
                            Edition = GameEdition.Legendary,
                            IsValid = true,
                            RequiresAdmin = _adminElevator.RequiresElevation(executablePath)
                        });
                    }
                }
            }

            return games;
        }

        /// <summary>
        /// Detects Original trilogy games in a directory.
        /// </summary>
        private List<DetectedGame> DetectOriginalGames(string basePath)
        {
            var games = new List<DetectedGame>();

            // Original games are typically in separate folders
            var originalFolders = new Dictionary<GameType, string[]>
            {
                { GameType.ME1, new[] { "Mass Effect", "MassEffect" } },
                { GameType.ME2, new[] { "Mass Effect 2", "MassEffect2" } },
                { GameType.ME3, new[] { "Mass Effect 3", "MassEffect3" } }
            };

            foreach (var kvp in originalFolders)
            {
                var gameType = kvp.Key;
                var folders = kvp.Value;

                foreach (var folder in folders)
                {
                    var gamePath = Path.Combine(basePath, folder);
                    if (!Directory.Exists(gamePath))
                        continue;

                    var relativePath = GamePaths.OriginalPaths[gameType];
                    var executablePath = Path.Combine(gamePath, relativePath);

                    if (File.Exists(executablePath))
                    {
                        games.Add(new DetectedGame
                        {
                            Name = $"Mass Effect {(int)gameType + 1}",
                            Path = gamePath,
                            ExecutablePath = executablePath,
                            Type = gameType,
                            Edition = GameEdition.Original,
                            IsValid = true,
                            RequiresAdmin = _adminElevator.RequiresElevation(executablePath)
                        });
                    }
                }
            }

            return games;
        }
    }
}
