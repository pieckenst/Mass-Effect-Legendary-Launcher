using System;
using System.Collections.Generic;
using System.Text.Json;
using MELE_launcher.Models;
using Xunit;

namespace MELE_launcher.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void LauncherConfig_HasSensibleDefaults()
        {
            var config = new LauncherConfig();

            Assert.NotNull(config.Games);
            Assert.Empty(config.Games);
            Assert.Equal("INT", config.DefaultLocale);
            Assert.Equal("INT", config.DefaultVoiceLanguage);
            Assert.False(config.DefaultForceFeedback);
            Assert.True(config.DefaultSkipIntro);
            Assert.Equal(DateTime.MinValue, config.LastScanDate);
        }

        [Fact]
        public void GameConfig_HasSensibleDefaults()
        {
            var config = new GameConfig();

            Assert.Equal(string.Empty, config.Path);
            Assert.Equal(string.Empty, config.Locale);
            Assert.Equal("INT", config.VoiceLanguage);
            Assert.False(config.ForceFeedback);
        }

        [Fact]
        public void LaunchOptions_PlayIntroDefaultsToTrue()
        {
            var options = new LaunchOptions();

            Assert.True(options.PlayIntro);
            Assert.False(options.ForceFeedback);
            Assert.False(options.Silent);
        }

        [Fact]
        public void LaunchResult_Defaults()
        {
            var result = new LaunchResult();

            Assert.False(result.Success);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public void CommandDefinition_InitializesCollections()
        {
            var command = new CommandDefinition();

            Assert.NotNull(command.Aliases);
            Assert.Empty(command.Aliases);
            Assert.NotNull(command.Parameters);
            Assert.Empty(command.Parameters);
            Assert.True(command.Enabled);
        }

        [Fact]
        public void CommandRegistry_HasDefaults()
        {
            var registry = new CommandRegistry();

            Assert.NotNull(registry.Commands);
            Assert.Empty(registry.Commands);
            Assert.Equal("1.0", registry.Version);
        }

        [Fact]
        public void MenuItem_IsEnabledByDefault()
        {
            var item = new MassEffectLauncher.Models.MenuItem();
            Assert.True(item.IsEnabled);
        }

        [Fact]
        public void GamePaths_ContainsAllGameTypes()
        {
            Assert.Equal(3, GamePaths.LegendaryPaths.Count);
            Assert.Equal(3, GamePaths.OriginalPaths.Count);
            foreach (GameType type in Enum.GetValues(typeof(GameType)))
            {
                Assert.True(GamePaths.LegendaryPaths.ContainsKey(type));
                Assert.True(GamePaths.OriginalPaths.ContainsKey(type));
            }
            Assert.NotEmpty(GamePaths.CommonDirectories);
        }

        [Fact]
        public void GameConfig_SerializesAndDeserializes()
        {
            var config = new GameConfig
            {
                Type = GameType.ME3,
                Edition = GameEdition.Original,
                Path = @"D:\Games\ME3",
                Locale = "DE",
                VoiceLanguage = "DE",
                ForceFeedback = true
            };

            var json = JsonSerializer.Serialize(config);
            var restored = JsonSerializer.Deserialize<GameConfig>(json);

            Assert.Equal(config.Type, restored.Type);
            Assert.Equal(config.Edition, restored.Edition);
            Assert.Equal(config.Path, restored.Path);
            Assert.Equal(config.Locale, restored.Locale);
            Assert.Equal(config.VoiceLanguage, restored.VoiceLanguage);
            Assert.Equal(config.ForceFeedback, restored.ForceFeedback);
        }

        [Fact]
        public void DetectedGame_StoresAssignedValues()
        {
            var game = new DetectedGame
            {
                Name = "Mass Effect",
                Path = @"C:\ME",
                ExecutablePath = @"C:\ME\Binaries\MassEffect.exe",
                Type = GameType.ME1,
                Edition = GameEdition.Original,
                IsValid = true,
                RequiresAdmin = true
            };

            Assert.Equal("Mass Effect", game.Name);
            Assert.Equal(GameType.ME1, game.Type);
            Assert.Equal(GameEdition.Original, game.Edition);
            Assert.True(game.IsValid);
            Assert.True(game.RequiresAdmin);
        }
    }
}
