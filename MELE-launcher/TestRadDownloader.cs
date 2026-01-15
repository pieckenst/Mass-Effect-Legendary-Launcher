using System;
using System.Threading.Tasks;
using MELE_launcher.Components;

namespace MELE_launcher
{
    /// <summary>
    /// Simple test class to verify RAD Video Tools downloader functionality.
    /// </summary>
    public class TestRadDownloader
    {
        public static async Task TestDownloadAsync()
        {
            Console.WriteLine("🧪 Testing RAD Video Tools Downloader...");
            
            var downloader = new RadVideoToolsDownloader();
            
            // Test if BinkPlay.exe is already available
            if (downloader.IsBinkPlayerAvailable())
            {
                Console.WriteLine("✅ BinkPlay.exe is already available!");
                return;
            }
            
            Console.WriteLine("📥 BinkPlay.exe not found, attempting download...");
            
            try
            {
                string binkPlayerPath = await downloader.EnsureBinkPlayerAsync();
                
                if (binkPlayerPath != null)
                {
                    Console.WriteLine($"✅ Successfully downloaded and set up BinkPlay.exe at: {binkPlayerPath}");
                }
                else
                {
                    Console.WriteLine("❌ Failed to download or set up BinkPlay.exe");
                    Console.WriteLine("💡 This may be due to:");
                    Console.WriteLine("   - Network connectivity issues");
                    Console.WriteLine("   - Missing 7-Zip installation");
                    Console.WriteLine("   - RAD Tools server unavailable");
                    Console.WriteLine("   - Antivirus blocking the download");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during download: {ex.Message}");
            }
        }
        
        public static async Task TestIntroPlayerAsync()
        {
            Console.WriteLine("🧪 Testing Intro Player...");
            
            // Test with a fake game path to see if the intro player handles missing files gracefully
            var introPlayer = new IntroPlayer();
            
            string testGamePath = @"C:\NonExistent\Path";
            
            try
            {
                bool result = await introPlayer.PlayBioWareIntroAsync(testGamePath, allowSkip: true);
                
                if (!result)
                {
                    Console.WriteLine("✅ Intro player correctly handled missing video file");
                }
                else
                {
                    Console.WriteLine("⚠ Unexpected result from intro player");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in intro player: {ex.Message}");
            }
        }
    }
}