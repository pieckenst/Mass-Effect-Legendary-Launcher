namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents options for launching a Mass Effect game.
    /// </summary>
    public class LaunchOptions
    {
        /// <summary>
        /// Gets or sets the text/subtitle language code for the game (e.g., "INT" for English).
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the voice-over language code for the game (e.g., "INT" for English).
        /// If not specified, defaults to the same as Locale.
        /// </summary>
        public string VoiceLanguage { get; set; }

        /// <summary>
        /// Gets or sets whether force feedback is enabled.
        /// </summary>
        public bool ForceFeedback { get; set; }

        /// <summary>
        /// Gets or sets whether to launch the game in silent mode (no splash screens).
        /// </summary>
        public bool Silent { get; set; }

        /// <summary>
        /// Gets or sets whether to play the BioWare intro video before launching the game.
        /// </summary>
        public bool PlayIntro { get; set; } = true;
    }
}
