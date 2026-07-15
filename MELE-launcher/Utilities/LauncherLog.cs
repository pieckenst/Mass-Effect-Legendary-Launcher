using System;

namespace MELE_launcher.Utilities
{
    /// <summary>
    /// Central logging seam for the launcher. Provides a single place where
    /// diagnostics are formatted and routed, replacing ad-hoc calls to
    /// <see cref="Console.Error"/> and <see cref="System.Diagnostics.Debug"/>
    /// scattered across components.
    ///
    /// User-facing messages (<see cref="Warning"/>/<see cref="Error"/>) are written
    /// to standard error so they are visible in the console, and full detail
    /// (including exceptions) is always written to debug output. Purely internal
    /// diagnostics use <see cref="Diagnostic"/>, which never touches the console.
    /// </summary>
    public static class LauncherLog
    {
        /// <summary>
        /// Records an internal, non-fatal diagnostic. Written to debug output only;
        /// never shown to the user.
        /// </summary>
        public static void Diagnostic(string component, string message, Exception ex = null)
        {
            WriteDebug(component, message, ex);
        }

        /// <summary>
        /// Reports a warning to the user (stderr) and records full detail for diagnostics.
        /// </summary>
        public static void Warning(string component, string message, Exception ex = null)
        {
            Console.Error.WriteLine(message);
            WriteDebug(component, message, ex);
        }

        /// <summary>
        /// Reports an error to the user (stderr) and records full detail for diagnostics.
        /// </summary>
        public static void Error(string component, string message, Exception ex = null)
        {
            Console.Error.WriteLine(message);
            WriteDebug(component, message, ex);
        }

        private static void WriteDebug(string component, string message, Exception ex)
        {
            string prefixed = string.IsNullOrEmpty(component) ? message : $"[{component}] {message}";
            System.Diagnostics.Debug.WriteLine(ex != null ? $"{prefixed} Exception: {ex}" : prefixed);
        }
    }
}
