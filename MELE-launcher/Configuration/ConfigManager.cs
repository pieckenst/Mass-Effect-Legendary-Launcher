using System;
using System.IO;
using System.Text.Json;
using MELE_launcher.Models;
using MELE_launcher.Utilities;

namespace MELE_launcher.Configuration
{
    /// <summary>
    /// Manages persistent configuration using JSON serialization.
    /// </summary>
    public class ConfigManager
    {
        private const string ConfigFileName = "launcher-config.json";
        private const string AppDataFolderName = "MELE-Launcher";

        private readonly string _configDirectory;

        /// <summary>
        /// Initializes a new instance that stores configuration in the current user's
        /// application-data directory. This keeps the file writable even when the
        /// launcher itself is installed in a read-only location such as Program Files.
        /// </summary>
        public ConfigManager() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance, optionally overriding the directory the
        /// configuration file is stored in (primarily for testing).
        /// </summary>
        /// <param name="configDirectoryOverride">
        /// Directory to store the configuration in, or <c>null</c> to use the
        /// per-user application-data directory.
        /// </param>
        public ConfigManager(string configDirectoryOverride)
        {
            _configDirectory = string.IsNullOrWhiteSpace(configDirectoryOverride)
                ? ResolveDefaultConfigDirectory()
                : configDirectoryOverride;
        }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        /// <returns>The absolute path to the configuration file.</returns>
        public string GetConfigPath()
        {
            return Path.Combine(_configDirectory, ConfigFileName);
        }

        /// <summary>
        /// Resolves the per-user application-data directory for the launcher,
        /// creating it if necessary. Falls back to the current directory only if
        /// the application-data location cannot be resolved or created.
        /// </summary>
        private static string ResolveDefaultConfigDirectory()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                if (string.IsNullOrEmpty(appData))
                {
                    return Directory.GetCurrentDirectory();
                }

                string directory = Path.Combine(appData, AppDataFolderName);
                Directory.CreateDirectory(directory);
                return directory;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // If we cannot use the application-data directory, fall back to the
                // working directory rather than failing to construct the manager.
                LauncherLog.Warning(
                    nameof(ConfigManager),
                    $"\u26a0 Could not use the application-data directory for configuration: {ex.Message}",
                    ex);
                return Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// Migrates a configuration file from the legacy location (next to the
        /// executable / current working directory) into the application-data
        /// directory the first time the launcher runs after the move. Best-effort:
        /// any failure leaves the legacy file untouched and falls through to normal
        /// first-run behaviour.
        /// </summary>
        private void MigrateLegacyConfigIfNeeded(string configPath)
        {
            try
            {
                string legacyPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);

                // Nothing to migrate if the legacy file is absent, the new file
                // already exists, or the legacy file *is* the new file.
                if (File.Exists(configPath) ||
                    !File.Exists(legacyPath) ||
                    string.Equals(Path.GetFullPath(legacyPath), Path.GetFullPath(configPath), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                File.Copy(legacyPath, configPath, overwrite: false);
                LauncherLog.Diagnostic(
                    nameof(ConfigManager),
                    $"Migrated configuration from legacy location '{legacyPath}' to '{configPath}'.");
            }
            catch (Exception ex)
            {
                LauncherLog.Diagnostic(
                    nameof(ConfigManager),
                    $"Could not migrate legacy configuration file: {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Loads the launcher configuration from the JSON file.
        /// If the file doesn't exist or is corrupted, creates a default configuration.
        /// </summary>
        /// <returns>The loaded or default launcher configuration.</returns>
        public LauncherConfig Load()
        {
            string configPath = GetConfigPath();

            MigrateLegacyConfigIfNeeded(configPath);

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
                LogConfigError($"⚠ Could not read configuration file '{configPath}': {ex.Message}", ex);
                return CreateDefaultConfig();
            }
            catch (UnauthorizedAccessException ex)
            {
                LogConfigError($"⚠ Access denied reading configuration file '{configPath}': {ex.Message}", ex);
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Reports a configuration error to the user (stderr) and records full
        /// exception detail for diagnostics (debug output).
        /// </summary>
        private static void LogConfigError(string message, Exception ex = null)
        {
            LauncherLog.Error(nameof(ConfigManager), message, ex);
        }

        /// <summary>
        /// Backs up a corrupt configuration file so the user's data is not lost when
        /// the launcher falls back to a default configuration.
        /// </summary>
        /// <param name="configPath">Path to the corrupt configuration file.</param>
        /// <param name="reason">Human-readable reason the file was rejected.</param>
        private void BackupCorruptConfig(string configPath, string reason)
        {
            LogConfigError($"⚠ Configuration file '{configPath}' is corrupt ({reason}). Using defaults.");

            try
            {
                string backupPath = $"{configPath}.corrupt-{DateTime.Now:yyyyMMddHHmmss}.bak";
                File.Copy(configPath, backupPath, overwrite: true);
                LauncherLog.Warning(nameof(ConfigManager), $"  A backup was saved to '{backupPath}'.");
            }
            catch (Exception backupEx)
            {
                // Backing up is best-effort; report but do not fail loading over it.
                LogConfigError($"  Could not back up the corrupt configuration: {backupEx.Message}", backupEx);
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
                LauncherLog.Diagnostic(nameof(ConfigManager), $"Could not delete temp file '{path}': {ex.Message}", ex);
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
