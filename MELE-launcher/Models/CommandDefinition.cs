using System;
using System.Collections.Generic;

namespace MELE_launcher.Models
{
    /// <summary>
    /// Represents a command that can be executed in command mode.
    /// </summary>
    public class CommandDefinition
    {
        /// <summary>
        /// The command name/trigger (e.g., "help", "scan", "exit").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Alternative names/aliases for the command.
        /// </summary>
        public List<string> Aliases { get; set; } = new List<string>();

        /// <summary>
        /// Description of what the command does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Usage syntax (e.g., "help [command]").
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// Category for grouping commands (e.g., "System", "Game", "Settings").
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Whether the command requires admin privileges.
        /// </summary>
        public bool RequiresAdmin { get; set; }

        /// <summary>
        /// Whether the command is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Built-in action type for predefined commands.
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Parameters for the action (flexible for different command types).
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Container for command definitions loaded from JSON.
    /// </summary>
    public class CommandRegistry
    {
        /// <summary>
        /// List of available commands.
        /// </summary>
        public List<CommandDefinition> Commands { get; set; } = new List<CommandDefinition>();

        /// <summary>
        /// Version of the command registry format.
        /// </summary>
        public string Version { get; set; } = "1.0";
    }
}
