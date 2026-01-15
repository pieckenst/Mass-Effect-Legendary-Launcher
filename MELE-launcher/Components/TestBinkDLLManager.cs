using System;
using System.IO;

namespace MELE_launcher.Components
{
    /// <summary>
    /// Test class for BinkDLLManager functionality.
    /// </summary>
    public static class TestBinkDLLManager
    {
        /// <summary>
        /// Tests the BinkDLLManager functionality.
        /// </summary>
        /// <param name="gamePath">Optional game path to test with.</param>
        public static void RunTest(string gamePath = null)
        {
            Console.WriteLine("🧪 Testing BinkDLLManager...");
            Console.WriteLine();

            // Test 1: Check if DLL is already available
            Console.WriteLine("Test 1: Checking if binkw32.dll is already available...");
            bool isAvailable = BinkDLLManager.IsBinkDLLAvailable();
            Console.WriteLine($"Result: {(isAvailable ? "✅ Available" : "❌ Not available")}");
            
            if (isAvailable)
            {
                var info = BinkDLLManager.GetBinkDLLInfo();
                Console.WriteLine($"Location: {info.FullName}");
                Console.WriteLine($"Size: {info.Length:N0} bytes");
                Console.WriteLine($"Modified: {info.LastWriteTime}");
            }
            Console.WriteLine();

            // Test 2: Try to ensure DLL is available
            Console.WriteLine("Test 2: Ensuring binkw32.dll is available...");
            bool ensured = BinkDLLManager.EnsureBinkDLL(gamePath);
            Console.WriteLine($"Result: {(ensured ? "✅ Success" : "❌ Failed")}");
            
            if (ensured)
            {
                var info = BinkDLLManager.GetBinkDLLInfo();
                Console.WriteLine($"Location: {info.FullName}");
                Console.WriteLine($"Size: {info.Length:N0} bytes");
            }
            Console.WriteLine();

            // Test 3: Validate the DLL
            Console.WriteLine("Test 3: Validating binkw32.dll...");
            bool isValid = BinkDLLManager.ValidateBinkDLL();
            Console.WriteLine($"Result: {(isValid ? "✅ Valid" : "❌ Invalid")}");
            Console.WriteLine();

            // Test 4: Test BinkSDKPlayer availability
            Console.WriteLine("Test 4: Testing BinkSDKPlayer availability...");
            bool sdkAvailable = BinkSDKPlayer.IsSDKAvailable;
            Console.WriteLine($"Result: {(sdkAvailable ? "✅ SDK Available" : "❌ SDK Not Available")}");
            Console.WriteLine();

            // Summary
            Console.WriteLine("📊 Test Summary:");
            Console.WriteLine($"  DLL Available: {(BinkDLLManager.IsBinkDLLAvailable() ? "✅" : "❌")}");
            Console.WriteLine($"  DLL Valid: {(BinkDLLManager.ValidateBinkDLL() ? "✅" : "❌")}");
            Console.WriteLine($"  SDK Ready: {(BinkSDKPlayer.IsSDKAvailable ? "✅" : "❌")}");
            
            if (BinkDLLManager.IsBinkDLLAvailable())
            {
                Console.WriteLine($"  DLL Path: {BinkDLLManager.GetLocalBinkDLLPath()}");
            }
            
            Console.WriteLine();
            Console.WriteLine("🧪 BinkDLLManager test completed!");
        }

        /// <summary>
        /// Tests finding the DLL in a specific game path.
        /// </summary>
        /// <param name="gamePath">Game installation path to test.</param>
        public static void TestGamePath(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
            {
                Console.WriteLine($"❌ Invalid game path: {gamePath}");
                return;
            }

            Console.WriteLine($"🎮 Testing game path: {gamePath}");
            Console.WriteLine();

            // Remove existing DLL to test fresh discovery
            if (BinkDLLManager.IsBinkDLLAvailable())
            {
                Console.WriteLine("🗑 Removing existing DLL for clean test...");
                BinkDLLManager.RemoveBinkDLL();
            }

            // Test discovery from this specific path
            bool found = BinkDLLManager.EnsureBinkDLL(gamePath);
            Console.WriteLine($"Discovery result: {(found ? "✅ Found and copied" : "❌ Not found")}");

            if (found)
            {
                var info = BinkDLLManager.GetBinkDLLInfo();
                Console.WriteLine($"Copied to: {info.FullName}");
                Console.WriteLine($"Size: {info.Length:N0} bytes");
            }
        }

        /// <summary>
        /// Demonstrates the complete workflow.
        /// </summary>
        public static void DemoWorkflow()
        {
            Console.WriteLine("🚀 BinkDLLManager Workflow Demo");
            Console.WriteLine("================================");
            Console.WriteLine();

            Console.WriteLine("Step 1: Check current status...");
            bool initialStatus = BinkDLLManager.IsBinkDLLAvailable();
            Console.WriteLine($"Initial DLL status: {(initialStatus ? "Available" : "Not available")}");
            Console.WriteLine();

            Console.WriteLine("Step 2: Ensure DLL is available...");
            bool ensured = BinkDLLManager.EnsureBinkDLL();
            Console.WriteLine($"Ensure result: {(ensured ? "Success" : "Failed")}");
            Console.WriteLine();

            if (ensured)
            {
                Console.WriteLine("Step 3: Validate DLL...");
                bool valid = BinkDLLManager.ValidateBinkDLL();
                Console.WriteLine($"Validation result: {(valid ? "Valid" : "Invalid")}");
                Console.WriteLine();

                Console.WriteLine("Step 4: Test SDK availability...");
                bool sdkReady = BinkSDKPlayer.IsSDKAvailable;
                Console.WriteLine($"SDK ready: {(sdkReady ? "Yes" : "No")}");
                Console.WriteLine();

                if (sdkReady)
                {
                    Console.WriteLine("✅ Complete workflow successful!");
                    Console.WriteLine("The Bink SDK is ready for video playback.");
                }
                else
                {
                    Console.WriteLine("⚠ SDK not ready despite DLL being available.");
                }
            }
            else
            {
                Console.WriteLine("❌ Could not ensure DLL availability.");
                Console.WriteLine("Possible reasons:");
                Console.WriteLine("- Mass Effect Legendary Edition not installed");
                Console.WriteLine("- RAD Video Tools not installed");
                Console.WriteLine("- Insufficient permissions");
            }

            Console.WriteLine();
            Console.WriteLine("🚀 Workflow demo completed!");
        }
    }
}