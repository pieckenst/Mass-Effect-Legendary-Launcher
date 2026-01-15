namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents a detected Mass Effect game installation.
    /// </summary>
    public class DetectedGame
    {
        /// <summary>
        /// Gets or sets the display name of the game.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the installation directory path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the full path to the game executable.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets the type of Mass Effect game.
        /// </summary>
        public GameType Type { get; set; }

        /// <summary>
        /// Gets or sets the edition of the game (Legendary or Original).
        /// </summary>
        public GameEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets whether the installation is valid (executable exists).
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets whether the game requires administrator privileges to launch.
        /// </summary>
        public bool RequiresAdmin { get; set; }
    }
}
