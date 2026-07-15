using System.Collections.Generic;
using System.Linq;
using MELE_launcher.Components;
using MELE_launcher.Models;
using Xunit;

namespace MELE_launcher.Tests
{
    public class CommandExecutorTests
    {
        private static CommandExecutor NewExecutor() => new CommandExecutor();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Execute_BlankInput_ReturnsNull(string input)
        {
            Assert.Null(NewExecutor().Execute(input));
        }

        [Fact]
        public void Execute_UnknownCommand_ReturnsError()
        {
            var result = NewExecutor().Execute("nonsense");
            Assert.Contains("Unknown directive", result);
            Assert.Contains("nonsense", result);
        }

        [Fact]
        public void Execute_Version_ReturnsVersionBanner()
        {
            var result = NewExecutor().Execute("version");
            Assert.Contains("MASS EFFECT LEGENDARY LAUNCHER", result);
        }

        [Fact]
        public void Execute_ResolvesAliasesCaseInsensitively()
        {
            var executor = NewExecutor();
            // "ver" and "about" are aliases of "version"; command matching is case-insensitive.
            Assert.Contains("MASS EFFECT", executor.Execute("VER"));
            Assert.Contains("MASS EFFECT", executor.Execute("about"));
        }

        [Fact]
        public void Execute_Clear_ReturnsClearToken()
        {
            Assert.Equal("[CLEAR]", NewExecutor().Execute("clear"));
        }

        [Fact]
        public void Execute_Help_ListsRegistry()
        {
            var result = NewExecutor().Execute("help");
            Assert.Contains("SYSTEM COMMAND REGISTRY", result);
            Assert.Contains("launch", result);
        }

        [Fact]
        public void Execute_HelpForSpecificCommand_ShowsUsage()
        {
            var result = NewExecutor().Execute("help launch");
            Assert.Contains("LAUNCH", result);
            Assert.Contains("launch <1|2|3>", result);
        }

        [Fact]
        public void Execute_HelpForUnknownCommand_ReturnsError()
        {
            var result = NewExecutor().Execute("help bogus");
            Assert.Contains("Unknown directive", result);
        }

        [Fact]
        public void Execute_Rescan_InvokesCallback()
        {
            var executor = NewExecutor();
            bool called = false;
            executor.RegisterCallbacks(
                onRescan: () => called = true,
                onExit: () => { },
                onSettings: () => { },
                getDetectedGames: () => new List<DetectedGame>(),
                onLaunchGame: (_, __) => { },
                getConfig: () => new LauncherConfig());

            var result = executor.Execute("scan");

            Assert.True(called);
            Assert.Contains("sensors refreshed", result);
        }

        [Fact]
        public void Execute_Exit_InvokesCallback()
        {
            var executor = NewExecutor();
            bool called = false;
            executor.RegisterCallbacks(() => { }, () => called = true, () => { },
                () => new List<DetectedGame>(), (_, __) => { }, () => new LauncherConfig());

            executor.Execute("exit");

            Assert.True(called);
        }

        [Fact]
        public void Execute_Settings_ReturnsNullAndInvokesCallback()
        {
            var executor = NewExecutor();
            bool called = false;
            executor.RegisterCallbacks(() => { }, () => { }, () => called = true,
                () => new List<DetectedGame>(), (_, __) => { }, () => new LauncherConfig());

            var result = executor.Execute("settings");

            Assert.True(called);
            Assert.Null(result);
        }

        [Fact]
        public void Execute_List_NoGames_ReturnsEmptyMessage()
        {
            var result = NewExecutor().Execute("list");
            Assert.Contains("No Mass Effect modules detected", result);
        }

        [Fact]
        public void Execute_List_WithGames_ListsThem()
        {
            var executor = NewExecutor();
            executor.RegisterCallbacks(() => { }, () => { }, () => { },
                () => new List<DetectedGame>
                {
                    new DetectedGame { Name = "Mass Effect 1", Type = GameType.ME1, Edition = GameEdition.Legendary, IsValid = true }
                },
                (_, __) => { }, () => new LauncherConfig());

            var result = executor.Execute("list");

            Assert.Contains("INSTALLED MODULES", result);
            Assert.Contains("Mass Effect 1", result);
            Assert.Contains("READY", result);
        }

        [Theory]
        [InlineData("launch")]        // missing argument
        [InlineData("launch 0")]      // out of range
        [InlineData("launch 4")]      // out of range
        [InlineData("launch abc")]    // not a number
        public void Execute_LaunchWithBadArgs_ReturnsError(string command)
        {
            var result = NewExecutor().Execute(command);
            Assert.Contains("ERROR", result);
        }

        [Fact]
        public void Execute_LaunchMissingGame_ReturnsNotFound()
        {
            var executor = NewExecutor();
            executor.RegisterCallbacks(() => { }, () => { }, () => { },
                () => new List<DetectedGame>(), (_, __) => { }, () => new LauncherConfig());

            var result = executor.Execute("launch 1");

            Assert.Contains("not found", result);
        }

        [Fact]
        public void Execute_LaunchInvalidGame_ReturnsIntegrityError()
        {
            var executor = NewExecutor();
            executor.RegisterCallbacks(() => { }, () => { }, () => { },
                () => new List<DetectedGame>
                {
                    new DetectedGame { Name = "ME1", Type = GameType.ME1, Edition = GameEdition.Legendary, IsValid = false }
                },
                (_, __) => { }, () => new LauncherConfig());

            var result = executor.Execute("launch 1");

            Assert.Contains("Integrity check failed", result);
        }

        [Fact]
        public void Execute_LaunchValidGame_InvokesCallbackWithConfiguredOptions()
        {
            var executor = NewExecutor();
            DetectedGame launchedGame = null;
            LaunchOptions launchedOptions = null;

            var config = new LauncherConfig
            {
                DefaultSkipIntro = true,
                Games =
                {
                    new GameConfig
                    {
                        Type = GameType.ME2,
                        Edition = GameEdition.Legendary,
                        Locale = "FR",
                        VoiceLanguage = "DE",
                        ForceFeedback = true
                    }
                }
            };

            executor.RegisterCallbacks(() => { }, () => { }, () => { },
                () => new List<DetectedGame>
                {
                    new DetectedGame { Name = "Mass Effect 2", Type = GameType.ME2, Edition = GameEdition.Legendary, IsValid = true }
                },
                (game, options) => { launchedGame = game; launchedOptions = options; },
                () => config);

            var result = executor.Execute("launch 2");

            Assert.Contains("SUCCESS", result);
            Assert.NotNull(launchedGame);
            Assert.Equal(GameType.ME2, launchedGame.Type);
            Assert.NotNull(launchedOptions);
            Assert.Equal("FR", launchedOptions.Locale);
            Assert.Equal("DE", launchedOptions.VoiceLanguage);
            Assert.True(launchedOptions.ForceFeedback);
            Assert.False(launchedOptions.PlayIntro); // DefaultSkipIntro == true => PlayIntro false
        }

        [Fact]
        public void Execute_LaunchWithQuotedArgument_ParsesCorrectly()
        {
            var executor = NewExecutor();
            executor.RegisterCallbacks(() => { }, () => { }, () => { },
                () => new List<DetectedGame>
                {
                    new DetectedGame { Name = "ME3", Type = GameType.ME3, Edition = GameEdition.Legendary, IsValid = true }
                },
                (_, __) => { }, () => new LauncherConfig());

            var result = executor.Execute("launch \"3\"");

            Assert.Contains("SUCCESS", result);
        }

        [Fact]
        public void GetAvailableCommands_ReturnsEnabledCommandsSorted()
        {
            var commands = NewExecutor().GetAvailableCommands();

            Assert.NotEmpty(commands);
            Assert.All(commands, c => Assert.True(c.Enabled));
            Assert.Contains(commands, c => c.Name == "launch");
            Assert.Contains(commands, c => c.Name == "version");

            var categories = commands.Select(c => c.Category).ToList();
            var sortedCategories = categories.OrderBy(x => x).ToList();
            Assert.Equal(sortedCategories, categories);
        }
    }
}
