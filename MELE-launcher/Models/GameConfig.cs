namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents the configuration for a single Mass Effect game installation.
    /// </summary>
    public class GameConfig
    {
        /// <summary>
        /// Gets or sets the type of game (ME1, ME2, or ME3).
        /// </summary>
        public GameType Type { get; set; }

        /// <summary>
        /// Gets or sets the edition of the game (Legendary or Original).
        /// </summary>
        public GameEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets the installation path of the game.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text/subtitle language code for the game (e.g., "INT", "FR", "RU").
        /// </summary>
        public string Locale { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the voice-over language code for the game (e.g., "INT", "FR", "RU").
        /// Defaults to "INT" (English) if not set.
        /// </summary>
        public string VoiceLanguage { get; set; } = "INT";

        /// <summary>
        /// Gets or sets whether force feedback is enabled for this game.
        /// </summary>
        public bool ForceFeedback { get; set; }
    }
}
