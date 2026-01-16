using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MELE_launcher.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Executes commands entered in command mode.
    /// </summary>
    public class CommandExecutor
    {
        private readonly CommandRegistry _registry;
        private readonly Dictionary<string, Action<string[], CommandDefinition>> _builtInHandlers;
        
        // Callbacks for built-in actions
        private Action _onRescan;
        private Action _onExit;
        private Action _onSettings;
        private Func<List<DetectedGame>> _getDetectedGames;
        private Action<DetectedGame, LaunchOptions> _onLaunchGame;
        private Func<LauncherConfig> _getConfig;

        public CommandExecutor()
        {
            _registry = new CommandRegistry();
            _builtInHandlers = new Dictionary<string, Action<string[], CommandDefinition>>();
            
            RegisterBuiltInHandlers();
            LoadCommandDefinitions();
        }

        /// <summary>
        /// Registers callbacks for built-in command actions.
        /// </summary>
        public void RegisterCallbacks(
            Action onRescan, 
            Action onExit, 
            Action onSettings,
            Func<List<DetectedGame>> getDetectedGames,
            Action<DetectedGame, LaunchOptions> onLaunchGame,
            Func<LauncherConfig> getConfig)
        {
            _onRescan = onRescan;
            _onExit = onExit;
            _onSettings = onSettings;
            _getDetectedGames = getDetectedGames;
            _onLaunchGame = onLaunchGame;
            _getConfig = getConfig;
        }

        /// <summary>
        /// Executes a command string.
        /// </summary>
        /// <param name="commandLine">The full command line entered by the user.</param>
        /// <returns>Result message to display to the user.</returns>
        public string Execute(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return "No command entered.";
            }

            // Parse command and arguments
            var parts = ParseCommandLine(commandLine);
            if (parts.Length == 0)
            {
                return "Invalid command.";
            }

            var commandName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            // Find matching command
            var command = FindCommand(commandName);
            
            if (command == null)
            {
                return $"Unknown command: '{commandName}'. Type 'help' for available commands.";
            }

            if (!command.Enabled)
            {
                return $"Command '{commandName}' is currently disabled.";
            }

            // Execute the command
            try
            {
                if (!string.IsNullOrEmpty(command.ActionType) && _builtInHandlers.ContainsKey(command.ActionType))
                {
                    _builtInHandlers[command.ActionType](args, command);
                    return null; // Built-in handlers manage their own output
                }
                else
                {
                    return $"Command '{commandName}' has no handler defined.";
                }
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets all available commands for help display.
        /// </summary>
        public List<CommandDefinition> GetAvailableCommands()
        {
            return _registry.Commands.Where(c => c.Enabled).OrderBy(c => c.Category).ThenBy(c => c.Name).ToList();
        }

        private void RegisterBuiltInHandlers()
        {
            // Help command
            _builtInHandlers["help"] = (args, cmd) =>
            {
                if (args.Length > 0)
                {
                    ShowCommandHelp(args[0]);
                }
                else
                {
                    ShowAllCommands();
                }
            };

            // List games
            _builtInHandlers["list_games"] = (args, cmd) =>
            {
                var games = _getDetectedGames?.Invoke() ?? new List<DetectedGame>();
                
                if (games.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No games detected.[/]");
                }
                else
                {
                    var table = new Table();
                    table.AddColumn("Game");
                    table.AddColumn("Edition");
                    table.AddColumn("Status");
                    
                    foreach (var game in games.OrderBy(g => g.Edition).ThenBy(g => g.Type))
                    {
                        var status = game.IsValid ? "[green]✓ Installed[/]" : "[red]✗ Missing[/]";
                        table.AddRow(game.Name, game.Edition.ToString(), status);
                    }
                    
                    AnsiConsole.Write(table);
                }
                
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
            };

            // Rescan
            _builtInHandlers["rescan"] = (args, cmd) =>
            {
                _onRescan?.Invoke();
            };

            // Settings
            _builtInHandlers["settings"] = (args, cmd) =>
            {
                _onSettings?.Invoke();
            };

            // Exit
            _builtInHandlers["exit"] = (args, cmd) =>
            {
                _onExit?.Invoke();
            };

            // Clear screen
            _builtInHandlers["clear"] = (args, cmd) =>
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[green]Screen cleared.[/]");
                System.Threading.Thread.Sleep(500);
            };

            // Version info
            _builtInHandlers["version"] = (args, cmd) =>
            {
                AnsiConsole.Clear();
                var panel = new Panel(new Markup(
                    "[bold cyan]Mass Effect Legendary Launcher[/]\n" +
                    "[dim]Version:[/] 2.1.0\n" +
                    "[dim]Command System:[/] v1.0\n" +
                    "[dim]Platform:[/] .NET 6.0"))
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Cyan1),
                    Header = new PanelHeader(" [bold]VERSION INFO[/] ")
                };
                
                AnsiConsole.Write(panel);
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
            };

            // Launch game
            _builtInHandlers["launch_game"] = (args, cmd) =>
            {
                if (args.Length == 0)
                {
                    AnsiConsole.MarkupLine("[red]Error: Game number required (1, 2, or 3)[/]");
                    AnsiConsole.MarkupLine("[dim]Usage: launch <1|2|3>[/]");
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                }

                if (!int.TryParse(args[0], out int gameNum) || gameNum < 1 || gameNum > 3)
                {
                    AnsiConsole.MarkupLine("[red]Invalid game number. Use 1, 2, or 3.[/]");
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                }

                // Find the game
                var games = _getDetectedGames?.Invoke() ?? new List<DetectedGame>();
                var targetType = (GameType)(gameNum - 1);
                var game = games.FirstOrDefault(g => g.Type == targetType && g.Edition == GameEdition.Legendary);

                if (game == null)
                {
                    AnsiConsole.MarkupLine($"[red]Mass Effect {gameNum} Legendary Edition not found.[/]");
                    AnsiConsole.MarkupLine("[yellow]Run 'scan' to search for games.[/]");
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                }

                if (!game.IsValid)
                {
                    AnsiConsole.MarkupLine($"[red]Mass Effect {gameNum} installation is invalid.[/]");
                    AnsiConsole.MarkupLine($"[dim]Path: {game.Path}[/]");
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                }

                // Get configuration for the game
                var config = _getConfig?.Invoke();
                var gameConfig = config?.Games.FirstOrDefault(g => g.Type == game.Type && g.Edition == game.Edition);

                // Build launch options
                var launchOptions = new LaunchOptions
                {
                    Locale = gameConfig?.Locale ?? config?.DefaultLocale ?? "INT",
                    VoiceLanguage = gameConfig?.VoiceLanguage ?? config?.DefaultVoiceLanguage ?? "INT",
                    ForceFeedback = gameConfig?.ForceFeedback ?? config?.DefaultForceFeedback ?? false,
                    PlayIntro = !(config?.DefaultSkipIntro ?? true),
                    Silent = false
                };

                // Show launch info
                AnsiConsole.Clear();
                var textLang = LocaleMapper.GetLanguageOption(launchOptions.Locale).DisplayName;
                var voiceLang = LocaleMapper.GetLanguageOption(launchOptions.VoiceLanguage).DisplayName;
                var hasNativeVO = LocaleMapper.HasNativeVoiceOver(launchOptions.VoiceLanguage, game.Type);
                var voiceType = hasNativeVO ? "Native" : "English";

                var launchPanel = new Panel(new Markup(
                    $"[cyan bold]LAUNCHING GAME[/]\n\n" +
                    $"[white]Game:[/] {game.Name}\n" +
                    $"[white]Text:[/] {textLang}\n" +
                    $"[white]Voice:[/] {voiceLang} ({voiceType} VO)\n" +
                    $"[white]Rumble:[/] {(launchOptions.ForceFeedback ? "ON" : "OFF")}"))
                {
                    Border = BoxBorder.Heavy,
                    BorderStyle = new Style(Color.Cyan1),
                    Header = new PanelHeader(" [bold]COMMAND LAUNCH[/] ")
                };

                AnsiConsole.Write(launchPanel);
                AnsiConsole.WriteLine();

                // Launch the game
                _onLaunchGame?.Invoke(game, launchOptions);
            };
        }

        private void ShowAllCommands()
        {
            AnsiConsole.Clear();
            
            var commands = GetAvailableCommands();
            var grouped = commands.GroupBy(c => c.Category ?? "General");

            AnsiConsole.MarkupLine("[bold cyan]Available Commands[/]\n");

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                AnsiConsole.MarkupLine($"[bold yellow]{group.Key}[/]");
                
                var table = new Table().NoBorder().HideHeaders();
                table.AddColumn("Command");
                table.AddColumn("Description");

                foreach (var cmd in group)
                {
                    var aliases = cmd.Aliases.Count > 0 ? $" ({string.Join(", ", cmd.Aliases)})" : "";
                    table.AddRow(
                        new Markup($"  [cyan]{cmd.Name}{aliases}[/]"),
                        new Text(cmd.Description ?? "")
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine("[dim]Type 'help <command>' for detailed information about a specific command.[/]");
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        private void ShowCommandHelp(string commandName)
        {
            AnsiConsole.Clear();
            
            var command = FindCommand(commandName);
            
            if (command == null)
            {
                AnsiConsole.MarkupLine($"[red]Unknown command: '{commandName}'[/]");
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
                return;
            }

            var content = new List<IRenderable>();
            
            content.Add(new Markup($"[bold cyan]{command.Name}[/]"));
            
            if (command.Aliases.Count > 0)
            {
                content.Add(new Markup($"[dim]Aliases:[/] {string.Join(", ", command.Aliases)}"));
            }
            
            content.Add(new Text(""));
            content.Add(new Markup($"[white]{command.Description ?? "No description available."}[/]"));
            
            if (!string.IsNullOrEmpty(command.Usage))
            {
                content.Add(new Text(""));
                content.Add(new Markup($"[dim]Usage:[/] [cyan]{command.Usage}[/]"));
            }
            
            if (command.RequiresAdmin)
            {
                content.Add(new Text(""));
                content.Add(new Markup("[yellow]⚠ Requires administrator privileges[/]"));
            }

            var panel = new Panel(new Rows(content))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Cyan1),
                Header = new PanelHeader(" [bold]COMMAND HELP[/] "),
                Padding = new Padding(2, 1, 2, 1)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        private CommandDefinition FindCommand(string name)
        {
            name = name.ToLower();
            return _registry.Commands.FirstOrDefault(c => 
                c.Name.ToLower() == name || 
                c.Aliases.Any(a => a.ToLower() == name));
        }

        private string[] ParseCommandLine(string commandLine)
        {
            // Simple parsing - split by spaces, respecting quotes
            var parts = new List<string>();
            var current = "";
            var inQuotes = false;

            foreach (var ch in commandLine)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (ch == ' ' && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += ch;
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current);
            }

            return parts.ToArray();
        }

        private void LoadCommandDefinitions()
        {
            // Load built-in commands
            LoadBuiltInCommands();

            // Try to load custom commands from JSON
            var customCommandsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands.json");
            
            if (File.Exists(customCommandsPath))
            {
                try
                {
                    var json = File.ReadAllText(customCommandsPath);
                    var customRegistry = JsonSerializer.Deserialize<CommandRegistry>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (customRegistry != null && customRegistry.Commands != null)
                    {
                        // Merge custom commands (they can override built-in ones)
                        foreach (var customCmd in customRegistry.Commands)
                        {
                            var existing = _registry.Commands.FirstOrDefault(c => c.Name == customCmd.Name);
                            if (existing != null)
                            {
                                _registry.Commands.Remove(existing);
                            }
                            _registry.Commands.Add(customCmd);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Silently fail - custom commands are optional
                    Console.WriteLine($"Warning: Could not load custom commands: {ex.Message}");
                }
            }
        }

        private void LoadBuiltInCommands()
        {
            _registry.Commands.AddRange(new[]
            {
                new CommandDefinition
                {
                    Name = "help",
                    Aliases = new List<string> { "?", "h" },
                    Description = "Display help information about commands",
                    Usage = "help [command]",
                    Category = "System",
                    ActionType = "help"
                },
                new CommandDefinition
                {
                    Name = "list",
                    Aliases = new List<string> { "ls", "games" },
                    Description = "List all detected Mass Effect games",
                    Usage = "list",
                    Category = "Game",
                    ActionType = "list_games"
                },
                new CommandDefinition
                {
                    Name = "launch",
                    Aliases = new List<string> { "play", "start" },
                    Description = "Quick launch a game by number (1=ME1, 2=ME2, 3=ME3)",
                    Usage = "launch <1|2|3>",
                    Category = "Game",
                    ActionType = "launch_game"
                },
                new CommandDefinition
                {
                    Name = "scan",
                    Aliases = new List<string> { "rescan", "refresh" },
                    Description = "Rescan for Mass Effect game installations",
                    Usage = "scan",
                    Category = "Game",
                    ActionType = "rescan"
                },
                new CommandDefinition
                {
                    Name = "settings",
                    Aliases = new List<string> { "config", "preferences" },
                    Description = "Open settings menu",
                    Usage = "settings",
                    Category = "System",
                    ActionType = "settings"
                },
                new CommandDefinition
                {
                    Name = "exit",
                    Aliases = new List<string> { "quit", "q" },
                    Description = "Exit the launcher",
                    Usage = "exit",
                    Category = "System",
                    ActionType = "exit"
                },
                new CommandDefinition
                {
                    Name = "clear",
                    Aliases = new List<string> { "cls" },
                    Description = "Clear the screen",
                    Usage = "clear",
                    Category = "System",
                    ActionType = "clear"
                },
                new CommandDefinition
                {
                    Name = "version",
                    Aliases = new List<string> { "ver", "about" },
                    Description = "Display version information",
                    Usage = "version",
                    Category = "System",
                    ActionType = "version"
                }
            });
        }
    }
}
