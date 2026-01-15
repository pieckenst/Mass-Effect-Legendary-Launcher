using System;
using System.Diagnostics;
using MELE_launcher.Models;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Handles launching Mass Effect games with appropriate arguments and elevation.
    /// </summary>
    public class GameLauncher
    {
        /// <summary>
        /// Launches a Mass Effect game with the specified options.
        /// </summary>
        /// <param name="game">The detected game to launch.</param>
        /// <param name="options">Launch options including locale and force feedback settings.</param>
        /// <returns>A LaunchResult indicating success or failure with error details.</returns>
        public LaunchResult Launch(DetectedGame game, LaunchOptions options)
        {
            if (game == null)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = "Game cannot be null."
                };
            }

            if (!game.IsValid)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = $"Game installation at '{game.Path}' is not valid."
                };
            }

            if (string.IsNullOrEmpty(game.ExecutablePath) || !System.IO.File.Exists(game.ExecutablePath))
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = $"Game executable not found at '{game.ExecutablePath}'."
                };
            }

            try
            {
                string arguments = BuildArguments(game, options);
                ProcessStartInfo startInfo = CreateStartInfo(game, arguments);
                
                // Log what we're about to launch for debugging
                System.Diagnostics.Debug.WriteLine($"Launching: {startInfo.FileName}");
                System.Diagnostics.Debug.WriteLine($"Arguments: {startInfo.Arguments}");
                System.Diagnostics.Debug.WriteLine($"Working Directory: {startInfo.WorkingDirectory}");
                System.Diagnostics.Debug.WriteLine($"Use Shell Execute: {startInfo.UseShellExecute}");
                System.Diagnostics.Debug.WriteLine($"Verb: {startInfo.Verb}");
                
                Process process = Process.Start(startInfo);
                
                if (process == null)
                {
                    return new LaunchResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to start game process. Process.Start returned null."
                    };
                }

                // Give the process a moment to start
                System.Threading.Thread.Sleep(500);
                
                // Check if the process actually started and is still running
                try
                {
                    if (process.HasExited)
                    {
                        return new LaunchResult
                        {
                            Success = false,
                            ErrorMessage = $"Game process exited immediately with code: {process.ExitCode}"
                        };
                    }
                }
                catch
                {
                    // If we can't check HasExited, the process might have started with elevation
                    // and we don't have access to it anymore - this is actually okay
                }

                return new LaunchResult
                {
                    Success = true,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to launch game: {ex.Message}\n\nStack trace: {ex.StackTrace}"
                };
            }
        }

        /// <summary>
        /// Builds command-line arguments based on game edition and launch options.
        /// </summary>
        /// <param name="game">The game to build arguments for.</param>
        /// <param name="options">Launch options to include in arguments.</param>
        /// <returns>A string containing the command-line arguments.</returns>
        private string BuildArguments(DetectedGame game, LaunchOptions options)
        {
            if (options == null)
            {
                return string.Empty;
            }

            var args = new System.Collections.Generic.List<string>();

            // Legendary Edition supports locale and force feedback options
            if (game.Edition == GameEdition.Legendary)
            {
                // Required base arguments for all Legendary Edition games
                args.Add("-NoHomeDir");
                args.Add("-SeekFreeLoadingPCConsole");
                
                // Add locale argument if specified
                if (!string.IsNullOrWhiteSpace(options.Locale))
                {
                    // Get text and voice language (voice defaults to locale if not specified)
                    string textLang = options.Locale;
                    string voiceLang = string.IsNullOrWhiteSpace(options.VoiceLanguage) 
                        ? options.Locale 
                        : options.VoiceLanguage;
                    
                    // Convert to game-specific locale code
                    string gameLocaleCode = LocaleMapper.GetGameLocaleCode(textLang, voiceLang, game.Type);
                    
                    // ME1 and ME2 use -OVERRIDELANGUAGE, ME3 uses -language
                    if (game.Type == GameType.ME1 || game.Type == GameType.ME2)
                    {
                        args.Add("-locale");
                        args.Add("locale");
                        args.Add($"-OVERRIDELANGUAGE={gameLocaleCode}");
                    }
                    else if (game.Type == GameType.ME3)
                    {
                        args.Add("-locale");
                        args.Add("locale");
                        args.Add($"-language={gameLocaleCode}");
                    }
                }

                // Add subtitles
                args.Add("-Subtitles");
                args.Add("20");

                // Add force feedback argument if disabled
                if (!options.ForceFeedback)
                {
                    args.Add("-NOFORCEFEEDBACK");
                }

                // Telemetry opt-in (set to 0 to disable)
                args.Add("-TELEMOPTIN");
                args.Add("0");

                // Add silent mode if requested
                if (options.Silent)
                {
                    args.Add("-NOSPLASH");
                }
            }
            else if (game.Edition == GameEdition.Original)
            {
                // Original trilogy has different argument format
                // Most original games don't support these options, but we can add silent mode
                if (options.Silent)
                {
                    args.Add("-nosplash");
                }
            }

            return string.Join(" ", args);
        }

        /// <summary>
        /// Creates a ProcessStartInfo configured with the game executable, arguments, and elevation if needed.
        /// </summary>
        /// <param name="game">The game to create start info for.</param>
        /// <param name="arguments">Command-line arguments to pass to the game.</param>
        /// <returns>A configured ProcessStartInfo object.</returns>
        private ProcessStartInfo CreateStartInfo(DetectedGame game, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = game.ExecutablePath,
                Arguments = arguments,
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(game.ExecutablePath)
            };

            // Request elevation if the game requires administrator privileges
            if (game.RequiresAdmin)
            {
                startInfo.Verb = "runas";
            }

            return startInfo;
        }
    }
}
