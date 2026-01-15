using System.Collections.Generic;

namespace MELE_launcher.Models
{
    /// <summary>
    /// Contains constants for game installation paths and common directories.
    /// </summary>
    public static class GamePaths
    {
        /// <summary>
        /// Legendary Edition executable paths relative to installation root.
        /// </summary>
        public static readonly Dictionary<GameType, string> LegendaryPaths = new()
        {
            { GameType.ME1, @"Game\ME1\Binaries\Win64\MassEffect1.exe" },
            { GameType.ME2, @"Game\ME2\Binaries\Win64\MassEffect2.exe" },
            { GameType.ME3, @"Game\ME3\Binaries\Win64\MassEffect3.exe" }
        };

        /// <summary>
        /// Original trilogy executable paths relative to installation root.
        /// </summary>
        public static readonly Dictionary<GameType, string> OriginalPaths = new()
        {
            { GameType.ME1, @"Binaries\MassEffect.exe" },
            { GameType.ME2, @"Binaries\MassEffect2.exe" },
            { GameType.ME3, @"Binaries\Win32\MassEffect3.exe" }
        };

        /// <summary>
        /// Common installation directories to scan for games.
        /// </summary>
        public static readonly string[] CommonDirectories =
        {
            @"C:\Program Files\EA Games",
            @"C:\Program Files (x86)\EA Games",
            @"C:\Program Files\Origin Games",
            @"C:\Program Files (x86)\Origin Games",
            @"C:\Program Files\Steam\steamapps\common",
            @"C:\Program Files (x86)\Steam\steamapps\common",
            @"E:\Mass Effect Legendary Edition",
            @"D:\Games",
            @"E:\Games",
            @"F:\Games",
            @"G:\Games"
        };
    }
}
