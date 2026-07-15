using System;
using System.IO;
using MELE_launcher.Configuration;
using MELE_launcher.Models;
using Xunit;

namespace MELE_launcher.Tests
{
    /// <summary>
    /// ConfigManager reads/writes launcher-config.json relative to the current working
    /// directory. These tests run serially (single class, no parallel collection) and each
    /// switches the process working directory to an isolated temp folder so they never
    /// clobber the real config file or each other.
    /// </summary>
    public class ConfigManagerTests : IDisposable
    {
        private readonly string _originalDirectory;
        private readonly string _tempDirectory;

        public ConfigManagerTests()
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "mele-config-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
            Directory.SetCurrentDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
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
        public void GetConfigPath_IsInCurrentDirectory()
        {
            var manager = new ConfigManager();
            var expected = Path.Combine(_tempDirectory, "launcher-config.json");
            Assert.Equal(expected, manager.GetConfigPath());
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsDefaultConfig()
        {
            var manager = new ConfigManager();

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
            var manager = new ConfigManager();
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
            var manager = new ConfigManager();

            manager.Save(new LauncherConfig());

            var json = File.ReadAllText(manager.GetConfigPath());
            Assert.Contains(Environment.NewLine, json);
            Assert.Contains("\"DefaultLocale\"", json);
        }

        [Fact]
        public void Save_NullConfig_Throws()
        {
            var manager = new ConfigManager();
            Assert.Throws<ArgumentNullException>(() => manager.Save(null));
        }

        [Fact]
        public void Load_CorruptedFile_ReturnsDefaultConfig()
        {
            var manager = new ConfigManager();
            File.WriteAllText(manager.GetConfigPath(), "{ this is not valid json ]");

            var config = manager.Load();

            Assert.NotNull(config);
            Assert.Equal("INT", config.DefaultLocale);
            Assert.Empty(config.Games);
        }

        [Fact]
        public void Load_JsonNullLiteral_ReturnsDefaultConfig()
        {
            var manager = new ConfigManager();
            File.WriteAllText(manager.GetConfigPath(), "null");

            var config = manager.Load();

            Assert.NotNull(config);
            Assert.Equal("INT", config.DefaultLocale);
        }
    }
}
