using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MELE_launcher.Utilities
{
    /// <summary>
    /// Locates files inside known RAD Video Tools installation directories.
    /// Shared by the Bink DLL manager and the RAD Video Tools downloader so both
    /// search the same set of install locations.
    /// </summary>
    public static class RadToolsLocator
    {
        /// <summary>
        /// Common RAD Video Tools installation directories, in search order.
        /// </summary>
        public static IEnumerable<string> GetInstallDirectories()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            var directories = new[]
            {
                Path.Combine(programFilesX86, "RADVideo"),
                Path.Combine(programFiles, "RADVideo"),
                Path.Combine(programFilesX86, "RAD Game Tools"),
                Path.Combine(programFiles, "RAD Game Tools"),
                Path.Combine(programFilesX86, "RADGameTools"),
                Path.Combine(programFiles, "RADGameTools")
            };

            return directories.Distinct();
        }

        /// <summary>
        /// Returns the path to <paramref name="fileName"/> in the first RAD Tools install
        /// directory that contains it, or null if not found in any of them.
        /// </summary>
        /// <param name="fileName">The file name to look for (e.g. "binkw32.dll" or "binkplay.exe").</param>
        public static string FindInstalledFile(string fileName)
        {
            return GetInstallDirectories()
                .Select(directory => Path.Combine(directory, fileName))
                .FirstOrDefault(File.Exists);
        }
    }
}
