using System;
using System.Collections.Generic;

namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents the complete launcher configuration including all game settings.
    /// </summary>
    public class LauncherConfig
    {
        /// <summary>
        /// Gets or sets the list of configured games.
        /// </summary>
        public List<GameConfig> Games { get; set; } = new List<GameConfig>();

        /// <summary>
        /// Gets or sets the default text/subtitle language code for new games.
        /// </summary>
        public string DefaultLocale { get; set; } = "INT";

        /// <summary>
        /// Gets or sets the default voice-over language code for new games.
        /// </summary>
        public string DefaultVoiceLanguage { get; set; } = "INT";

        /// <summary>
        /// Gets or sets the default force feedback setting for new games.
        /// </summary>
        public bool DefaultForceFeedback { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to skip the BioWare intro video by default.
        /// </summary>
        public bool DefaultSkipIntro { get; set; } = true;

        /// <summary>
        /// Gets or sets the date when games were last scanned.
        /// </summary>
        public DateTime LastScanDate { get; set; } = DateTime.MinValue;
    }
}
