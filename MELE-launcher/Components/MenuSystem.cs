using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MassEffectLauncher.Models;
using MELE_launcher.Models;
using MELE_launcher.Configuration;
using MELE_launcher.Components;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MassEffectLauncher.Components
{
    public class MenuSystem
    {
        #region Core State
        private int _selectedIndex;
        private List<MenuItem> _menuItems;
        private MenuState _currentState;
        
        // Input System State
        private bool _isInputMode;
        private StringBuilder _inputBuffer;
        private string _inputPrompt;
        private Action<string> _onInputComplete;
        
        // Command System
        private CommandExecutor _commandExecutor;
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

        #region Public Properties
        
        /// <summary>
        /// Gets whether the menu system is currently in input mode.
        /// </summary>
        public bool IsInInputMode => _isInputMode;
        
        #endregion

        public MenuSystem()
        {
            _selectedIndex = 0;
            _menuItems = new List<MenuItem>();
            _currentState = MenuState.Main;
            _isAdmin = false;
            
            // Input System Init
            _isInputMode = false;
            _inputBuffer = new StringBuilder();
            
            // Command System Init
            _commandExecutor = new CommandExecutor();
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
        /// Registers command system callbacks for built-in commands.
        /// </summary>
        public void RegisterCommandCallbacks(
            Action onRescan,
            Action onExit,
            Action onSettings,
            Func<List<DetectedGame>> getDetectedGames,
            Action<DetectedGame, LaunchOptions> onLaunchGame,
            Func<LauncherConfig> getConfig)
        {
            _commandExecutor.RegisterCallbacks(onRescan, onExit, onSettings, getDetectedGames, onLaunchGame, getConfig);
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
            // 1. INPUT MODE (Typing in the bottom bar)
            if (_isInputMode)
            {
                HandleTextInput(key);
                return;
            }

            // 2. NAVIGATION MODE
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
                
                // Shortcut: Ctrl+F to Search/Command (Extensibility)
                case ConsoleKey.F: 
                    if ((key.Modifiers & ConsoleModifiers.Control) != 0) 
                        ActivateInputMode("COMMAND >", (cmd) => ExecuteCommand(cmd));
                    break;
            }
        }

        private void HandleTextInput(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Enter)
            {
                _isInputMode = false;
                string result = _inputBuffer.ToString();
                _inputBuffer.Clear();
                _onInputComplete?.Invoke(result);
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                _isInputMode = false;
                _inputBuffer.Clear();
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (_inputBuffer.Length > 0) 
                    _inputBuffer.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _inputBuffer.Append(key.KeyChar);
            }
        }

        private void ActivateInputMode(string prompt, Action<string> onComplete)
        {
            _isInputMode = true;
            _inputPrompt = prompt;
            _onInputComplete = onComplete;
            _inputBuffer.Clear();
        }

        /// <summary>
        /// Executes a command entered in command mode.
        /// </summary>
        private void ExecuteCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return; // Empty command, just return to normal mode
            }

            var result = _commandExecutor.Execute(commandLine);
            
            // If there's a result message, show it
            if (!string.IsNullOrEmpty(result))
            {
                ShowMessage(result, MessageType.Info);
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
            try
            {
                if (_isInputMode)
                {
                    // Command Mode: Show only header and centered input box
                    var rootLayout = new Layout("Root")
                        .SplitRows(
                            new Layout("Header").Size(4),
                            new Layout("CommandArea")  // Full height for centered input
                        );

                    rootLayout["Header"].Update(RenderHeader());
                    rootLayout["CommandArea"].Update(RenderCenteredCommandInput());

                    AnsiConsole.Clear();
                    AnsiConsole.Write(rootLayout);
                }
                else
                {
                    // Normal Mode: Three-section layout
                    var rootLayout = new Layout("Root")
                        .SplitRows(
                            new Layout("Header").Size(4),
                            new Layout("Body"),
                            new Layout("Input").Size(3)  // Fixed bottom bar
                        );

                    rootLayout["Header"].Update(RenderHeader());
                    rootLayout["Body"].Update(RenderDashboardBody());
                    rootLayout["Input"].Update(RenderInputArea());

                    AnsiConsole.Clear();
                    AnsiConsole.Write(rootLayout);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                AnsiConsole.WriteLine($"Render error: {ex.Message}");
                AnsiConsole.WriteLine($"Stack: {ex.StackTrace}");
                Console.ReadKey();
                throw;
            }
        }

        private IRenderable RenderHeader()
        {
            // Simple, strong branding
            var grid = new Grid();
            grid.AddColumn(new GridColumn().NoWrap());
            grid.AddColumn(new GridColumn().RightAligned());

            var textTitle = new Markup($"[bold {_theme.AccentName}]MASS EFFECT[/] [white]LAUNCHER[/] [dim]v2.1[/]");
            var userInfo = new Markup($"[dim]{Environment.UserName}@{Environment.MachineName}[/]");

            grid.AddRow(textTitle, userInfo);
            grid.AddRow(new Markup($"[{_theme.SecondaryName}]SYSTEMS ALLIANCE NETWORK[/]"), new Text(""));

            return new Panel(grid) 
            { 
                Border = BoxBorder.None, 
                Padding = new Padding(1, 0, 1, 0) 
            };
        }

        private IRenderable RenderDashboardBody()
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().Width(35)); // Navigation Pane
            grid.AddColumn(new GridColumn().Padding(2, 0, 0, 0)); // Content Pane

            // Left Pane: Navigation
            grid.AddRow(RenderNavigationPane(), RenderContextPane());

            return grid;
        }

        private IRenderable RenderNavigationPane()
        {
            var table = new Table().NoBorder().HideHeaders().Expand();
            table.AddColumn("Indicator");
            table.AddColumn("Text");

            for (int i = 0; i < _menuItems.Count; i++)
            {
                var item = _menuItems[i];
                
                if (item.Type == MenuItemType.Separator)
                {
                    table.AddRow(new Text(""), new Rule { Style = new Style(_theme.Muted) });
                    continue;
                }

                bool isSelected = (i == _selectedIndex) && !_isInputMode;
                
                // Escape markup in titles to prevent parsing errors
                var safeTitle = item.Title?.EscapeMarkup() ?? "";
                
                if (isSelected)
                {
                    // Active block style
                    table.AddRow(
                        new Markup($"[{_theme.HighlightName}]▌[/]"), 
                        new Markup($"[black on {_theme.HighlightName}] {safeTitle} [/]")
                    );
                }
                else
                {
                    string style = item.IsEnabled ? "white" : "grey";
                    table.AddRow(
                        new Markup(" "), 
                        new Markup($"[{style}]{safeTitle}[/]")
                    );
                }
            }

            return new Panel(table)
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(_theme.Border),
                Header = new PanelHeader($" [bold]{GetMenuTitle()}[/] "),
                Padding = new Padding(1, 1, 1, 1),
                Expand = true
            };
        }

        private string GetMenuTitle() => _currentState switch
        {
            MenuState.GameOptions => "CONFIGURATION",
            MenuState.Settings => "SYSTEM SETTINGS",
            _ => "LIBRARY"
        };

        private IRenderable RenderContextPane()
        {
            var selected = GetSelectedItem();
            if (selected == null) return new Panel(new Text("No selection"));

            var content = new List<IRenderable>();

            try
            {
                // 1. ASCII Art or Big Header (Contextual)
                var headerText = GetShortCode(selected);
                content.Add(new FigletText(headerText).Color(_theme.Muted));

                // 2. Info Block - Escape both title and description
                var safeTitle = selected.Title?.EscapeMarkup() ?? "";
                var rule = new Rule($"[{_theme.SecondaryName}]{safeTitle}[/]");
                rule.Justification = Justify.Left;
                content.Add(rule);
                content.Add(new Text(""));
                
                // Escape markup in description to prevent parsing errors
                var safeDescription = selected.Description?.EscapeMarkup() ?? "";
                content.Add(new Markup($"[white]{safeDescription}[/]\n"));

                // 3. Dynamic Details via Tags
                if (selected.Tag is DetectedGame game) 
                    RenderGameDetails(game, content);
                else if (selected.Type == MenuItemType.Setting) 
                    RenderSettingDetails(selected, content);
                else 
                    RenderDefaultDetails(content);
            }
            catch (Exception ex)
            {
                content.Clear();
                content.Add(new Text($"Error rendering context: {ex.Message}"));
            }

            return new Panel(new Rows(content))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(_theme.Border),
                Header = new PanelHeader(" [bold]DETAILS[/] "),
                Padding = new Padding(2, 1, 2, 1),
                Expand = true
            };
        }

        // The "Command Bar" - Switches between Status bar and Input Field
        private IRenderable RenderInputArea()
        {
            // In input mode, this is not used (we show centered input instead)
            if (_isInputMode)
            {
                return new Panel(new Text("")) { Border = BoxBorder.None };
            }
            
            // Status State
            var grid = new Grid().AddColumn().AddColumn(new GridColumn().RightAligned());
            
            string modeText = _isAdmin ? "ADMIN" : "USER";
            string modeColor = _isAdmin ? "red" : "orange1";
            grid.AddRow(
                new Markup($" [{modeColor}]{modeText}[/] [dim]Ready[/]"), 
                new Markup("[dim]↑/↓[/] [cyan]Nav[/] [dim]HOME/END[/] [cyan]Jump[/] [dim]Ent[/] [cyan]Sel[/] [dim]Esc[/] [cyan]Back[/] [dim]Ctrl+F[/] [cyan]Cmd[/]")
            );

            return new Panel(grid) 
            { 
                Border = BoxBorder.Heavy, 
                BorderStyle = new Style(_theme.Muted), 
                Height = 3 
            };
        }

       /// <summary>
/// Renders a centered command input box for command mode.
/// </summary>
private IRenderable RenderCenteredCommandInput()
{
    // 1. Construct the Input Display Line
    // We check if the buffer is empty to show a 'ghost' hint if needed, 
    // or just the cursor.
    string inputLine;
    if (_inputBuffer.Length == 0)
    {
        inputLine = $"[bold {_theme.HighlightName}]{_inputPrompt}[/] [dim italic]Type here...[/][blink {_theme.AccentName}]_[/]";
    }
    else
    {
        // Escape markup in user input to prevent crashing if they type "[red]" etc.
        string safeBuffer = _inputBuffer.ToString().Replace("[", "[[").Replace("]", "]]");
        inputLine = $"[bold {_theme.HighlightName}]{_inputPrompt}[/] [white]{safeBuffer}[/][blink {_theme.AccentName}]_[/]";
    }

    // 2. Build the Content Grid
    var content = new Grid();
    content.AddColumn(new GridColumn().Centered());

    // Icon / Header
    content.AddRow(new Markup($"[bold {_theme.AccentName} underline]TERMINAL INTERFACE[/]"));
    content.AddRow(new Text("")); // Spacer

    // The Input Line (Big and clear)
    content.AddRow(new Markup(inputLine));
    
    content.AddRow(new Text("")); // Spacer
    
    // Instructions / Separator
    content.AddRow(new Rule { Style = new Style(_theme.Muted) });
    content.AddRow(new Markup("[dim]Press [/][cyan]ENTER[/][dim] to execute • [/][orange1]ESC[/][dim] to cancel[/]"));

    // 3. Wrap in a High-Tech Panel
    var panel = new Panel(content)
    {
        Border = BoxBorder.Rounded, // Rounded looks more modern/sci-fi
        BorderStyle = new Style(_theme.Highlight), // Glowing border color
        Padding = new Padding(4, 1, 4, 1),
        Width = 70 
    };

    // 4. Use Align.Center to perfectly center it in the available space
    // This removes the need for magic number padding
    return Align.Center(panel, VerticalAlignment.Middle);
}

        #endregion

        #region UI Component Helpers

        private void RenderGameDetails(DetectedGame game, List<IRenderable> content)
        {
            content.Add(new Text(""));
            
            var table = new Table().NoBorder().HideHeaders();
            table.AddColumn("Key");
            table.AddColumn("Value");

            string statusColor = game.IsValid ? "green" : "red";
            string statusText = game.IsValid ? "INSTALLED" : "MISSING";
            
            table.AddRow(new Markup("[dim]STATUS[/]"), new Markup($"[{statusColor}]{statusText}[/]"));
            table.AddRow(new Markup("[dim]PATH[/]"), new Text(Truncate(game.Path, 35)));

            // Config Lookup for live preview
            var gameConfig = _config?.Games.FirstOrDefault(g => g.Type == game.Type && g.Edition == game.Edition);
            
            if (gameConfig != null)
            {
                var textLang = LocaleMapper.GetLanguageOption(gameConfig.Locale).DisplayName;
                var voiceLang = LocaleMapper.GetLanguageOption(gameConfig.VoiceLanguage).DisplayName;
                var hasNativeVO = LocaleMapper.HasNativeVoiceOver(gameConfig.VoiceLanguage, game.Type);
                var voInfo = hasNativeVO ? "[green]Native[/]" : "[yellow]Dubbed[/]";
                
                table.AddRow(new Markup("[dim]LOCALE[/]"), new Markup($"[cyan]{textLang}[/]"));
                table.AddRow(new Markup("[dim]AUDIO[/]"), new Markup($"[cyan]{voiceLang}[/] ({voInfo})"));
            }

            content.Add(table);
        }

        private void RenderSettingDetails(MenuItem item, List<IRenderable> content)
        {
            content.Add(new Text(""));
            content.Add(new Markup($"[dim]Current value configured in [/][cyan]config.json[/]"));
        }

        private void RenderDefaultDetails(List<IRenderable> content)
        {
            content.Add(new Text(""));
            content.Add(new Markup("[dim]Select an option to proceed.[/]"));
        }

        private string Truncate(string path, int max)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.Length > max) return "..." + path.Substring(path.Length - (max - 3));
            return path;
        }

        private string GetShortCode(MenuItem item)
        {
            if (item.Tag is DetectedGame dg) return $"ME {(int)dg.Type + 1}";
            
            var title = item.Title ?? "";
            if (title.Contains("Settings") || title.Contains("CFG")) return "CFG";
            if (title.Contains("Exit") || title.Contains("BYE")) return "BYE";
            return "CMD";
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
                MessageType.Info => Color.Cyan1,
                _ => Color.Cyan1
            };

            AnsiConsole.Clear();
            
            var panel = new Panel(new Markup($"[{color}]{message}[/]"))
            {
                Border = BoxBorder.Heavy,
                BorderStyle = new Style(color),
                Header = new PanelHeader($"[bold {color}] {type.ToString().ToUpper()} [/]"),
                Width = 60
            };

            AnsiConsole.Write(new Padder(panel, new Padding(0, 5)));
            AnsiConsole.MarkupLine("[dim]Press any key...[/]");
            Console.ReadKey(true);
        }

        public bool ShowConfirmation(string message)
        {
            // Centered modal dialog
            AnsiConsole.Clear();
            
            var content = new Grid();
            content.AddColumn(new GridColumn().Centered());
            content.AddRow(new Markup($"[bold white]{message}[/]"));
            content.AddRow(new Text("")); // spacer
            content.AddRow(new Markup("[dim]Use Arrow Keys to Choose[/]"));

            var panel = new Panel(content)
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Orange1),
                Header = new PanelHeader("[bold orange1] CONFIRMATION [/]").Centered(),
                Padding = new Padding(2, 1, 2, 1),
                Width = 60
            };

            // Render box centered vertically/horizontally
            AnsiConsole.Write(new Padder(panel, new Padding(0, 5)));

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices("Yes", "No")
                    .HighlightStyle(new Style(Color.Black, Color.Orange1)));

            return selection == "Yes";
        }

        public void ShowProgress(string task, Action action)
        {
            // Using Spectre's Progress for the bar
            AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(new ProgressColumn[] 
                {
                    new TaskDescriptionColumn(),    // Task name
                    new ProgressBarColumn(),        // The [...] bar
                    new PercentageColumn(),         // 50%
                    new SpinnerColumn(Spinner.Known.Dots) // Spinner
                })
                .Start(ctx => 
                {
                    var pTask = ctx.AddTask($"[cyan]{task}[/]");
                    pTask.IsIndeterminate = true;
                    action();
                    pTask.StopTask();
                });
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
            return new List<MenuItem>
            {
                new MenuItem 
                { 
                    Title = $"Locale: {currentLocale}", 
                    Description = "Default Text Language", 
                    Type = MenuItemType.Setting, 
                    OnSelect = () => ShowLocaleSelection(currentLocale) 
                },
                new MenuItem 
                { 
                    Title = $"Rumble: {(currentForceFeedback ? "ON" : "OFF")}", 
                    Description = "Default Controller Feedback", 
                    Type = MenuItemType.Setting, 
                    OnSelect = () => ToggleForceFeedback(currentForceFeedback) 
                },
                new MenuItem 
                { 
                    Title = $"No Intro: {(currentSkipIntro ? "ON" : "OFF")}", 
                    Description = "Skip Startup Logos", 
                    Type = MenuItemType.Setting, 
                    OnSelect = () => ToggleSkipIntro(currentSkipIntro) 
                },
                new MenuItem 
                { 
                    Type = MenuItemType.Separator 
                },
                new MenuItem 
                { 
                    Title = "Rescan Library", 
                    Description = "Auto-detect games", 
                    Type = MenuItemType.Action, 
                    OnSelect = () => TriggerRescan() 
                },
                new MenuItem 
                { 
                    Title = "Add Manual Path", 
                    Description = "Type path in console", 
                    Type = MenuItemType.Action, 
                    OnSelect = () => ShowManualPathInput() 
                },
                new MenuItem 
                { 
                    Type = MenuItemType.Separator 
                },
                new MenuItem 
                { 
                    Title = "Back", 
                    Description = "Return to Main", 
                    Type = MenuItemType.Action, 
                    OnSelect = () => _currentState = MenuState.Main 
                }
            };
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

            // Non-selectable Header
            list.Add(new MenuItem 
            { 
                Title = game.Name, 
                Description = "Configuration", 
                Type = MenuItemType.Separator, 
                Tag = game 
            });

            // Only show locale and force feedback options for Legendary Edition games
            if (game.Edition == GameEdition.Legendary)
            {
                var textLangName = LocaleMapper.GetLanguageOption(loc).DisplayName;
                var voiceLangName = LocaleMapper.GetLanguageOption(vo).DisplayName;
                
                // Rich VO Logic preserved
                var voiceType = LocaleMapper.HasNativeVoiceOver(vo, game.Type) ? "Native" : "English Dub";
                
                list.Add(new MenuItem 
                { 
                    Title = $"Text: {textLangName}", 
                    Description = "Change Subtitles", 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ShowGameTextSelection(game, loc)
                });

                list.Add(new MenuItem 
                { 
                    Title = $"Voice: {voiceLangName}", 
                    Description = $"Audio ({voiceType})", 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ShowGameVoiceSelection(game, vo)
                });

                list.Add(new MenuItem 
                { 
                    Title = $"Rumble: {(ff ? "ON" : "OFF")}", 
                    Description = "Controller Vibration", 
                    Type = MenuItemType.Setting, 
                    Tag = game,
                    IsEnabled = true,
                    OnSelect = () => ToggleGameFF(game, ff)
                });

                list.Add(new MenuItem 
                { 
                    Type = MenuItemType.Separator 
                });
            }

            list.Add(new MenuItem 
            { 
                Title = "INITIATE LAUNCH", 
                Description = "Start Game Process", 
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

            list.Add(new MenuItem 
            { 
                Title = "Back", 
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
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = PromptSelection("Global Text Language", choices);

            var lang = LocaleMapper.AvailableLanguages
                .FirstOrDefault(l => l.GetDisplayString() == selectedDisplay);

            if (lang != null) 
            {
                _onLocaleChanged?.Invoke(lang.Code);
                ShowMessage($"Locale set to {lang.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Toggles force feedback setting and triggers callback.
        /// </summary>
        private void ToggleForceFeedback(bool current) 
        {
            bool newValue = !current;
            _onForceFeedbackChanged?.Invoke(newValue);
            ShowMessage($"Global Rumble: {(newValue ? "ON" : "OFF")}", MessageType.Success);
        }

        /// <summary>
        /// Toggles skip intro setting and triggers callback.
        /// </summary>
        private void ToggleSkipIntro(bool current)
        {
            bool newValue = !current;
            _onSkipIntroChanged?.Invoke(newValue);
            ShowMessage($"Skip Intro: {(newValue ? "ON" : "OFF")}", MessageType.Success);
        }

        /// <summary>
        /// Triggers the rescan games callback.
        /// </summary>
        private void TriggerRescan() 
        { 
            if (ShowConfirmation("Perform full filesystem scan?")) 
                _onRescanGames?.Invoke(); 
        }

        /// <summary>
        /// Shows manual path input prompt using integrated input mode.
        /// </summary>
        private void ShowManualPathInput() 
        {
            // Instead of clearing screen, we activate the bottom bar
            ActivateInputMode("PATH >", (path) => 
            {
                if (!string.IsNullOrWhiteSpace(path)) 
                {
                    _onManualPathAdded?.Invoke(path);
                }
                else 
                {
                    ShowMessage("Path cannot be empty.", MessageType.Warning);
                }
            });
        }

        /// <summary>
        /// Shows text/subtitle language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameTextSelection(DetectedGame game, string currentLocale)
        {
            var choices = LocaleMapper.AvailableLanguages
                .Select(lang => lang.GetDisplayString())
                .ToList();

            var selectedDisplay = PromptSelection($"Text Language: {game.Name}", choices);

            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.GetDisplayString() == selectedDisplay);

            if (selectedLanguage != null)
            {
                UpdateGameConfig(game, conf => conf.Locale = selectedLanguage.Code);
                ShowMessage($"Text set to {selectedLanguage.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Shows voice-over language selection for a specific game and saves the choice.
        /// </summary>
        private void ShowGameVoiceSelection(DetectedGame game, string currentVoiceLanguage)
        {
            // Complex Choice Building
            var choices = new List<string>();
            
            foreach (var lang in LocaleMapper.AvailableLanguages)
            {
                bool hasNativeVO = LocaleMapper.HasNativeVoiceOver(lang.Code, game.Type);
                string voiceInfo = hasNativeVO ? "Native" : "Dub";
                choices.Add($"{lang.DisplayName} ({voiceInfo})");
            }

            var selectedDisplay = PromptSelection($"Voice Language: {game.Name}", choices);

            // Extract the language name (before the parenthesis)
            var languageName = selectedDisplay.Split('(')[0].Trim();
            
            var selectedLanguage = LocaleMapper.AvailableLanguages
                .FirstOrDefault(lang => lang.DisplayName == languageName);

            if (selectedLanguage != null)
            {
                UpdateGameConfig(game, conf => conf.VoiceLanguage = selectedLanguage.Code);
                ShowMessage($"Voice set to {selectedLanguage.DisplayName}", MessageType.Success);
            }
        }

        /// <summary>
        /// Toggles force feedback for a specific game and saves the choice.
        /// </summary>
        private void ToggleGameFF(DetectedGame game, bool currentValue)
        {
            bool newValue = !currentValue;
            UpdateGameConfig(game, conf => conf.ForceFeedback = newValue);
            ShowMessage($"Rumble: {(newValue ? "ON" : "OFF")}", MessageType.Success);
        }

        /// <summary>
        /// Updates or creates game configuration with the specified action.
        /// </summary>
        private void UpdateGameConfig(DetectedGame game, Action<GameConfig> modifier)
        {
            var config = _config.Games.FirstOrDefault(x => x.Type == game.Type && x.Edition == game.Edition);
            
            if (config == null) 
            {
                // Create new config if missing
                config = new GameConfig 
                { 
                    Type = game.Type, 
                    Edition = game.Edition, 
                    Path = game.Path, 
                    Locale = "INT", 
                    VoiceLanguage = "INT" 
                };
                _config.Games.Add(config);
            }

            modifier(config);
            _configManager.Save(_config);
        }

        /// <summary>
        /// Helper method for showing selection prompts with consistent styling.
        /// </summary>
        private string PromptSelection(string title, List<string> choices)
        {
            AnsiConsole.Clear();
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[cyan]{title}[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(Color.Black, Color.Cyan1)));
        }

        // Legacy method names for backward compatibility
        private void ShowGameTextLanguageSelection(DetectedGame game, string currentLocale) 
            => ShowGameTextSelection(game, currentLocale);
        
        private void ShowGameVoiceLanguageSelection(DetectedGame game, string currentVoiceLanguage) 
            => ShowGameVoiceSelection(game, currentVoiceLanguage);
        
        private void ToggleGameForceFeedback(DetectedGame game, bool currentValue) 
            => ToggleGameFF(game, currentValue);

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
            if (game == null) 
            { 
                ShowMessage("Cannot launch: Game is null.", MessageType.Error); 
                return; 
            }
            
            if (admin == null) 
            { 
                ShowMessage("Cannot launch: Admin elevator is not initialized.", MessageType.Error); 
                return; 
            }
            
            if (launcher == null) 
            { 
                ShowMessage("Cannot launch: Game launcher is not initialized.", MessageType.Error); 
                return; 
            }

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
            
            var panel = new Panel(new Markup(
                $"[cyan bold]INITIALIZING LAUNCH SEQUENCE[/]\n\n" +
                $"Target: {game.Name}\n" +
                $"Mode:   {textLang}/{voiceLang} ({voiceType} VO)")) 
            { 
                Border = BoxBorder.Heavy, 
                BorderStyle = new Style(Color.Cyan1),
                Padding = new Padding(2)
            };

            AnsiConsole.Write(new Padder(panel, new Padding(0, 2)));

            LaunchResult result = null;
            ShowProgress("Executing...", () => { result = launcher.Launch(game, options); });

            if (result != null && result.Success)
            {
                ShowMessage($"Game Process Started.\n{game.Name} launched successfully!", MessageType.Success);
            }
            else
            {
                string errorMsg = result?.ErrorMessage ?? "Unknown error occurred.";
                ShowMessage($"Launch Failed:\n{errorMsg}", MessageType.Error);
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
