using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MELE_launcher.Utilities
{
    /// <summary>
    /// Shared HTTP download helpers used by the various tool downloaders.
    /// </summary>
    public static class FileDownloader
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Downloads a file from the given URL to the destination path.
        /// </summary>
        /// <param name="url">The URL to download from.</param>
        /// <param name="destinationPath">The local file path to write the response body to.</param>
        /// <param name="timeout">Optional request timeout (defaults to 5 minutes).</param>
        /// <param name="userAgent">Optional User-Agent header to send with the request.</param>
        public static async Task DownloadToFileAsync(string url, string destinationPath, TimeSpan? timeout = null, string userAgent = null)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = timeout ?? DefaultTimeout;

            if (!string.IsNullOrEmpty(userAgent))
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var fileStream = File.Create(destinationPath);
            await response.Content.CopyToAsync(fileStream);
        }
    }
}
