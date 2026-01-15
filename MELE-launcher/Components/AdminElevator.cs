using System;
using System.Diagnostics;
using System.Security.Principal;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Handles Windows administrator privilege detection and elevation.
    /// </summary>
    public class AdminElevator
    {
        /// <summary>
        /// Checks if the current process is running with administrator privileges.
        /// </summary>
        /// <returns>True if running as administrator, false otherwise.</returns>
        public bool IsRunningAsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception)
            {
                // If we can't determine admin status, assume we're not admin
                return false;
            }
        }

        /// <summary>
        /// Checks if a given path is in a Windows protected directory.
        /// Protected directories include Program Files, Windows, and system drive root with restricted permissions.
        /// </summary>
        /// <param name="path">The file or directory path to check.</param>
        /// <returns>True if the path is in a protected directory, false otherwise.</returns>
        public bool IsProtectedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                // Normalize the path to handle different formats
                string normalizedPath = System.IO.Path.GetFullPath(path).ToUpperInvariant();

                // Get system drive (usually C:)
                string systemDrive = Environment.GetFolderPath(Environment.SpecialFolder.System)
                    .Substring(0, 3)
                    .ToUpperInvariant();

                // Check for Program Files directories
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                    .ToUpperInvariant();
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                    .ToUpperInvariant();

                if (normalizedPath.StartsWith(programFiles) || 
                    normalizedPath.StartsWith(programFilesX86))
                {
                    return true;
                }

                // Check for Windows directory
                string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows)
                    .ToUpperInvariant();
                
                if (normalizedPath.StartsWith(windowsDir))
                {
                    return true;
                }

                // Check if path is directly on system drive root (e.g., C:\SomeFolder)
                // This is considered protected as it requires elevated permissions
                if (normalizedPath.StartsWith(systemDrive))
                {
                    // Extract the path after the drive letter
                    string pathAfterDrive = normalizedPath.Substring(3);
                    
                    // If there's only one directory level after the drive (e.g., C:\Games\)
                    // it's considered protected
                    int firstSlash = pathAfterDrive.IndexOf('\\');
                    if (firstSlash == -1 || firstSlash == pathAfterDrive.Length - 1)
                    {
                        // Direct child of system drive root
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                // If we can't determine the path status, assume it's protected to be safe
                return true;
            }
        }

        /// <summary>
        /// Determines if elevation is required to access a given path.
        /// Combines protected path detection with current privilege status.
        /// </summary>
        /// <param name="path">The file or directory path to check.</param>
        /// <returns>True if elevation is required, false otherwise.</returns>
        public bool RequiresElevation(string path)
        {
            // If we're already running as admin, no elevation needed
            if (IsRunningAsAdmin())
            {
                return false;
            }

            // If the path is protected and we're not admin, elevation is required
            return IsProtectedPath(path);
        }

        /// <summary>
        /// Requests administrator elevation by restarting the application with elevated privileges.
        /// Uses Windows UAC (User Account Control) to prompt for elevation.
        /// </summary>
        /// <param name="args">Command-line arguments to pass to the restarted application.</param>
        /// <returns>True if elevation request was successful, false otherwise.</returns>
        public bool RequestElevation(string[] args)
        {
            try
            {
                // Get the current executable path
                string exePath = Process.GetCurrentProcess().MainModule?.FileName;
                
                if (string.IsNullOrEmpty(exePath))
                {
                    return false;
                }

                // Create process start info with elevation
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas", // This triggers UAC elevation prompt
                    Arguments = args != null ? string.Join(" ", args) : string.Empty
                };

                // Start the elevated process
                Process.Start(startInfo);

                // If we successfully started the elevated process, return true
                // The calling code should exit the current non-elevated process
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // User declined UAC prompt or elevation failed
                return false;
            }
            catch (Exception)
            {
                // Other errors during elevation
                return false;
            }
        }
    }
}
