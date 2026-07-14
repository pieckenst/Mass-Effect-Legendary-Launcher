using System;
using System.IO;
using System.Text.Json;
using MELE_launcher.Models;

namespace MELE_launcher.Configuration
{
    /// <summary>
    /// Manages persistent configuration using JSON serialization.
    /// </summary>
    public class ConfigManager
    {
        private const string ConfigFileName = "launcher-config.json";

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        /// <returns>The absolute path to the configuration file.</returns>
        public string GetConfigPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        }

        /// <summary>
        /// Loads the launcher configuration from the JSON file.
        /// If the file doesn't exist or is corrupted, creates a default configuration.
        /// </summary>
        /// <returns>The loaded or default launcher configuration.</returns>
        public LauncherConfig Load()
        {
            string configPath = GetConfigPath();

            // A missing file is an expected first-run condition, not an error.
            if (!File.Exists(configPath))
            {
                return CreateDefaultConfig();
            }

            try
            {
                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<LauncherConfig>(json);

                // If deserialization returns null, the file is present but empty/invalid.
                if (config == null)
                {
                    BackupCorruptConfig(configPath, "configuration file was empty or contained only null");
                    return CreateDefaultConfig();
                }

                return config;
            }
            catch (JsonException ex)
            {
                // The file exists but is not valid JSON. Preserve it so the user's
                // data is not silently discarded, and surface the problem.
                BackupCorruptConfig(configPath, ex.Message);
                return CreateDefaultConfig();
            }
            catch (IOException ex)
            {
                // The file could not be read (locked, permissions, etc.). Do not
                // overwrite it with defaults; report and fall back for this session.
                Console.Error.WriteLine($"⚠ Could not read configuration file '{configPath}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ConfigManager.Load IO error: {ex}");
                return CreateDefaultConfig();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"⚠ Access denied reading configuration file '{configPath}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ConfigManager.Load access error: {ex}");
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Backs up a corrupt configuration file so the user's data is not lost when
        /// the launcher falls back to a default configuration.
        /// </summary>
        /// <param name="configPath">Path to the corrupt configuration file.</param>
        /// <param name="reason">Human-readable reason the file was rejected.</param>
        private void BackupCorruptConfig(string configPath, string reason)
        {
            Console.Error.WriteLine($"⚠ Configuration file '{configPath}' is corrupt ({reason}). Using defaults.");
            System.Diagnostics.Debug.WriteLine($"ConfigManager.Load corrupt config: {reason}");

            try
            {
                string backupPath = $"{configPath}.corrupt-{DateTime.Now:yyyyMMddHHmmss}.bak";
                File.Copy(configPath, backupPath, overwrite: true);
                Console.Error.WriteLine($"  A backup was saved to '{backupPath}'.");
            }
            catch (Exception backupEx)
            {
                // Backing up is best-effort; report but do not fail loading over it.
                Console.Error.WriteLine($"  Could not back up the corrupt configuration: {backupEx.Message}");
                System.Diagnostics.Debug.WriteLine($"ConfigManager.Load backup failed: {backupEx}");
            }
        }

        /// <summary>
        /// Saves the launcher configuration to the JSON file.
        /// </summary>
        /// <param name="config">The configuration to save.</param>
        public void Save(LauncherConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string configPath = GetConfigPath();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(config, options);

            // Write to a temporary file first, then replace the target atomically.
            // This prevents a partially-written file from corrupting existing
            // configuration if the process is interrupted mid-write.
            string tempPath = configPath + ".tmp";
            try
            {
                File.WriteAllText(tempPath, json);

                if (File.Exists(configPath))
                {
                    File.Replace(tempPath, configPath, destinationBackupFileName: null);
                }
                else
                {
                    File.Move(tempPath, configPath);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // Clean up the temp file so we do not leave stray artifacts behind,
                // then propagate a clear, actionable error to the caller.
                TryDeleteFile(tempPath);
                throw new IOException(
                    $"Failed to save configuration to '{configPath}': {ex.Message}", ex);
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                // Best-effort cleanup; the original failure is more important.
                System.Diagnostics.Debug.WriteLine($"ConfigManager could not delete temp file '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a default launcher configuration.
        /// </summary>
        /// <returns>A new default configuration.</returns>
        private LauncherConfig CreateDefaultConfig()
        {
            return new LauncherConfig
            {
                Games = new System.Collections.Generic.List<GameConfig>(),
                DefaultLocale = "INT",
                DefaultVoiceLanguage = "INT",
                DefaultForceFeedback = false,
                DefaultSkipIntro = true,
                LastScanDate = DateTime.MinValue
            };
        }
    }
}
