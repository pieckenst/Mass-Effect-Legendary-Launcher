using System;
using System.IO;
using MELE_launcher.Configuration;
using MELE_launcher.Models;
using Xunit;

namespace MELE_launcher.Tests
{
    /// <summary>
    /// ConfigManager stores launcher-config.json in a configurable directory (the
    /// per-user application-data directory by default). Each test points a fresh
    /// ConfigManager at an isolated temp directory via the constructor override so it
    /// never clobbers the real config file or other tests.
    /// </summary>
    public class ConfigManagerTests : IDisposable
    {
        private readonly string _tempDirectory;

        public ConfigManagerTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "mele-config-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        private ConfigManager CreateManager() => new ConfigManager(_tempDirectory);

        public void Dispose()
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }

        [Fact]
        public void GetConfigPath_UsesConfiguredDirectory()
        {
            var manager = CreateManager();
            var expected = Path.Combine(_tempDirectory, "launcher-config.json");
            Assert.Equal(expected, manager.GetConfigPath());
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsDefaultConfig()
        {
            var manager = CreateManager();

            var config = manager.Load();

            Assert.NotNull(config);
            Assert.Empty(config.Games);
            Assert.Equal("INT", config.DefaultLocale);
            Assert.Equal("INT", config.DefaultVoiceLanguage);
            Assert.False(config.DefaultForceFeedback);
            Assert.True(config.DefaultSkipIntro);
            Assert.Equal(DateTime.MinValue, config.LastScanDate);
        }

        [Fact]
        public void Save_ThenLoad_RoundTripsValues()
        {
            var manager = CreateManager();
            var scanDate = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var config = new LauncherConfig
            {
                DefaultLocale = "FR",
                DefaultVoiceLanguage = "DE",
                DefaultForceFeedback = true,
                DefaultSkipIntro = false,
                LastScanDate = scanDate,
                Games =
                {
                    new GameConfig
                    {
                        Type = GameType.ME2,
                        Edition = GameEdition.Legendary,
                        Path = @"C:\Games\ME2",
                        Locale = "RU",
                        VoiceLanguage = "RU",
                        ForceFeedback = true
                    }
                }
            };

            manager.Save(config);
            var loaded = manager.Load();

            Assert.Equal("FR", loaded.DefaultLocale);
            Assert.Equal("DE", loaded.DefaultVoiceLanguage);
            Assert.True(loaded.DefaultForceFeedback);
            Assert.False(loaded.DefaultSkipIntro);
            Assert.Equal(scanDate, loaded.LastScanDate);
            var game = Assert.Single(loaded.Games);
            Assert.Equal(GameType.ME2, game.Type);
            Assert.Equal(GameEdition.Legendary, game.Edition);
            Assert.Equal(@"C:\Games\ME2", game.Path);
            Assert.Equal("RU", game.Locale);
            Assert.True(game.ForceFeedback);
        }

        [Fact]
        public void Save_WritesIndentedJson()
        {
            var manager = CreateManager();

            manager.Save(new LauncherConfig());

            var json = File.ReadAllText(manager.GetConfigPath());
            Assert.Contains(Environment.NewLine, json);
            Assert.Contains("\"DefaultLocale\"", json);
        }

        [Fact]
        public void Save_NullConfig_Throws()
        {
            var manager = CreateManager();
            Assert.Throws<ArgumentNullException>(() => manager.Save(null));
        }

        [Fact]
        public void Load_CorruptedFile_ReturnsDefaultConfig()
        {
            var manager = CreateManager();
            File.WriteAllText(manager.GetConfigPath(), "{ this is not valid json ]");

            var config = manager.Load();

            Assert.NotNull(config);
            Assert.Equal("INT", config.DefaultLocale);
            Assert.Empty(config.Games);
        }

        [Fact]
        public void Load_JsonNullLiteral_ReturnsDefaultConfig()
        {
            var manager = CreateManager();
            File.WriteAllText(manager.GetConfigPath(), "null");

            var config = manager.Load();

            Assert.NotNull(config);
            Assert.Equal("INT", config.DefaultLocale);
        }

        [Fact]
        public void Load_MigratesLegacyConfigFromWorkingDirectory()
        {
            // Legacy config lives next to the executable (the working directory).
            string originalDirectory = Directory.GetCurrentDirectory();
            string legacyDirectory = Path.Combine(_tempDirectory, "legacy");
            string configDirectory = Path.Combine(_tempDirectory, "appdata");
            Directory.CreateDirectory(legacyDirectory);
            Directory.CreateDirectory(configDirectory);

            try
            {
                Directory.SetCurrentDirectory(legacyDirectory);

                var legacyManager = new ConfigManager(legacyDirectory);
                legacyManager.Save(new LauncherConfig { DefaultLocale = "PL" });

                // A new manager pointed at a different (empty) directory should pick up
                // the legacy file on first load rather than starting fresh.
                var manager = new ConfigManager(configDirectory);
                var config = manager.Load();

                Assert.Equal("PL", config.DefaultLocale);
                Assert.True(File.Exists(Path.Combine(configDirectory, "launcher-config.json")));
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

        [Fact]
        public void Load_DoesNotMigrateWhenTargetConfigAlreadyExists()
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            string legacyDirectory = Path.Combine(_tempDirectory, "legacy");
            string configDirectory = Path.Combine(_tempDirectory, "appdata");
            Directory.CreateDirectory(legacyDirectory);
            Directory.CreateDirectory(configDirectory);

            try
            {
                Directory.SetCurrentDirectory(legacyDirectory);

                new ConfigManager(legacyDirectory).Save(new LauncherConfig { DefaultLocale = "PL" });

                var manager = new ConfigManager(configDirectory);
                manager.Save(new LauncherConfig { DefaultLocale = "FR" });

                var config = manager.Load();

                // The existing app-data config must win; the legacy file is ignored.
                Assert.Equal("FR", config.DefaultLocale);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
    }
}
