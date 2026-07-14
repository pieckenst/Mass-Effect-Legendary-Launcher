using System.Diagnostics;
using System.Threading.Tasks;

namespace MELE_launcher.Utilities
{
    /// <summary>
    /// The result of running an external process via <see cref="ProcessRunner"/>.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>Whether the process was successfully started.</summary>
        public bool Started { get; set; }

        /// <summary>The process exit code (only meaningful when <see cref="Started"/> is true).</summary>
        public int ExitCode { get; set; }

        /// <summary>Captured standard output.</summary>
        public string StandardOutput { get; set; } = string.Empty;

        /// <summary>Captured standard error.</summary>
        public string StandardError { get; set; } = string.Empty;

        /// <summary>True when the process started and exited with code 0.</summary>
        public bool Success => Started && ExitCode == 0;
    }

    /// <summary>
    /// Runs external processes with their output captured, centralizing the common
    /// <see cref="ProcessStartInfo"/> setup shared across components.
    /// </summary>
    public static class ProcessRunner
    {
        /// <summary>
        /// Starts a process with redirected/captured output and waits for it to exit.
        /// </summary>
        /// <param name="fileName">The executable to run.</param>
        /// <param name="arguments">The command-line arguments.</param>
        /// <returns>A <see cref="ProcessResult"/> describing the outcome.</returns>
        public static async Task<ProcessResult> RunAsync(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ProcessResult { Started = false };
            }

            // Read streams concurrently before waiting to avoid deadlocks on full buffers.
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new ProcessResult
            {
                Started = true,
                ExitCode = process.ExitCode,
                StandardOutput = await outputTask,
                StandardError = await errorTask
            };
        }
    }
}
