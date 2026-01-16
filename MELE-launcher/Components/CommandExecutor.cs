using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
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
        // Changed to Func to return string output instead of void
        private readonly Dictionary<string, Func<string[], CommandDefinition, string>> _builtInHandlers;
        
        // Callbacks for built-in actions
        private Action _onRescan;
        private Action _onExit;
        private Action _onSettings;
        private Func<List<DetectedGame>> _getDetectedGames;
        private Action<DetectedGame, LaunchOptions> _onLaunchGame;
        private Func<LauncherConfig> _getConfig;

        // Visual Theme (Matches MenuSystem)
        private readonly Theme _theme = new Theme();

        public CommandExecutor()
        {
            _registry = new CommandRegistry();
            _builtInHandlers = new Dictionary<string, Func<string[], CommandDefinition, string>>();
            
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
        /// <returns>Result message to display in the terminal log.</returns>
        public string Execute(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return null; 
            }

            // Parse command and arguments
            var parts = ParseCommandLine(commandLine);
            if (parts.Length == 0)
            {
                return null;
            }

            var commandName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            // Find matching command
            var command = FindCommand(commandName);
            
            if (command == null)
            {
                return $"[red]ERR:[/] Unknown directive '{commandName}'. Type [cyan]help[/] for protocol list.";
            }

            if (!command.Enabled)
            {
                return $"[orange1]WARN:[/] Module '{commandName}' is offline.";
            }

            // Execute the command
            try
            {
                if (!string.IsNullOrEmpty(command.ActionType) && _builtInHandlers.ContainsKey(command.ActionType))
                {
                    return _builtInHandlers[command.ActionType](args, command);
                }
                else
                {
                    return $"[red]ERR:[/] Command '{commandName}' has no handler definition.";
                }
            }
            catch (Exception ex)
            {
                return $"[red]CRITICAL FAULT:[/] {ex.Message}";
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
                    return GetCommandHelpText(args[0]);
                }
                else
                {
                    return GetAllCommandsText();
                }
            };

            // List games
            _builtInHandlers["list_games"] = (args, cmd) =>
            {
                var games = _getDetectedGames?.Invoke() ?? new List<DetectedGame>();
                
                if (games.Count == 0)
                {
                    return "[yellow]No Mass Effect modules detected in local file system.[/]";
                }
                
                var sb = new StringBuilder();
                sb.AppendLine($"[bold {_theme.HighlightName}]INSTALLED MODULES:[/]");
                
                int index = 1;
                foreach (var game in games.OrderBy(g => g.Edition).ThenBy(g => g.Type))
                {
                    var status = game.IsValid ? "[green]READY[/]" : "[red]MISSING[/]";
                    sb.AppendLine($"  [cyan]{index}[/] [bold white]{game.Name}[/] - {status}");
                    index++;
                }
                
                sb.Append($"\n[dim]Use [white]launch <id>[/] to initialize specific module.[/]");
                return sb.ToString();
            };

            // Rescan
            _builtInHandlers["rescan"] = (args, cmd) =>
            {
                _onRescan?.Invoke();
                return "[green]Detection sensors refreshed.[/]";
            };

            // Settings
            _builtInHandlers["settings"] = (args, cmd) =>
            {
                _onSettings?.Invoke();
                // Return null so terminal closes and settings menu takes over
                return null;
            };

            // Exit
            _builtInHandlers["exit"] = (args, cmd) =>
            {
                _onExit?.Invoke();
                return "[red]Terminating session...[/]";
            };

            // Clear screen
            _builtInHandlers["clear"] = (args, cmd) =>
            {
                return "[CLEAR]"; // Special token handled by MenuSystem
            };

            // Version info
            _builtInHandlers["version"] = (args, cmd) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("[bold cyan]MASS EFFECT LEGENDARY LAUNCHER[/]");
                sb.AppendLine($"[dim]Version 2.1.0 RC • .NET 6.0 • {Environment.OSVersion}[/]");
                return sb.ToString();
            };

            // Launch game
            _builtInHandlers["launch_game"] = (args, cmd) =>
            {
                if (args.Length == 0)
                {
                    return $"[{_theme.Alert}]ERROR:[/] Target designation required.\n[dim]Usage: launch <1|2|3>[/]";
                }

                if (!int.TryParse(args[0], out int gameNum) || gameNum < 1 || gameNum > 3)
                {
                    return $"[{_theme.Alert}]ERROR:[/] Invalid target designation. Use 1, 2, or 3.";
                }

                // Find the game
                var games = _getDetectedGames?.Invoke() ?? new List<DetectedGame>();
                var targetType = (GameType)(gameNum - 1);
                var game = games.FirstOrDefault(g => g.Type == targetType && g.Edition == GameEdition.Legendary);

                if (game == null)
                {
                    return $"[{_theme.Alert}]ERROR:[/] Module ME{gameNum} not found. Run [white]scan[/] to update database.";
                }

                if (!game.IsValid)
                {
                    return $"[{_theme.Alert}]CRITICAL:[/] Integrity check failed for ME{gameNum}.";
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

                // Trigger launch callback
                _onLaunchGame?.Invoke(game, launchOptions);
                
                // Return a nice log message
                var textLang = LocaleMapper.GetLanguageOption(launchOptions.Locale).DisplayName;
                return $"[green]SUCCESS:[/] Initiating [bold white]{game.Name}[/]\n[dim]Profile: {textLang}[/]";
            };
        }

        private string GetAllCommandsText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[bold {_theme.AccentName}]SYSTEM COMMAND REGISTRY[/]");
            
            var commands = GetAvailableCommands();
            var grouped = commands.GroupBy(c => c.Category ?? "General");

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                sb.AppendLine();
                sb.AppendLine($"[bold underline]{group.Key.ToUpper()}[/]");
                
                foreach (var cmd in group)
                {
                    var aliases = cmd.Aliases.Count > 0 ? $" ({string.Join(",", cmd.Aliases)})" : "";
                    sb.AppendLine($"  [cyan]{cmd.Name}[/][dim]{aliases}[/] - {cmd.Description}");
                }
            }

            return sb.ToString();
        }

        private string GetCommandHelpText(string commandName)
        {
            var command = FindCommand(commandName);
            
            if (command == null)
            {
                return $"[{_theme.Alert}]ERROR:[/] Unknown directive '{commandName}'";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[bold cyan]{command.Name.ToUpper()}[/] - {command.Description}");
            
            if (!string.IsNullOrEmpty(command.Usage))
                sb.AppendLine($"[dim]Usage:[/]  [white]{command.Usage}[/]");
            
            if (command.Aliases.Count > 0)
                sb.AppendLine($"[dim]Aliases:[/] {string.Join(", ", command.Aliases)}");
            
            if (command.RequiresAdmin)
                sb.AppendLine($"[dim]Access:[/]  [{_theme.Alert}]ADMINISTRATOR[/]");

            return sb.ToString();
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
            var parts = new List<string>();
            var current = "";
            var inQuotes = false;

            foreach (var ch in commandLine)
            {
                if (ch == '"') inQuotes = !inQuotes;
                else if (ch == ' ' && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        parts.Add(current);
                        current = "";
                    }
                }
                else current += ch;
            }

            if (!string.IsNullOrEmpty(current)) parts.Add(current);
            return parts.ToArray();
        }

        private void LoadCommandDefinitions()
        {
            LoadBuiltInCommands();

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
                        foreach (var customCmd in customRegistry.Commands)
                        {
                            var existing = _registry.Commands.FirstOrDefault(c => c.Name == customCmd.Name);
                            if (existing != null) _registry.Commands.Remove(existing);
                            _registry.Commands.Add(customCmd);
                        }
                    }
                }
                catch { /* Ignore errors in custom commands */ }
            }
        }

        private void LoadBuiltInCommands()
        {
            _registry.Commands.AddRange(new[]
            {
                new CommandDefinition { Name = "help", Aliases = new List<string> { "?", "h" }, Description = "Display help protocol", Usage = "help [command]", Category = "System", ActionType = "help" },
                new CommandDefinition { Name = "list", Aliases = new List<string> { "ls", "games" }, Description = "Query installed game modules", Usage = "list", Category = "Game", ActionType = "list_games" },
                new CommandDefinition { Name = "launch", Aliases = new List<string> { "play", "start", "run" }, Description = "Initialize specific game module (1-3)", Usage = "launch <1|2|3>", Category = "Game", ActionType = "launch_game" },
                new CommandDefinition { Name = "scan", Aliases = new List<string> { "rescan", "refresh" }, Description = "Refresh detection sensors", Usage = "scan", Category = "Game", ActionType = "rescan" },
                new CommandDefinition { Name = "settings", Aliases = new List<string> { "config", "cfg" }, Description = "Modify system parameters", Usage = "settings", Category = "System", ActionType = "settings" },
                new CommandDefinition { Name = "exit", Aliases = new List<string> { "quit", "q" }, Description = "Terminate session", Usage = "exit", Category = "System", ActionType = "exit" },
                new CommandDefinition { Name = "clear", Aliases = new List<string> { "cls" }, Description = "Purge terminal display", Usage = "clear", Category = "System", ActionType = "clear" },
                new CommandDefinition { Name = "version", Aliases = new List<string> { "ver", "about" }, Description = "System integrity check", Usage = "version", Category = "System", ActionType = "version" }
            });
        }

        private class Theme
        {
            public Color Primary = Color.White;
            public Color Secondary = Color.SlateBlue1;
            public Color Accent = Color.Cyan1;
            public Color Muted = Color.Grey39;
            public Color Highlight = Color.Cyan1;
            public Color Alert = Color.Orange1;
            
            public string SecondaryName = "slateBlue1";
            public string AccentName = "cyan1";
            public string HighlightName = "cyan1";
        }
    }
}