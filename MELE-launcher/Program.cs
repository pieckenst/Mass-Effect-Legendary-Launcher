using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MELE_launcher.Components;
using MELE_launcher.Configuration;
using MELE_launcher.Models;
using MassEffectLauncher.Components;
using MassEffectLauncher.Models;
using Spectre.Console;
using System.Threading.Tasks;

namespace MELE_launcher
{
	class Program
	{
		// P/Invoke declarations for console allocation
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		private static ConfigManager _configManager;
		private static AdminElevator _adminElevator;
		private static GameDetector _gameDetector;
		private static MenuSystem _menuSystem;
		private static GameLauncher _gameLauncher;
		private static List<DetectedGame> _detectedGames;
		private static bool _shouldExit = false;

		static void Main(string[] args)
		{
			// Ensure we have a console window (important for Windows Forms apps)
			if (GetConsoleWindow() == IntPtr.Zero)
			{
				AllocConsole();
			}

			Console.Title = "Legendary Launcher";
            Console.OutputEncoding = System.Text.Encoding.Unicode;

			try
			{
				// Initialize all components
				InitializeComponents();

				// Scan for games
				ScanForGames();

				// Register command system callbacks
				_menuSystem.RegisterCommandCallbacks(
					onRescan: () => ScanForGames(),
					onExit: () => _shouldExit = true,
					onSettings: () => ShowSettings(),
					getDetectedGames: () => _detectedGames,
					onLaunchGame: (game, options) => _menuSystem.LaunchGameWithFlow(game, options, _adminElevator, _gameLauncher),
					getConfig: () => _menuSystem.GetConfig()
				);

				// Check if command-line arguments were provided
				if (args.Length > 0)
				{
					// Parse and execute command-line launch
					bool success = ParseAndLaunchFromCommandLine(args);
					
					// Exit after command-line launch attempt
					if (!success)
					{
						Environment.Exit(1);
					}
					Environment.Exit(0);
				}

				// Build main menu
				BuildMainMenu();

				// Main application loop
				RunMainLoop();
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fatal error: {ex.Message.EscapeMarkup()}[/]");
				AnsiConsole.MarkupLine("[dim]Press any key to exit...[/]");
				Console.ReadKey(true);
			}
		}

		/// <summary>
		/// Parses command-line arguments and launches the specified game directly.
		/// Format: -ME(1|2|3) -yes|-no LanguageCode [-silent] for Legendary Edition
		///         -OLDME(1|2|3) [-silent] for Original Trilogy
		///         -test-rad for testing RAD Video Tools downloader
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		/// <returns>True if launch was successful, false otherwise.</returns>
		private static bool ParseAndLaunchFromCommandLine(string[] args)
		{
			// Check for test commands first
			if (args.Length > 0 && args[0].Equals("-test-rad", StringComparison.OrdinalIgnoreCase))
			{
				return RunTestCommands(args).GetAwaiter().GetResult();
			}

			bool silent = args.Any(a => a.Equals("-silent", StringComparison.OrdinalIgnoreCase));
			GameType? gameType = null;
			GameEdition? edition = null;
			bool forceFeedback = false;
			string textLanguage = "INT";
			string voiceLanguage = "INT";

			// Parse game selection
			foreach (var arg in args)
			{
				var upperArg = arg.ToUpperInvariant();
				
				// Legendary Edition games
				if (upperArg == "-ME1")
				{
					gameType = GameType.ME1;
					edition = GameEdition.Legendary;
				}
				else if (upperArg == "-ME2")
				{
					gameType = GameType.ME2;
					edition = GameEdition.Legendary;
				}
				else if (upperArg == "-ME3")
				{
					gameType = GameType.ME3;
					edition = GameEdition.Legendary;
				}
				// Original Trilogy games
				else if (upperArg == "-OLDME1")
				{
					gameType = GameType.ME1;
					edition = GameEdition.Original;
				}
				else if (upperArg == "-OLDME2")
				{
					gameType = GameType.ME2;
					edition = GameEdition.Original;
				}
				else if (upperArg == "-OLDME3")
				{
					gameType = GameType.ME3;
					edition = GameEdition.Original;
				}
			}

			// Validate game selection
			if (!gameType.HasValue || !edition.HasValue)
			{
				if (!silent)
				{
					Console.WriteLine("Error: No valid game specified.");
					Console.WriteLine("Usage for Legendary Edition: -ME(1|2|3) -yes|-no LanguageCode [-silent] [-nointro]");
					Console.WriteLine("Usage for Original Trilogy: -OLDME(1|2|3) [-silent]");
				}
				return false;
			}

			// Parse Legendary Edition specific options
			if (edition.Value == GameEdition.Legendary)
			{
				// Parse force feedback
				bool foundForceFeedback = false;
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-yes", StringComparison.OrdinalIgnoreCase))
					{
						forceFeedback = true;
						foundForceFeedback = true;
					}
					else if (args[i].Equals("-no", StringComparison.OrdinalIgnoreCase))
					{
						forceFeedback = false;
						foundForceFeedback = true;
					}
				}

				if (!foundForceFeedback && !silent)
				{
					Console.WriteLine("Warning: Force feedback option not specified, defaulting to disabled.");
				}

				// Parse language code (should be after force feedback argument)
				// Find the position of force feedback argument and get the next non-flag argument
				for (int i = 0; i < args.Length; i++)
				{
					var upperArg = args[i].ToUpperInvariant();
					
					// Skip known flags
					if (upperArg.StartsWith("-ME") || upperArg.StartsWith("-OLDME") || 
					    upperArg == "-YES" || upperArg == "-NO" || upperArg == "-SILENT")
					{
						continue;
					}

					// This should be the language code
					// Check if it's a valid language code format (2-4 uppercase letters)
					if (upperArg.Length >= 2 && upperArg.Length <= 4 && upperArg.All(char.IsLetter))
					{
						// Map old launcher codes to our universal codes
						textLanguage = MapLegacyLanguageCode(upperArg);
						voiceLanguage = MapLegacyLanguageCode(upperArg);
						
						// Check if this is a text-only code (ends with E for English VO)
						// ME1: FE, GE, IE, RU, PL, JA = English VO
						// ME2/ME3: FRE, DEE, ITE, RUS, POE, POL (ME3), JPN = English VO
						if (IsEnglishVoiceOverCode(upperArg, gameType.Value))
						{
							voiceLanguage = "INT"; // English voice-over
						}
						
						break;
					}
				}
			}

			// Find the game in detected games
			var targetGame = _detectedGames.FirstOrDefault(g => 
				g.Type == gameType.Value && 
				g.Edition == edition.Value && 
				g.IsValid);

			if (targetGame == null)
			{
				if (!silent)
				{
					string gameName = edition.Value == GameEdition.Legendary
						? $"Mass Effect {(int)gameType.Value + 1} Legendary Edition"
						: $"Mass Effect {(int)gameType.Value + 1}";
					Console.WriteLine($"Error: {gameName} not found or not configured.");
					Console.WriteLine("Please run the launcher without arguments to configure game paths.");
				}
				return false;
			}

			// Create launch options
			var launchOptions = new LaunchOptions
			{
				Locale = textLanguage,
				VoiceLanguage = voiceLanguage,
				ForceFeedback = forceFeedback,
				Silent = silent,
				PlayIntro = !silent && !args.Any(a => a.Equals("-nointro", StringComparison.OrdinalIgnoreCase))
			};

			// Launch the game
			if (!silent)
			{
				Console.WriteLine($"Launching {targetGame.Name}...");
				if (edition.Value == GameEdition.Legendary)
				{
					Console.WriteLine($"Text Language: {textLanguage}");
					Console.WriteLine($"Voice Language: {voiceLanguage}");
					Console.WriteLine($"Force Feedback: {(forceFeedback ? "Enabled" : "Disabled")}");
				}
			}

			var result = _gameLauncher.Launch(targetGame, launchOptions);

			if (!result.Success)
			{
				if (!silent)
				{
					Console.WriteLine($"Error: {result.ErrorMessage}");
				}
				return false;
			}

			if (!silent)
			{
				Console.WriteLine("Game launched successfully!");
			}

			return true;
		}

		/// <summary>
		/// Maps legacy language codes from the old launcher to universal language codes.
		/// </summary>
		/// <param name="legacyCode">The legacy language code.</param>
		/// <returns>The universal language code.</returns>
		private static string MapLegacyLanguageCode(string legacyCode)
		{
			var upperCode = legacyCode.ToUpperInvariant();
			
			// Direct mappings
			if (upperCode == "INT") return "INT";
			
			// ME1 codes
			if (upperCode == "FE" || upperCode == "FR") return "FR";
			if (upperCode == "GE" || upperCode == "DE") return "DE";
			if (upperCode == "ES") return "ES";
			if (upperCode == "IE" || upperCode == "IT") return "IT";
			if (upperCode == "RU" || upperCode == "RA") return "RU";
			if (upperCode == "PL" || upperCode == "PLPC") return "PL";
			if (upperCode == "JA") return "JA";
			
			// ME2/ME3 codes
			if (upperCode == "FRE" || upperCode == "FRA") return "FR";
			if (upperCode == "DEE" || upperCode == "DEU") return "DE";
			if (upperCode == "ESN") return "ES";
			if (upperCode == "ITE" || upperCode == "ITA") return "IT";
			if (upperCode == "RUS") return "RU";
			if (upperCode == "POE" || upperCode == "POL") return "PL";
			if (upperCode == "JPN") return "JA";
			
			// Default to English if unknown
			return "INT";
		}

		/// <summary>
		/// Checks if a legacy language code represents English voice-over.
		/// </summary>
		/// <param name="legacyCode">The legacy language code.</param>
		/// <param name="gameType">The game type.</param>
		/// <returns>True if the code represents English voice-over.</returns>
		private static bool IsEnglishVoiceOverCode(string legacyCode, GameType gameType)
		{
			var upperCode = legacyCode.ToUpperInvariant();
			
			if (gameType == GameType.ME1)
			{
				// ME1: FE, GE, IE, RU, PL, JA = English VO
				return upperCode == "FE" || upperCode == "GE" || upperCode == "IE" || 
				       upperCode == "RU" || upperCode == "PL" || upperCode == "JA";
			}
			else
			{
				// ME2/ME3: FRE, DEE, ITE, RUS, POE, POL (ME3), JPN = English VO
				return upperCode == "FRE" || upperCode == "DEE" || upperCode == "ITE" || 
				       upperCode == "RUS" || upperCode == "POE" || 
				       (upperCode == "POL" && gameType == GameType.ME3) || 
				       upperCode == "JPN";
			}
		}

		/// <summary>
		/// Initializes all core components: ConfigManager, AdminElevator, GameDetector, MenuSystem, GameLauncher.
		/// Requirements: 1.1, 2.1, 3.1
		/// </summary>
		private static void InitializeComponents()
		{
			// Initialize ConfigManager and load settings
			_configManager = new ConfigManager();
			
			// Initialize AdminElevator and check current status
			_adminElevator = new AdminElevator();
			bool isAdmin = _adminElevator.IsRunningAsAdmin();

			// Initialize GameDetector
			_gameDetector = new GameDetector(_adminElevator);

			// Initialize MenuSystem with detected games
			_menuSystem = new MenuSystem();
			_menuSystem.Initialize(_configManager);
			_menuSystem.SetAdminStatus(isAdmin);

			// Initialize GameLauncher
			_gameLauncher = new GameLauncher();

			// Set up menu system callbacks
			_menuSystem.SetRescanGamesCallback(() =>
			{
				ScanForGames();
				BuildMainMenu();
			});

			_menuSystem.SetManualPathAddedCallback((path) =>
			{
				HandleManualPathInput(path);
			});
		}

		/// <summary>
		/// Scans for Mass Effect game installations and updates the configuration.
		/// </summary>
		private static void ScanForGames()
		{
			_menuSystem.ShowProgress("Scanning for Mass Effect games...", () =>
			{
				_detectedGames = _gameDetector.ScanForGames();
				_menuSystem.UpdateDetectedGames(_detectedGames);
			});

			// Validate saved game paths and offer rescan if invalid (Requirement 6.4)
			ValidateSavedPaths();

			// Display scan results
			if (_detectedGames.Count == 0)
			{
				_menuSystem.ShowMessage(
					"No Mass Effect games were detected.\n" +
					"You can manually add a game path from the Settings menu.",
					MessageType.Warning);
			}
			else
			{
				_menuSystem.ShowMessage(
					$"Found {_detectedGames.Count} Mass Effect game(s)!",
					MessageType.Success);
			}
		}

		/// <summary>
		/// Validates saved game paths and offers to rescan if any are invalid.
		/// Requirement 6.4: Handle invalid saved paths with rescan offer
		/// </summary>
		private static void ValidateSavedPaths()
		{
			var config = _menuSystem.GetConfig();
			if (config == null || config.Games == null || config.Games.Count == 0)
				return;

			var invalidGames = new List<GameConfig>();

			foreach (var gameConfig in config.Games)
			{
				// Check if the saved path is still valid
				if (!_gameDetector.ValidateInstallation(gameConfig.Path, gameConfig.Type, gameConfig.Edition))
				{
					invalidGames.Add(gameConfig);
				}
			}

			if (invalidGames.Count > 0)
			{
				var gameNames = string.Join("\n", invalidGames.Select(g =>
					$"- {(g.Edition == GameEdition.Legendary ? "Mass Effect " + ((int)g.Type + 1) + " Legendary Edition" : "Mass Effect " + ((int)g.Type + 1))}"));

				_menuSystem.ShowMessage(
					$"The following saved game paths are no longer valid:\n{gameNames}\n\n" +
					"These games may have been moved or uninstalled.",
					MessageType.Warning);

				if (_menuSystem.ShowConfirmation("Would you like to rescan for games now?"))
				{
					// Remove invalid games from config
					foreach (var invalidGame in invalidGames)
					{
						config.Games.Remove(invalidGame);
					}
					_configManager.Save(config);

					// Rescan
					_menuSystem.ShowProgress("Rescanning for Mass Effect games...", () =>
					{
						_detectedGames = _gameDetector.ScanForGames();
						_menuSystem.UpdateDetectedGames(_detectedGames);
					});
				}
			}
		}

		/// <summary>
		/// Builds the main menu with detected games and options.
		/// </summary>
		private static void BuildMainMenu()
		{
			var menuItems = new List<MenuItem>();

			// Add detected games to menu
			foreach (var game in _detectedGames.OrderBy(g => g.Edition).ThenBy(g => g.Type))
			{
				var capturedGame = game; // Capture for closure
				menuItems.Add(new MenuItem
				{
					Title = game.Name,
					Description = game.Path,
					Type = MenuItemType.Game,
					IsEnabled = true,
					Tag = game,  // Add Tag for context panel
					OnSelect = () => ShowGameOptions(capturedGame)
				});
			}

			// Add separator if we have games
			if (menuItems.Count > 0)
			{
				menuItems.Add(new MenuItem
				{
					Title = "",
					Type = MenuItemType.Separator,
					IsEnabled = false
				});
			}

			// Add settings menu item
			menuItems.Add(new MenuItem
			{
				Title = "⚙ Settings",
				Description = "Configure launcher preferences",
				Type = MenuItemType.Action,
				IsEnabled = true,
				OnSelect = () => ShowSettings()
			});

			// Add exit menu item
			menuItems.Add(new MenuItem
			{
				Title = "✕ Exit",
				Description = "Close the launcher",
				Type = MenuItemType.Action,
				IsEnabled = true,
				OnSelect = () => _shouldExit = true
			});

			_menuSystem.SetMenuItems(menuItems);
			_menuSystem.SetMenuState(MenuState.Main);
		}

		/// <summary>
		/// Shows the game options submenu for a selected game.
		/// </summary>
		private static void ShowGameOptions(DetectedGame game)
		{
			var gameOptionsMenu = _menuSystem.BuildGameOptionsMenu(game, (launchOptions) =>
			{
				_menuSystem.LaunchGameWithFlow(game, launchOptions, _adminElevator, _gameLauncher);
			});

			_menuSystem.SetMenuItems(gameOptionsMenu);
			_menuSystem.SetMenuState(MenuState.GameOptions);
		}

		/// <summary>
		/// Shows the settings submenu.
		/// </summary>
		private static void ShowSettings()
		{
			var settingsMenu = _menuSystem.BuildSettingsMenu();
			_menuSystem.SetMenuItems(settingsMenu);
			_menuSystem.SetMenuState(MenuState.Settings);
		}

		/// <summary>
		/// Handles manual game path input from the user.
		/// </summary>
		private static void HandleManualPathInput(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
			{
				_menuSystem.ShowMessage("Invalid path. Please enter a valid directory path.", MessageType.Error);
				return;
			}

			// Try to detect what game this is
			bool foundGame = false;

			foreach (GameType gameType in Enum.GetValues(typeof(GameType)))
			{
				foreach (GameEdition edition in Enum.GetValues(typeof(GameEdition)))
				{
					if (_gameDetector.ValidateInstallation(path, gameType, edition))
					{
						var relativePath = edition == GameEdition.Legendary
							? GamePaths.LegendaryPaths[gameType]
							: GamePaths.OriginalPaths[gameType];

						var executablePath = Path.Combine(path, relativePath);

						var detectedGame = new DetectedGame
						{
							Name = edition == GameEdition.Legendary
								? $"Mass Effect {(int)gameType + 1} Legendary Edition"
								: $"Mass Effect {(int)gameType + 1}",
							Path = path,
							ExecutablePath = executablePath,
							Type = gameType,
							Edition = edition,
							IsValid = true,
							RequiresAdmin = _adminElevator.RequiresElevation(executablePath)
						};

						// Add to detected games if not already present
						if (!_detectedGames.Any(g => g.ExecutablePath?.Equals(executablePath, StringComparison.OrdinalIgnoreCase) == true))
						{
							_detectedGames.Add(detectedGame);
							_menuSystem.UpdateDetectedGames(_detectedGames);
							foundGame = true;

							_menuSystem.ShowMessage(
								$"Successfully added {detectedGame.Name}!",
								MessageType.Success);

							// Rebuild main menu to include the new game
							BuildMainMenu();
							break;
						}
						else
						{
							_menuSystem.ShowMessage(
								$"{detectedGame.Name} is already in your game list.",
								MessageType.Info);
							foundGame = true;
							break;
						}
					}
				}

				if (foundGame)
					break;
			}

			if (!foundGame)
			{
				_menuSystem.ShowMessage(
					"Could not detect a valid Mass Effect game at the specified path.\n" +
					"Please ensure the path points to a Mass Effect game installation directory.",
					MessageType.Error);
			}
		}

		/// <summary>
		/// Main application loop - renders menu and handles input.
		/// Requirements: 1.3, 1.4, 1.5
		/// </summary>
		private static void RunMainLoop()
		{
			// Build the main menu initially
			BuildMainMenu();
			
			while (!_shouldExit)
			{
				// Render the current menu
				_menuSystem.Render();

				// Handle keyboard input
				var key = Console.ReadKey(true);
				
				// Store the previous state before handling input
				var previousState = _menuSystem.GetCurrentState();
				
				_menuSystem.HandleInput(key);
				
				// Get the current state after handling input
				var currentState = _menuSystem.GetCurrentState();

				// Check if we need to rebuild the main menu after returning from submenus
				if (currentState == MenuState.Main && previousState != MenuState.Main)
				{
					// We just returned to main menu from a submenu - rebuild it
					BuildMainMenu();
				}
				else if (currentState == MenuState.Main && key.Key == ConsoleKey.Escape && !_menuSystem.IsInInputMode)
				{
					// User pressed Escape on main menu (and not in input mode) - confirm exit
					if (_menuSystem.ShowConfirmation("Are you sure you want to exit?"))
					{
						_shouldExit = true;
					}
				}
			}

			// Graceful exit
			AnsiConsole.Clear();
			AnsiConsole.MarkupLine("[cyan]Thank you for using Mass Effect Legendary Launcher![/]");
		}

		/// <summary>
		/// Runs test commands for debugging and verification.
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		/// <returns>True if tests completed successfully, false otherwise.</returns>
		private static async Task<bool> RunTestCommands(string[] args)
		{
			try
			{
				Console.WriteLine("🧪 Running RAD Video Tools Downloader Tests...");
				Console.WriteLine();

				// Test RAD downloader
				await TestRadDownloader.TestDownloadAsync();
				Console.WriteLine();

				// Test intro player
				await TestRadDownloader.TestIntroPlayerAsync();
				Console.WriteLine();

				Console.WriteLine("✅ All tests completed!");
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey(true);
				
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Test failed with exception: {ex.Message}");
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey(true);
				return false;
			}
		}
	}
}
