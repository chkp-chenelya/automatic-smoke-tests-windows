using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Threading;

namespace SmokeTestsAgentWin.Helpers
{
    public class ApplicationLauncher
    {
        private const string AppPath = "C:\\Program Files\\Perimeter 81\\Perimeter81.exe";
        private const int StartupWaitTimeMs = 120100; // 2 minutes

        private static readonly string[] ProcessNames = {
            "Perimeter81.Service",
            "Harmony SASE",
            "Perimeter81.HelperService",
            "Perimeter81"
        };

        /// Launches the Harmony SASE application and returns the main window.
        public static Window LaunchHarmonySaseApp()
        {
            // Launch the application
            var startInfo = new ProcessStartInfo(AppPath)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);

            Console.WriteLine("Waiting for Perimeter81 to start (max 2 minutes)...");
            var automation = new UIA3Automation();
            var stopwatch = Stopwatch.StartNew();

            // Use exponential backoff for more efficient polling
            int checkInterval = 100; // Start with 100ms
            const int maxInterval = 2000; // Max 2 seconds

            while (stopwatch.ElapsedMilliseconds < StartupWaitTimeMs)
            {
                var window = FindQuickAccessWindow(automation);
                if (window != null)
                {
                    Console.WriteLine($"Found Quick Access window after {stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine($"Working with window: {window.Name}");
                    return window;
                }

                // Exponential backoff: check more frequently at start, less frequently later
                Thread.Sleep(checkInterval);
                if (checkInterval < maxInterval)
                {
                    checkInterval = Math.Min(checkInterval * 2, maxInterval);
                }
            }

            throw new InvalidOperationException(
                $"Failed to find Harmony SASE application window after {StartupWaitTimeMs}ms. " +
                "Ensure the application is installed and can be launched.");
        }

        /// Searches for the QuickAccessWindow across all known process names.
        private static Window? FindQuickAccessWindow(UIA3Automation automation)
        {
            var desktop = automation.GetDesktop();

            foreach (var processName in ProcessNames)
            {
                var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));

                foreach (var process in processes)
                {
                    try
                    {
                        // Wait briefly for process to initialize its main window
                        if (process.MainWindowHandle == IntPtr.Zero)
                        {
                            process.WaitForInputIdle(500);
                        }

                        // Find windows belonging to this process
                        var allWindows = desktop.FindAllChildren();
                        foreach (var window in allWindows)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(window.Name) && window.Name == "QuickAccessWindow")
                                {
                                    return window.AsWindow();
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return null;
        }
    }
}
