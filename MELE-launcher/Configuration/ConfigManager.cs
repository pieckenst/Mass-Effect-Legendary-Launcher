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

            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<LauncherConfig>(json);
                    
                    // If deserialization returns null, create default
                    if (config == null)
                    {
                        return CreateDefaultConfig();
                    }

                    return config;
                }
            }
            catch (Exception)
            {
                // If file is corrupted or any error occurs, create default config
                // Silently handle the error and return default
            }

            // File doesn't exist or error occurred, create default
            return CreateDefaultConfig();
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
            File.WriteAllText(configPath, json);
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
                LastScanDate = DateTime.MinValue
            };
        }
    }
}
