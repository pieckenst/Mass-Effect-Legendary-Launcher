using System;
using System.Collections.Generic;
using System.Linq;
using MassEffectLauncher.Models;
using MELE_launcher.Models;
using MELE_launcher.Configuration;
using Spectre.Console;

namespace MassEffectLauncher.Components
{
    public class MenuSystem
    {
        private int _selectedIndex;
        private List<MenuItem> _menuItems;
        private MenuState _currentState;
        private bool _isAdmin;
        private Action<string> _onLocaleChanged;
        private Action<bool> _onForceFeedbackChanged;
        private Action _onRescanGames;
        private Action<string> _onManualPathAdded;
        private LauncherConfig _config;
        private ConfigManager _configManager;

        public MenuSystem()
        {
            _selectedIndex = 0;
            _menuItems = new List<MenuItem>();
            _currentState = MenuState.Main;
            _isAdmin = false;
        }

        /// <summary>
        /// Initializes the menu system with configuration management.
        /// </summary>
        /// <param name="configManager">The configuration manager instance.</param>
        public void Initialize(ConfigManager configManager)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _config = _configManager.Load();

            // Wire up settings callbacks to persist changes
            SetLocaleChangedCallback(locale =>
            {
                _config.DefaultLocale = locale;
                _configManager.Save(_config);
            });

            SetForceFeedbackChangedCallback(enabled =>
            {
                _config.DefaultForceFeedback = enabled;
                _configManager.Save(_config);
            });
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public LauncherConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// Updates the configuration with detected games and saves it.
        /// </summary>
        /// <param name="detectedGames">List of detected games to add to configuration.</param>
        public void UpdateDetectedGames(List<DetectedGame> detectedGames)
        {
            if (_config == null || _configManager == null)
            {
                throw new InvalidOperationException("MenuSystem must be initialized before updating games.");
            }

            // Update last scan date
            _config.LastScanDate = DateTime.Now;

            // Add new games that aren't already in config
            foreach (var game in detectedGames)
            {
                var existingGame = _config.Games.FirstOrDefault(g =>
                    g.Type == game.Type && g.Edition == game.Edition);

                if (existingGame == null)
                {
                    _config.Games.Add(new GameConfig
                    {
                        Type = game.Type,
                        Edition = game.Edition,
                        Path = game.Path,
                        Locale = _config.DefaultLocale,
                        ForceFeedback = _config.DefaultForceFeedback
                    });
                }
                else
                {
                    // Update path if it changed
                    existingGame.Path = game.Path;
                }
            }

            _configManager.Save(_config);
        }

        /// <summary>
        /// Adds a manually specified game path to the configuration.
        /// </summary>
        /// <param name="path">The game installation path.</param>
        /// <param name="gameType">The type of game.</param>
        /// <param name="edition">The edition of the game.</param>
        public void AddManualGamePath(string path, GameType gameType, GameEdition edition)
        {
            if (_config == null || _configManager == null)
            {
                throw new InvalidOperationException("MenuSystem must be initialized before adding games.");
            }

            var existingGame = _config.Games.FirstOrDefault(g =>
                g.Type == gameType && g.Edition == edition);

            if (existingGame == null)
            {
                _config.Games.Add(new GameConfig
                {
                    Type = gameType,
                    Edition = edition,
                    Path = path,
                    Locale = _config.DefaultLocale,
                    ForceFeedback = _config.DefaultForceFeedback
                });
            }
            else
            {
                existingGame.Path = path;
            }

            _configManager.Save(_config);
        }

        public void SetMenuItems(List<MenuItem> items)
        {
            _menuItems = items;
            _selectedIndex = 0;
            
            // Ensure we start on a selectable item (not a separator)
            if (_menuItems.Count > 0 && _menuItems[0].Type == MenuItemType.Separator)
            {
                // Find the first non-separator item
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    if (_menuItems[i].Type != MenuItemType.Separator)
                    {
                        _selectedIndex = i;
                        break;
                    }
                }
            }
        }

        public void SetAdminStatus(bool isAdmin)
        {
            _isAdmin = isAdmin;
        }

        public MenuItem GetSelectedItem()
        {
            if (_menuItems.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _menuItems.Count)
                return null;
            
            return _menuItems[_selectedIndex];
        }

        public MenuState GetCurrentState()
        {
            return _currentState;
        }

        public void SetMenuState(MenuState state)
        {
            _currentState = state;
        }

        public void HandleInput(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    NavigateUp();
                    break;
                
                case ConsoleKey.DownArrow:
                    NavigateDown();
                    break;
                
                case ConsoleKey.Enter:
                    SelectCurrentItem();
                    break;
                
                case ConsoleKey.Escape:
                    HandleEscape();
                    break;
            }
        }

        private void NavigateUp()
        {
            if (_menuItems.Count == 0)
                return;

            do
            {
                _selectedIndex--;
                
                // Wrap to last item if at first
                if (_selectedIndex < 0)
                {
                    _selectedIndex = _menuItems.Count - 1;
                }
            }
            while (_menuItems[_selectedIndex].Type == MenuItemType.Separator && HasSelectableItems());
        }

        private void NavigateDown()
        {
            if (_menuItems.Count == 0)
                return;

            do
            {
                _selectedIndex++;
                
                // Wrap to first item if at last
                if (_selectedIndex >= _menuItems.Count)
                {
                    _selectedIndex = 0;
                }
            }
            while (_menuItems[_selectedIndex].Type == MenuItemType.Separator && HasSelectableItems());
        }

        private bool HasSelectableItems()
        {
            return _menuItems.Any(m => m.Type != MenuItemType.Separator);
        }

        private void SelectCurrentItem()
        {
            var selectedItem = GetSelectedItem();
            
            if (selectedItem != null && selectedItem.IsEnabled && selectedItem.OnSelect != null)
            {
                selectedItem.OnSelect();
            }
        }

        private void HandleEscape()
        {
            // Navigate back to previous menu state or signal exit
            switch (_currentState)
            {
                case MenuState.GameOptions:
                case MenuState.Settings:
                case MenuState.Confirmation:
                    _currentState = MenuState.Main;
                    break;
                
                case MenuState.Main:
                    // Signal to exit application (handled by caller)
                    break;
            }
        }

        public void Render()
        {
            AnsiConsole.Clear();
            RenderHeader();
            RenderMenu();
            RenderStatusBar();
        }

        private void RenderHeader()
        {
            var panel = new Panel(
                new FigletText("MASS EFFECT")
                    .Centered()
                    .Color(Color.Red))
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Red)
            };

            AnsiConsole.Write(panel);
            
            AnsiConsole.MarkupLine("[dim]LEGENDARY LAUNCHER v2.0[/]");
            AnsiConsole.WriteLine();
        }

        private void RenderMenu()
        {
            var table = new Table()
            {
                Border = TableBorder.Rounded,
                BorderStyle = new Style(Color.Aqua)
            };

            table.AddColumn(new TableColumn("").Width(60));

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];
                
                if (item.Type == MenuItemType.Separator)
                {
                    table.AddRow(new Rule()
                    {
                        Style = new Style(Color.Grey)
                    });
                    continue;
                }

                string prefix = i == _selectedIndex ? "[cyan]►[/] " : "  ";
                string title = item.Title;
                
                if (!item.IsEnabled)
                {
                    title = $"[dim]{title}[/]";
                }
                else if (i == _selectedIndex)
                {
                    title = $"[bold cyan]{title}[/]";
                }
                else
                {
                    title = $"[white]{title}[/]";
                }

                table.AddRow(prefix + title);
            }

            var menuPanel = new Panel(table)
            {
                Header = new PanelHeader("[bold]Select an option[/]"),
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Aqua)
            };

            AnsiConsole.Write(menuPanel);
            AnsiConsole.WriteLine();
        }

        private void RenderStatusBar()
        {
            var statusItems = new List<string>();

            // Admin status
            if (_isAdmin)
            {
                statusItems.Add("[green]Admin Mode[/]");
            }
            else
            {
                statusItems.Add("[yellow]Standard Mode[/]");
            }

            // Game count
            int gameCount = _menuItems.Count(m => m.Type == MenuItemType.Game);
            statusItems.Add($"[cyan]{gameCount} Games Detected[/]");

            // Keyboard hints
            statusItems.Add("[dim]↑↓ Navigate[/]");
            statusItems.Add("[dim]Enter Select[/]");
            statusItems.Add("[dim]Esc Back[/]");

            var statusBar = string.Join(" [dim]│[/] ", statusItems);

            var statusPanel = new Panel(statusBar)
            {
                Border = BoxBorder.Heavy,
                BorderStyle = new Style(Color.Grey)
            };

            AnsiConsole.Write(statusPanel);
        }

        public void ShowMessage(string message, MessageType type)
        {
            var color = type switch
            {
                MessageType.Success => Color.Green,
                MessageType.Warning => Color.Yellow,
                MessageType.Error => Color.Red,
                MessageType.Info => Color.Aqua,
                _ => Color.White
            };

            var icon = type switch
            {
                MessageType.Success => "✓",
                MessageType.Warning => "⚠",
                MessageType.Error => "✕",
                MessageType.Info => "ℹ",
                _ => "•"
            };

            var markup = $"[{color}]{icon} {message}[/]";

            var panel = new Panel(markup)
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(color)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            
            // Pause to let user read the message
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        public void ShowProgress(string task, Action action)
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .Start(task, ctx =>
                {
                    action();
                });
        }

        public bool ShowConfirmation(string message)
        {
            var panel = new Panel($"[yellow]❓ {message}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow),
                Header = new PanelHeader("[bold]Confirmation[/]")
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            
            var response = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select your choice:")
                    .AddChoices(new[] { "Yes", "No" })
                    .HighlightStyle(new Style(Color.Aqua)));

            return response == "Yes";
        }

        /// <summary>
        /// Sets the callback for when locale is changed in settings.
        /// </summary>
        public void SetLocaleChangedCallback(Action<string> callback)
        {
            _onLocaleChanged = callback;
        }

        /// <summary>
        /// Sets the callback for when force feedback is changed in settings.
        /// </summary>
        public void SetForceFeedbackChangedCallback(Action<bool> callback)
        {
            _onForceFeedbackChanged = callback;
        }

        /// <summary>
        /// Sets the callback for when rescan games is triggered.
        /// </summary>
        public void SetRescanGamesCallback(Action callback)
        {
            _onRescanGames = callback;
        }

        /// <summary>
        /// Sets the callback for when a manual game path is added.
        /// </summary>
        public void SetManualPathAddedCallback(Action<string> callback)
        {
            _onManualPathAdded = callback;
        }

        /// <summary>
        /// Builds the settings menu with all available options using current config.
        /// </summary>
        public List<MenuItem> BuildSettingsMenu()
        {
            if (_config == null)
            {
                throw new InvalidOperationException("MenuSystem must be initialized with a ConfigManager before building settings menu.");
            }

            return BuildSettingsMenu(_config.DefaultLocale, _config.DefaultForceFeedback);
        }

        /// <summary>
        /// Builds the settings menu with all available options.
        /// </summary>
        /// <param name="currentLocale">The current default locale setting.</param>
        /// <param name="currentForceFeedback">The current default force feedback setting.</param>
        public List<MenuItem> BuildSettingsMenu(string currentLocale, bool currentForceFeedback)
        {
            var menuItems = new List<MenuItem>();

            // Locale selection
            menuItems.Add(new MenuItem
            {
                Title = $"Default Locale: {currentLocale}",
                Description = "Change the default language for games",
                Type = MenuItemType.Setting,
                IsEnabled = true,
                OnSelect = () => ShowLocaleSelection(currentLocale)
            });

            // Force feedback toggle
            menuItems.Add(new MenuItem
            {
                Title = $"Default Force Feedback: {(currentForceFeedback ? "Enabled" : "Disabled")}",
                Description = "Toggle controller force feedback",
                Type = MenuItemType.Setting,
                IsEnabled = true,
                OnSelect = () => ToggleForceFeedback(currentForceFeedback)
            });

            // Separator
            menuItems.Add(new MenuItem
            {
                Title = "",
                Type = MenuItemType.Separator,
                IsEnabled = false
            });

            // Rescan for games
            menuItems.Add(new MenuItem
            {
                Title = "⟳ Rescan for Games",
                Description = "Search for Mass Effect installations",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => TriggerRescan()
            });

            // Manually add game path
            menuItems.Add(new MenuItem
            {
                Title = "+ Add Game Path Manually",
                Description = "Specify a game installation path",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => ShowManualPathInput()
            });

            // Separator
            menuItems.Add(new MenuItem
            {
                Title = "",
                Type = MenuItemType.Separator,
                IsEnabled = false
            });

            // Back to main menu
            menuItems.Add(new MenuItem
            {
                Title = "← Back to Main Menu",
                Description = "Return to the main menu",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => _currentState = MenuState.Main
            });

            return menuItems;
        }

        /// <summary>
        /// Shows locale selection prompt and triggers callback.
        /// </summary>
        private void ShowLocaleSelection(string currentLocale)
        {
            AnsiConsole.Clear();
            
            // Build display choices from available languages
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select default locale:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Aqua)));

            // Find the selected language option
            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.GetDisplayString() == selectedDisplay);

            if (selectedLanguage != null)
            {
                _onLocaleChanged?.Invoke(selectedLanguage.Code);
                ShowMessage($"Default locale changed to: {selectedLanguage.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Toggles force feedback setting and triggers callback.
        /// </summary>
        private void ToggleForceFeedback(bool currentValue)
        {
            var newValue = !currentValue;
            _onForceFeedbackChanged?.Invoke(newValue);
            
            ShowMessage($"Default force feedback {(newValue ? "enabled" : "disabled")}", MessageType.Success);
        }

        /// <summary>
        /// Triggers the rescan games callback.
        /// </summary>
        private void TriggerRescan()
        {
            if (ShowConfirmation("Rescan for Mass Effect game installations?"))
            {
                _onRescanGames?.Invoke();
            }
        }

        /// <summary>
        /// Shows manual path input prompt and triggers callback.
        /// </summary>
        private void ShowManualPathInput()
        {
            AnsiConsole.Clear();
            
            var path = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Enter the game installation path:[/]")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            if (!string.IsNullOrWhiteSpace(path))
            {
                _onManualPathAdded?.Invoke(path);
            }
            else
            {
                ShowMessage("No path entered. Operation cancelled.", MessageType.Warning);
            }
        }

        /// <summary>
        /// Builds the game options menu for a specific detected game.
        /// Displays locale, voice language, and force feedback toggle for Legendary Edition games.
        /// </summary>
        /// <param name="game">The detected game to build options for.</param>
        /// <param name="onLaunch">Callback to invoke when the launch button is selected.</param>
        /// <returns>A list of menu items for the game options submenu.</returns>
        public List<MenuItem> BuildGameOptionsMenu(DetectedGame game, Action<LaunchOptions> onLaunch)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            var menuItems = new List<MenuItem>();

            // Get the game config from saved settings, or use defaults
            var gameConfig = _config?.Games.FirstOrDefault(g => 
                g.Type == game.Type && g.Edition == game.Edition);

            string currentLocale = gameConfig?.Locale ?? _config?.DefaultLocale ?? "INT";
            string currentVoiceLanguage = gameConfig?.VoiceLanguage ?? _config?.DefaultVoiceLanguage ?? "INT";
            bool currentForceFeedback = gameConfig?.ForceFeedback ?? _config?.DefaultForceFeedback ?? false;

            // Display game title
            menuItems.Add(new MenuItem
            {
                Title = $"[bold cyan]{game.Name}[/]",
                Description = game.Path,
                Type = MenuItemType.Separator,
                IsEnabled = false
            });

            // Only show locale and force feedback options for Legendary Edition games
            if (game.Edition == GameEdition.Legendary)
            {
                // Text/Subtitle language selection
                var textLangName = LocaleMapper.GetLanguageOption(currentLocale).DisplayName;
                menuItems.Add(new MenuItem
                {
                    Title = $"Text Language: {textLangName}",
                    Description = "Language for subtitles and menus",
                    Type = MenuItemType.Setting,
                    IsEnabled = true,
                    OnSelect = () => ShowGameTextLanguageSelection(game, currentLocale)
                });

                // Voice-over language selection
                var voiceLangName = LocaleMapper.GetLanguageOption(currentVoiceLanguage).DisplayName;
                var hasNativeVO = LocaleMapper.HasNativeVoiceOver(currentVoiceLanguage, game.Type);
                var voiceDesc = hasNativeVO ? "Native voice-over" : "English voice-over";
                
                menuItems.Add(new MenuItem
                {
                    Title = $"Voice Language: {voiceLangName}",
                    Description = voiceDesc,
                    Type = MenuItemType.Setting,
                    IsEnabled = true,
                    OnSelect = () => ShowGameVoiceLanguageSelection(game, currentVoiceLanguage)
                });

                // Force feedback toggle
                menuItems.Add(new MenuItem
                {
                    Title = $"Force Feedback: {(currentForceFeedback ? "Enabled" : "Disabled")}",
                    Description = "Toggle controller force feedback",
                    Type = MenuItemType.Setting,
                    IsEnabled = true,
                    OnSelect = () => ToggleGameForceFeedback(game, currentForceFeedback)
                });

                // Separator
                menuItems.Add(new MenuItem
                {
                    Title = "",
                    Type = MenuItemType.Separator,
                    IsEnabled = false
                });
            }

            // Launch button
            menuItems.Add(new MenuItem
            {
                Title = "▶ Launch Game",
                Description = "Start the game with selected options",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => 
                {
                    // Get the latest settings before launching
                    var config = _config?.Games.FirstOrDefault(g => 
                        g.Type == game.Type && g.Edition == game.Edition);
                    
                    var launchOptions = new LaunchOptions
                    {
                        Locale = config?.Locale ?? _config?.DefaultLocale ?? "INT",
                        VoiceLanguage = config?.VoiceLanguage ?? _config?.DefaultVoiceLanguage ?? "INT",
                        ForceFeedback = config?.ForceFeedback ?? _config?.DefaultForceFeedback ?? false,
                        Silent = false
                    };
                    
                    onLaunch?.Invoke(launchOptions);
                }
            });

            // Separator
            menuItems.Add(new MenuItem
            {
                Title = "",
                Type = MenuItemType.Separator,
                IsEnabled = false
            });

            // Back to main menu
            menuItems.Add(new MenuItem
            {
                Title = "← Back to Main Menu",
                Description = "Return to the main menu",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => _currentState = MenuState.Main
            });

            return menuItems;
        }

        /// <summary>
        /// Shows text/subtitle language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameTextLanguageSelection(DetectedGame game, string currentLocale)
        {
            AnsiConsole.Clear();
            
            // Build display choices from available languages
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]Select text/subtitle language for {game.Name}:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Aqua)));

            // Find the selected language option
            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.GetDisplayString() == selectedDisplay);

            if (selectedLanguage != null)
            {
                // Update or create game config
                var gameConfig = _config.Games.FirstOrDefault(g => 
                    g.Type == game.Type && g.Edition == game.Edition);

                if (gameConfig == null)
                {
                    gameConfig = new GameConfig
                    {
                        Type = game.Type,
                        Edition = game.Edition,
                        Path = game.Path,
                        Locale = selectedLanguage.Code,
                        VoiceLanguage = _config.DefaultVoiceLanguage,
                        ForceFeedback = _config.DefaultForceFeedback
                    };
                    _config.Games.Add(gameConfig);
                }
                else
                {
                    gameConfig.Locale = selectedLanguage.Code;
                }

                _configManager.Save(_config);
                
                ShowMessage($"Text language for {game.Name} changed to: {selectedLanguage.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Shows voice-over language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameVoiceLanguageSelection(DetectedGame game, string currentVoiceLanguage)
        {
            AnsiConsole.Clear();
            
            // Build display choices with voice-over availability info
            var choices = new List<string>();
            
            foreach (var lang in LocaleMapper.AvailableLanguages)
            {
                bool hasNativeVO = LocaleMapper.HasNativeVoiceOver(lang.Code, game.Type);
                string voiceInfo = hasNativeVO ? "Native VO" : "English VO";
                choices.Add($"{lang.DisplayName} ({voiceInfo})");
            }

            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]Select voice-over language for {game.Name}:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Aqua)));

            // Extract the language name (before the parenthesis)
            var languageName = selectedDisplay.Split('(')[0].Trim();
            
            // Find the selected language option
            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.DisplayName == languageName);

            if (selectedLanguage != null)
            {
                // Update or create game config
                var gameConfig = _config.Games.FirstOrDefault(g => 
                    g.Type == game.Type && g.Edition == game.Edition);

                if (gameConfig == null)
                {
                    gameConfig = new GameConfig
                    {
                        Type = game.Type,
                        Edition = game.Edition,
                        Path = game.Path,
                        Locale = _config.DefaultLocale,
                        VoiceLanguage = selectedLanguage.Code,
                        ForceFeedback = _config.DefaultForceFeedback
                    };
                    _config.Games.Add(gameConfig);
                }
                else
                {
                    gameConfig.VoiceLanguage = selectedLanguage.Code;
                }

                _configManager.Save(_config);
                
                bool hasNativeVO = LocaleMapper.HasNativeVoiceOver(selectedLanguage.Code, game.Type);
                string voiceType = hasNativeVO ? "native voice-over" : "English voice-over";
                ShowMessage($"Voice language for {game.Name} changed to: {selectedLanguage.DisplayName} ({voiceType})", MessageType.Success);
            }
        }

        /// <summary>
        /// Toggles force feedback for a specific game and saves the choice.
        /// </summary>
        private void ToggleGameForceFeedback(DetectedGame game, bool currentValue)
        {
            var newValue = !currentValue;

            // Update or create game config
            var gameConfig = _config.Games.FirstOrDefault(g => 
                g.Type == game.Type && g.Edition == game.Edition);

            if (gameConfig == null)
            {
                gameConfig = new GameConfig
                {
                    Type = game.Type,
                    Edition = game.Edition,
                    Path = game.Path,
                    Locale = _config.DefaultLocale,
                    VoiceLanguage = _config.DefaultVoiceLanguage,
                    ForceFeedback = newValue
                };
                _config.Games.Add(gameConfig);
            }
            else
            {
                gameConfig.ForceFeedback = newValue;
            }

            _configManager.Save(_config);
            
            ShowMessage($"Force feedback for {game.Name} {(newValue ? "enabled" : "disabled")}", MessageType.Success);
        }
         

        /// <summary>
        /// Handles the complete game launch flow including elevation checks.
        /// </summary>
        /// <param name="game">The detected game to launch.</param>
        /// <param name="launchOptions">The launch options including locale, voice language, and force feedback.</param>
        /// <param name="adminElevator">The admin elevator component for privilege checks.</param>
        /// <param name="gameLauncher">The game launcher component.</param>
        public void LaunchGameWithFlow(DetectedGame game, LaunchOptions launchOptions, 
            MELE_launcher.Components.AdminElevator adminElevator, 
            MELE_launcher.Components.GameLauncher gameLauncher)
        {
            if (game == null)
            {
                ShowMessage("Cannot launch: Game is null.", MessageType.Error);
                return;
            }

            if (adminElevator == null)
            {
                ShowMessage("Cannot launch: Admin elevator is not initialized.", MessageType.Error);
                return;
            }

            if (gameLauncher == null)
            {
                ShowMessage("Cannot launch: Game launcher is not initialized.", MessageType.Error);
                return;
            }

            if (launchOptions == null)
            {
                ShowMessage("Cannot launch: Launch options are null.", MessageType.Error);
                return;
            }

            // Check if elevation is required
            if (game.RequiresAdmin && !adminElevator.IsRunningAsAdmin())
            {
                // Prompt for elevation
                bool shouldElevate = ShowConfirmation(
                    $"{game.Name} is installed in a protected location and requires administrator privileges.\n" +
                    "Would you like to restart the launcher with administrator privileges?");

                if (shouldElevate)
                {
                    // Request elevation
                    bool elevationSuccess = adminElevator.RequestElevation(Environment.GetCommandLineArgs());

                    if (elevationSuccess)
                    {
                        ShowMessage("Restarting with administrator privileges...", MessageType.Info);
                        // The application will be restarted with elevation, so we should exit
                        Environment.Exit(0);
                    }
                    else
                    {
                        ShowMessage(
                            "Failed to obtain administrator privileges. The game may not launch correctly.\n" +
                            "You can try running the launcher as administrator manually.",
                            MessageType.Error);
                        return;
                    }
                }
                else
                {
                    ShowMessage("Launch cancelled. Administrator privileges are required for this game.", MessageType.Warning);
                    return;
                }
            }

            // Show launching message with language info
            AnsiConsole.Clear();
            var textLang = LocaleMapper.GetLanguageOption(launchOptions.Locale).DisplayName;
            var voiceLang = LocaleMapper.GetLanguageOption(launchOptions.VoiceLanguage).DisplayName;
            var hasNativeVO = LocaleMapper.HasNativeVoiceOver(launchOptions.VoiceLanguage, game.Type);
            var voiceType = hasNativeVO ? "native" : "English";
            
            AnsiConsole.MarkupLine($"[cyan]Launching {game.Name}...[/]");
            AnsiConsole.MarkupLine($"[dim]Text: {textLang} | Voice: {voiceLang} ({voiceType} VO)[/]");
            AnsiConsole.WriteLine();

            // Launch the game
            LaunchResult result = null;
            
            ShowProgress("Starting game...", () =>
            {
                result = gameLauncher.Launch(game, launchOptions);
            });

            // Display result
            if (result != null && result.Success)
            {
                ShowMessage($"{game.Name} launched successfully!", MessageType.Success);
            }
            else
            {
                string errorMsg = result?.ErrorMessage ?? "Unknown error occurred.";
                ShowMessage($"Failed to launch {game.Name}:\n{errorMsg}", MessageType.Error);
            }
        }
    }
}
