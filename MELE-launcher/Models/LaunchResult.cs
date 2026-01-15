namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents the result of a game launch attempt.
    /// </summary>
    public class LaunchResult
    {
        /// <summary>
        /// Gets or sets whether the launch was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the launch failed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
