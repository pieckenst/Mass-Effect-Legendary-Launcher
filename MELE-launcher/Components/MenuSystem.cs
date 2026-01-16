using System;
using System.Collections.Generic;
using System.Linq;
using MassEffectLauncher.Models;
using MELE_launcher.Models;
using MELE_launcher.Configuration;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MassEffectLauncher.Components
{
    public class MenuSystem
    {
        #region State
        private int _selectedIndex;
        private List<MenuItem> _menuItems;
        private MenuState _currentState;
        #endregion

        #region Configuration & Context
        private bool _isAdmin;
        private LauncherConfig _config;
        private ConfigManager _configManager;
        #endregion

        #region Callbacks
        private Action<string> _onLocaleChanged;
        private Action<bool> _onForceFeedbackChanged;
        private Action<bool> _onSkipIntroChanged;
        private Action _onRescanGames;
        private Action<string> _onManualPathAdded;
        #endregion

        #region Visual Theme Management
        private readonly Theme _theme = new Theme();
        #endregion

        public MenuSystem()
        {
            _selectedIndex = 0;
            _menuItems = new List<MenuItem>();
            _currentState = MenuState.Main;
            _isAdmin = false;
        }

        #region Initialization & Configuration
        
        /// <summary>
        /// Initializes the menu system with configuration management.
        /// </summary>
        /// <param name="configManager">The configuration manager instance.</param>
        public void Initialize(ConfigManager configManager)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _config = _configManager.Load();

            // Wire up settings callbacks to persist changes immediately
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

            SetSkipIntroChangedCallback(enabled =>
            {
                _config.DefaultSkipIntro = enabled;
                _configManager.Save(_config);
            });
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        public LauncherConfig GetConfig() => _config;

        public void SetAdminStatus(bool isAdmin)
        {
            _isAdmin = isAdmin;
            if (_isAdmin) _theme.SetAdminMode();
            else _theme.SetStandardMode();
        }

        #endregion

        #region Data Helpers & Configuration Updates

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

        #endregion

        #region Menu Logic & Navigation

        public void SetMenuItems(List<MenuItem> items)
        {
            _menuItems = items;
            _selectedIndex = 0;
            EnsureValidSelection(1); // Move forward to find first selectable item
        }

        public void SetMenuState(MenuState state) => _currentState = state;

        public MenuState GetCurrentState() => _currentState;

        public MenuItem GetSelectedItem() => 
            (_selectedIndex >= 0 && _selectedIndex < _menuItems.Count) ? _menuItems[_selectedIndex] : null;

        public void HandleInput(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow: 
                    MoveSelection(-1); 
                    break;
                
                case ConsoleKey.DownArrow: 
                    MoveSelection(1); 
                    break;
                
                case ConsoleKey.Home: 
                    _selectedIndex = 0; 
                    EnsureValidSelection(1); 
                    break;
                
                case ConsoleKey.End: 
                    _selectedIndex = _menuItems.Count - 1; 
                    EnsureValidSelection(-1); 
                    break;
                
                case ConsoleKey.Enter: 
                    ExecuteSelection(); 
                    break;
                
                case ConsoleKey.Escape: 
                    HandleBack(); 
                    break;
            }
        }

        private void MoveSelection(int direction)
        {
            if (_menuItems.Count == 0) return;

            int originalIndex = _selectedIndex;
            int newIndex = _selectedIndex;

            do
            {
                newIndex += direction;
                
                // Wrap around
                if (newIndex >= _menuItems.Count) newIndex = 0;
                if (newIndex < 0) newIndex = _menuItems.Count - 1;

                // Found a selectable item
                if (_menuItems[newIndex].Type != MenuItemType.Separator)
                {
                    _selectedIndex = newIndex;
                    return;
                }
            } 
            while (newIndex != originalIndex);
        }

        private void EnsureValidSelection(int direction)
        {
            if (_menuItems.Count == 0) return;
            
            if (_menuItems[_selectedIndex].Type == MenuItemType.Separator)
            {
                MoveSelection(direction);
            }
        }

        private void ExecuteSelection()
        {
            var item = GetSelectedItem();
            if (item != null && item.IsEnabled && item.OnSelect != null) 
                item.OnSelect();
        }

        private void HandleBack()
        {
            if (_currentState != MenuState.Main) 
                _currentState = MenuState.Main;
        }

        #endregion

        #region Render Engine (Dashboard Layout)

        public void Render()
        {
            // Root Layout
            var rootLayout = new Layout("Root")
                .SplitRows(
                    new Layout("Header").Size(4),
                    new Layout("Body"),
                    new Layout("Footer").Size(3)
                );

            rootLayout["Header"].Update(RenderHeader());
            rootLayout["Body"].Update(RenderDashboardBody());
            rootLayout["Footer"].Update(RenderStatusBar());

            AnsiConsole.Clear();
            AnsiConsole.Write(rootLayout);
        }

        private IRenderable RenderHeader()
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().NoWrap());
            grid.AddColumn(new GridColumn().RightAligned());

            var textTitle = new Markup($"[bold {_theme.AccentName}]MASS EFFECT[/] [white]LEGENDARY LAUNCHER[/]");
            var subTitle = new Markup($"[dim]SYSTEMS ALLIANCE TERMINAL // V2.1.0[/]");
            var userInfo = new Markup($"[dim]USER: {Environment.UserName.ToUpper()}[/]\n[{_theme.SecondaryName}]HOST: {Environment.MachineName}[/]");

            grid.AddRow(textTitle, userInfo);
            grid.AddRow(subTitle, new Text(""));

            return new Panel(grid) 
            { 
                Border = BoxBorder.None, 
                Padding = new Padding(1, 0, 1, 0) 
            };
        }

        private IRenderable RenderDashboardBody()
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().Width(40)); // Navigation
            grid.AddColumn(new GridColumn().Padding(2, 0, 0, 0)); // Context

            // Panel 1: Menu List
            var menuTable = new Table().NoBorder().HideHeaders().Expand();
            menuTable.AddColumn("State");
            menuTable.AddColumn("Title");

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];
                
                if (item.Type == MenuItemType.Separator)
                {
                    menuTable.AddRow(new Markup(""), new Rule { Style = new Style(_theme.Muted) });
                    continue;
                }

                bool isSelected = (i == _selectedIndex);
                
                if (isSelected)
                {
                    menuTable.AddRow(
                        new Markup($"[{_theme.HighlightName}]▌[/]"), 
                        new Markup($"[black on {_theme.HighlightName}] {item.Title} [/]")
                    );
                }
                else
                {
                    string style = item.IsEnabled ? "white" : "grey";
                    menuTable.AddRow(
                        new Markup(" "), 
                        new Markup($"[{style}]{item.Title}[/]")
                    );
                }
            }

            var leftPanel = new Panel(menuTable)
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(_theme.Border),
                Header = new PanelHeader($" [bold]{GetMenuTitle()}[/] "),
                Padding = new Padding(1, 1, 1, 1),
                Expand = true
            };

            // Panel 2: Context Details
            grid.AddRow(leftPanel, RenderContextPanel());

            return grid;
        }

        private string GetMenuTitle() => _currentState switch
        {
            MenuState.GameOptions => "TACTICAL",
            MenuState.Settings => "CONFIG",
            _ => "MISSIONS"
        };

        private IRenderable RenderContextPanel()
        {
            var selected = GetSelectedItem();
            if (selected == null) return new Panel("");

            var content = new List<IRenderable>();

            // Header Art Code
            var headerText = GetContextCode(selected);
            content.Add(new FigletText(headerText).Color(_theme.Muted));

            // Description
            var rule = new Rule($"[{_theme.SecondaryName}]{selected.Title}[/]");
            rule.Justification = Justify.Left;
            content.Add(rule);
            content.Add(new Text(""));
            content.Add(new Markup($"[white]{selected.Description}[/]\n"));

            // Rich Details (Tagging)
            if (selected.Tag is DetectedGame game) 
                RenderGameDetails(game, content);
            else if (selected.Type == MenuItemType.Setting) 
                RenderSettingDetails(selected, content);

            return new Panel(new Rows(content))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(_theme.Border),
                Header = new PanelHeader(" [bold]INTEL[/] "),
                Padding = new Padding(2, 1, 2, 1),
                Expand = true
            };
        }

        private void RenderGameDetails(DetectedGame game, List<IRenderable> content)
        {
            content.Add(new Text(""));
            
            var table = new Table().NoBorder().HideHeaders();
            table.AddColumn("Key");
            table.AddColumn("Value");

            string statusColor = game.IsValid ? "green" : "red";
            string statusText = game.IsValid ? "READY" : "INVALID";
            
            table.AddRow(new Markup("[dim]STATUS:[/]"), new Markup($"[{statusColor}]{statusText}[/]"));
            table.AddRow(new Markup("[dim]PATH:[/]"), new Text(TruncatePath(game.Path)));

            // Config Lookup for live preview
            var gameConfig = _config?.Games.FirstOrDefault(g => g.Type == game.Type && g.Edition == game.Edition);
            
            if (gameConfig != null)
            {
                var textLang = LocaleMapper.GetLanguageOption(gameConfig.Locale).DisplayName;
                var voiceLang = LocaleMapper.GetLanguageOption(gameConfig.VoiceLanguage).DisplayName;
                var hasNativeVO = LocaleMapper.HasNativeVoiceOver(gameConfig.VoiceLanguage, game.Type);
                
                table.AddRow(new Markup("[dim]TEXT:[/]"), new Markup($"[cyan]{textLang}[/]"));
                table.AddRow(new Markup("[dim]AUDIO:[/]"), new Markup($"[cyan]{voiceLang}[/]"));
                table.AddRow(new Markup("[dim]VO TYPE:[/]"), new Markup(hasNativeVO ? "[green]Native[/]" : "[yellow]English Dub[/]"));
            }

            content.Add(table);
        }

        private void RenderSettingDetails(MenuItem item, List<IRenderable> content)
        {
            content.Add(new Text(""));
            content.Add(new Markup("[dim]Select to modify this configuration value.[/]"));
        }

        private string TruncatePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.Length > 40) return path.Substring(0, 15) + "..." + path.Substring(path.Length - 20);
            return path;
        }

        private string GetContextCode(MenuItem item)
        {
            if (item.Tag is DetectedGame dg) return $"ME {(int)dg.Type + 1}";
            if (item.Title.Contains("Settings")) return "SYS";
            if (item.Title.Contains("Exit")) return "OFF";
            return "CMD";
        }

        private IRenderable RenderStatusBar()
        {
            var table = new Table().NoBorder().HideHeaders().Expand();
            table.AddColumn("Info");
            table.AddColumn(new TableColumn("Keys").RightAligned());

            string statusBadge = _isAdmin 
                ? "[black on red] ADMINISTRATOR MODE [/]" 
                : "[black on orange1] STANDARD USER MODE [/]";
            
            string keys = "[dim]↑/↓[/] [cyan]Nav[/]  [dim]HOME/END[/] [cyan]Jump[/]  [dim]RET[/] [cyan]Sel[/]  [dim]ESC[/] [cyan]Back[/]";

            table.AddRow(new Markup(statusBadge), new Markup(keys));

            return new Panel(table) 
            { 
                Border = BoxBorder.None, 
                Padding = new Padding(1, 1, 1, 0) 
            };
        }

        #endregion

        #region Actions & Popups (Functionality Restored)

        public void ShowMessage(string message, MessageType type)
        {
            var color = type switch
            {
                MessageType.Success => Color.Green,
                MessageType.Warning => Color.Orange1,
                MessageType.Error => Color.Red,
                _ => Color.Cyan1
            };

            var box = new Panel(new Markup($"[{color}]{message}[/]"))
            {
                Border = BoxBorder.Heavy,
                BorderStyle = new Style(color),
                Header = new PanelHeader($"[bold]{type.ToString().ToUpper()}[/]"),
                Padding = new Padding(2, 1, 2, 1)
            };

            AnsiConsole.Clear();
            AnsiConsole.Write(new Padder(box, new Padding(4)));
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        public bool ShowConfirmation(string message)
        {
            AnsiConsole.Clear();
            
            var panel = new Panel(new Markup($"[orange1 bold]{message}[/]"))
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Orange1),
                Header = new PanelHeader(" [bold]CONFIRM ACTION[/] "),
                Padding = new Padding(2, 1, 2, 1)
            };

            AnsiConsole.Write(new Padder(panel, new Padding(4)));

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices("Yes", "No")
                    .HighlightStyle(new Style(Color.Black, Color.Orange1)));

            return selection == "Yes";
        }

        public void ShowProgress(string task, Action action)
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .Start($" [cyan]{task}[/]", ctx => action());
        }

        #endregion

        #region Callback Registration

        /// <summary>
        /// Sets the callback for when locale is changed in settings.
        /// </summary>
        public void SetLocaleChangedCallback(Action<string> callback) => _onLocaleChanged = callback;

        /// <summary>
        /// Sets the callback for when force feedback is changed in settings.
        /// </summary>
        public void SetForceFeedbackChangedCallback(Action<bool> callback) => _onForceFeedbackChanged = callback;

        /// <summary>
        /// Sets the callback for when skip intro is changed in settings.
        /// </summary>
        public void SetSkipIntroChangedCallback(Action<bool> callback) => _onSkipIntroChanged = callback;

        /// <summary>
        /// Sets the callback for when rescan games is triggered.
        /// </summary>
        public void SetRescanGamesCallback(Action callback) => _onRescanGames = callback;

        /// <summary>
        /// Sets the callback for when a manual game path is added.
        /// </summary>
        public void SetManualPathAddedCallback(Action<string> callback) => _onManualPathAdded = callback;

        #endregion

        #region Menu Builders

        /// <summary>
        /// Builds the settings menu with all available options using current config.
        /// </summary>
        public List<MenuItem> BuildSettingsMenu()
        {
            if (_config == null)
            {
                throw new InvalidOperationException("MenuSystem must be initialized with a ConfigManager before building settings menu.");
            }

            return BuildSettingsMenu(_config.DefaultLocale, _config.DefaultForceFeedback, _config.DefaultSkipIntro);
        }

        /// <summary>
        /// Builds the settings menu with all available options.
        /// </summary>
        /// <param name="currentLocale">The current default locale setting.</param>
        /// <param name="currentForceFeedback">The current default force feedback setting.</param>
        /// <param name="currentSkipIntro">The current default skip intro setting.</param>
        public List<MenuItem> BuildSettingsMenu(string currentLocale, bool currentForceFeedback, bool currentSkipIntro)
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
                Title = $"Default Rumble: {(currentForceFeedback ? "ON" : "OFF")}",
                Description = "Toggle controller force feedback",
                Type = MenuItemType.Setting,
                IsEnabled = true,
                OnSelect = () => ToggleForceFeedback(currentForceFeedback)
            });

            // Skip intro toggle
            menuItems.Add(new MenuItem
            {
                Title = $"Skip BioWare Intro: {(currentSkipIntro ? "ON" : "OFF")}",
                Description = "Skip startup videos",
                Type = MenuItemType.Setting,
                IsEnabled = true,
                OnSelect = () => ToggleSkipIntro(currentSkipIntro)
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
                Title = "Rescan for Games",
                Description = "Search detection paths",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => TriggerRescan()
            });

            // Manually add game path
            menuItems.Add(new MenuItem
            {
                Title = "Add Game Path Manually",
                Description = "Specify game folder",
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
                Title = "Back to Main Menu",
                Description = "Return",
                Type = MenuItemType.Action,
                IsEnabled = true,
                OnSelect = () => _currentState = MenuState.Main
            });

            return menuItems;
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

            var list = new List<MenuItem>();

            // Get the game config from saved settings, or use defaults
            var gameConfig = _config?.Games.FirstOrDefault(g => 
                g.Type == game.Type && g.Edition == game.Edition);

            string loc = gameConfig?.Locale ?? _config?.DefaultLocale ?? "INT";
            string vo = gameConfig?.VoiceLanguage ?? _config?.DefaultVoiceLanguage ?? "INT";
            bool ff = gameConfig?.ForceFeedback ?? _config?.DefaultForceFeedback ?? false;

            // Title Header (Tag is set to game so Context Panel renders details!)
            list.Add(new MenuItem 
            { 
                Title = game.Name, 
                Description = game.Path, 
                Type = MenuItemType.Separator, 
                Tag = game 
            });

            // Only show locale and force feedback options for Legendary Edition games
            if (game.Edition == GameEdition.Legendary)
            {
                // Text/Subtitle language selection
                var textLangName = LocaleMapper.GetLanguageOption(loc).DisplayName;
                list.Add(new MenuItem 
                { 
                    Title = $"Text Language: {textLangName}", 
                    Description = "Subtitle/UI Language", 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ShowGameTextLanguageSelection(game, loc)
                });

                // Voice-over language selection - RESTORED: Dynamic description based on native voice support
                var voiceLangName = LocaleMapper.GetLanguageOption(vo).DisplayName;
                var hasNativeVO = LocaleMapper.HasNativeVoiceOver(vo, game.Type);
                var voiceDesc = hasNativeVO ? "Native voice-over" : "English voice-over";
                
                list.Add(new MenuItem 
                { 
                    Title = $"Voice Language: {voiceLangName}", 
                    Description = voiceDesc, 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ShowGameVoiceLanguageSelection(game, vo)
                });

                // Force feedback toggle
                list.Add(new MenuItem 
                { 
                    Title = $"Force Feedback: {(ff ? "ON" : "OFF")}", 
                    Description = "Controller Vibration", 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ToggleGameForceFeedback(game, ff)
                });

                // Separator
                list.Add(new MenuItem 
                { 
                    Type = MenuItemType.Separator 
                });
            }

            // Launch button
            list.Add(new MenuItem 
            { 
                Title = "LAUNCH GAME", 
                Description = "Execute with current settings",
                Type = MenuItemType.Action,
                Tag = game,
                IsEnabled = true,
                OnSelect = () => 
                {
                    var finalConfig = _config?.Games.FirstOrDefault(g => g.Type == game.Type && g.Edition == game.Edition);
                    onLaunch(new LaunchOptions 
                    {
                        Locale = finalConfig?.Locale ?? loc,
                        VoiceLanguage = finalConfig?.VoiceLanguage ?? vo,
                        ForceFeedback = finalConfig?.ForceFeedback ?? ff,
                        PlayIntro = !(_config?.DefaultSkipIntro ?? true),
                        Silent = false
                    });
                }
            });

            // Separator
            list.Add(new MenuItem 
            { 
                Type = MenuItemType.Separator 
            });

            // Back to main menu
            list.Add(new MenuItem 
            { 
                Title = "Back to Main Menu", 
                Type = MenuItemType.Action, 
                IsEnabled = true,
                OnSelect = () => _currentState = MenuState.Main 
            });

            return list;
        }

        #endregion

        #region Helper Logic (Restored Callbacks & Success Messages)

        /// <summary>
        /// Shows locale selection prompt and triggers callback.
        /// </summary>
        private void ShowLocaleSelection(string current) 
        {
            AnsiConsole.Clear();
            
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Select default locale:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Black, Color.Cyan1)));

            var lang = LocaleMapper.AvailableLanguages
                .FirstOrDefault(l => l.GetDisplayString() == selectedDisplay);

            if (lang != null) 
            {
                _onLocaleChanged?.Invoke(lang.Code);
                ShowMessage($"Default locale changed to: {lang.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Toggles force feedback setting and triggers callback.
        /// </summary>
        private void ToggleForceFeedback(bool current) 
        {
            bool newValue = !current;
            _onForceFeedbackChanged?.Invoke(newValue);
            ShowMessage($"Default force feedback {(newValue ? "enabled" : "disabled")}", MessageType.Success);
        }

        /// <summary>
        /// Toggles skip intro setting and triggers callback.
        /// </summary>
        private void ToggleSkipIntro(bool current)
        {
            bool newValue = !current;
            _onSkipIntroChanged?.Invoke(newValue);
            ShowMessage($"Skip BioWare intro {(newValue ? "enabled" : "disabled")}", MessageType.Success);
        }

        /// <summary>
        /// Triggers the rescan games callback.
        /// </summary>
        private void TriggerRescan() 
        { 
            if (ShowConfirmation("Rescan for Mass Effect game installations?")) 
                _onRescanGames?.Invoke(); 
        }

        /// <summary>
        /// Shows manual path input prompt and triggers callback.
        /// </summary>
        private void ShowManualPathInput() 
        {
            AnsiConsole.Clear();
            
            var p = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Enter the game installation path:[/]")
                    .AllowEmpty());

            if (!string.IsNullOrWhiteSpace(p)) 
                _onManualPathAdded?.Invoke(p);
            else 
                ShowMessage("No path entered. Operation cancelled.", MessageType.Warning);
        }

        /// <summary>
        /// Shows text/subtitle language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameTextLanguageSelection(DetectedGame game, string currentLocale)
        {
            AnsiConsole.Clear();
            
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]Select text/subtitle language for {game.Name}:[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Black, Color.Cyan1)));

            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.GetDisplayString() == selectedDisplay);

            if (selectedLanguage != null)
            {
                UpdateGameConfig(game, conf => conf.Locale = selectedLanguage.Code);
                ShowMessage($"Text language for {game.Name} changed to: {selectedLanguage.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Shows voice-over language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameVoiceLanguageSelection(DetectedGame game, string currentVoiceLanguage)
        {
            AnsiConsole.Clear();
            
            // RESTORED: Logic to build display strings with "Native VO" indicators
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
                    .HighlightStyle(new Style(Color.Black, Color.Cyan1)));

            // RESTORED: Logic to parse the display string back to a language
            var languageName = selectedDisplay.Split('(')[0].Trim();
            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.DisplayName == languageName);

            if (selectedLanguage != null)
            {
                UpdateGameConfig(game, conf => conf.VoiceLanguage = selectedLanguage.Code);
                
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
            bool newValue = !currentValue;
            UpdateGameConfig(game, conf => conf.ForceFeedback = newValue);
            ShowMessage($"Force feedback for {game.Name} {(newValue ? "enabled" : "disabled")}", MessageType.Success);
        }

        /// <summary>
        /// Updates or creates game configuration with the specified action.
        /// </summary>
        private void UpdateGameConfig(DetectedGame game, Action<GameConfig> action)
        {
            var cfg = _config.Games.FirstOrDefault(x => x.Type == game.Type && x.Edition == game.Edition);
            
            if (cfg == null) 
            {
                // Create new config if missing
                cfg = new GameConfig 
                { 
                    Type = game.Type, 
                    Edition = game.Edition, 
                    Path = game.Path, 
                    Locale = _config.DefaultLocale, 
                    VoiceLanguage = _config.DefaultVoiceLanguage 
                };
                _config.Games.Add(cfg);
            }

            action(cfg);
            _configManager.Save(_config);
        }

        #endregion

        #region Game Launch Flow

        /// <summary>
        /// Handles the complete game launch flow including elevation checks.
        /// </summary>
        /// <param name="game">The detected game to launch.</param>
        /// <param name="options">The launch options including locale, voice language, and force feedback.</param>
        /// <param name="admin">The admin elevator component for privilege checks.</param>
        /// <param name="launcher">The game launcher component.</param>
        public void LaunchGameWithFlow(DetectedGame game, LaunchOptions options, 
            MELE_launcher.Components.AdminElevator admin, 
            MELE_launcher.Components.GameLauncher launcher)
        {
            if (game == null) { ShowMessage("Cannot launch: Game is null.", MessageType.Error); return; }
            if (admin == null) { ShowMessage("Cannot launch: Admin elevator is not initialized.", MessageType.Error); return; }
            if (launcher == null) { ShowMessage("Cannot launch: Game launcher is not initialized.", MessageType.Error); return; }

            // Admin Elevation Logic
            if (game.RequiresAdmin && !admin.IsRunningAsAdmin())
            {
                bool shouldElevate = ShowConfirmation(
                    $"{game.Name} is installed in a protected location and requires administrator privileges.\n" +
                    "Would you like to restart the launcher with administrator privileges?");

                if (shouldElevate)
                {
                    bool elevationSuccess = admin.RequestElevation(Environment.GetCommandLineArgs());

                    if (elevationSuccess)
                    {
                        ShowMessage("Restarting with administrator privileges...", MessageType.Info);
                        Environment.Exit(0);
                    }
                    else
                    {
                        ShowMessage("Failed to obtain administrator privileges. The game may not launch correctly.", MessageType.Error);
                        return;
                    }
                }
                else
                {
                    ShowMessage("Launch cancelled. Administrator privileges are required.", MessageType.Warning);
                    return;
                }
            }

            // Launch UI
            AnsiConsole.Clear();
            
            var textLang = LocaleMapper.GetLanguageOption(options.Locale).DisplayName;
            var voiceLang = LocaleMapper.GetLanguageOption(options.VoiceLanguage).DisplayName;
            var hasNativeVO = LocaleMapper.HasNativeVoiceOver(options.VoiceLanguage, game.Type);
            var voiceType = hasNativeVO ? "native" : "English";
            
            var panel = new Panel(new Markup($"[cyan]Launching {game.Name}...[/]\n[dim]Text: {textLang} | Voice: {voiceLang} ({voiceType} VO)[/]")) 
            { 
                Border = BoxBorder.Heavy, 
                BorderStyle = new Style(Color.Cyan1) 
            };

            AnsiConsole.Write(new Padder(panel, new Padding(4)));

            LaunchResult result = null;
            ShowProgress("Starting game...", () => { result = launcher.Launch(game, options); });

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

        #endregion

        #region Visual Theme Structure

        // Visual Theme Struct
        private class Theme
        {
            public Color Primary, Secondary, Accent, Muted, Highlight, Border;
            public string SecondaryName, AccentName, HighlightName;

            public Theme() => SetStandardMode();

            public void SetStandardMode()
            {
                Primary = Color.White; 
                Secondary = Color.SlateBlue1; 
                Accent = Color.Cyan1;
                Muted = Color.Grey39; 
                Highlight = Color.Cyan1; 
                Border = Color.SlateBlue3;
                SecondaryName = "slateBlue1"; 
                AccentName = "cyan1"; 
                HighlightName = "cyan1";
            }

            public void SetAdminMode()
            {
                Primary = Color.White; 
                Secondary = Color.Orange1; 
                Accent = Color.Red1;
                Muted = Color.Grey39; 
                Highlight = Color.Orange1; 
                Border = Color.Red3;
                SecondaryName = "orange1"; 
                AccentName = "red1"; 
                HighlightName = "orange1";
            }
        }

        #endregion
    }
}
