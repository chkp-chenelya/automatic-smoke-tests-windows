using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SmokeTestsAgentWin.Helpers
{
    /// Handles launching and connecting to the Harmony SASE application.
    public class ApplicationLauncher
    {
        private const string AppPath = "C:\\Program Files\\Perimeter 81\\Perimeter81.exe";
        private const int StartupWaitTimeMs = 120100; // 2 minutes
        private const int InitialCheckIntervalMs = 100;
        private const int MaxCheckIntervalMs = 2000;
        private const string QuickAccessWindowAutomationId = "QuickAccessWindow";

        /// Launches the Harmony SASE application and returns the main window.
        public static Window LaunchHarmonySaseApp()
        {
            LaunchApplication();

            Console.WriteLine("Waiting for Perimeter81 to start (max 2 minutes)...");
            var automation = new UIA3Automation();
            var stopwatch = Stopwatch.StartNew();

            var window = WaitForWindowWithExponentialBackoff(automation, stopwatch);

            if (window != null)
            {
                Console.WriteLine($"Found Quick Access window after {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Working with window: {window.Name}");
                return window;
            }

            throw new InvalidOperationException(
                $"Failed to find Harmony SASE application window after {StartupWaitTimeMs}ms. " +
                "Ensure the application is installed and can be launched.");
        }

        private static void LaunchApplication()
        {
            var startInfo = new ProcessStartInfo(AppPath)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }

        private static Window? WaitForWindowWithExponentialBackoff(UIA3Automation automation, Stopwatch stopwatch)
        {
            int checkInterval = InitialCheckIntervalMs;

            while (stopwatch.ElapsedMilliseconds < StartupWaitTimeMs)
            {
                var window = FindQuickAccessWindow(automation);
                if (window != null)
                {
                    return window;
                }

                Thread.Sleep(checkInterval);
                checkInterval = Math.Min(checkInterval * 2, MaxCheckIntervalMs);
            }

            return null;
        }

        /// Searches for the QuickAccessWindow by AutomationId.
        private static Window? FindQuickAccessWindow(UIA3Automation automation)
        {
            var desktop = automation.GetDesktop();
            var allWindows = desktop.FindAllChildren();

            // Search for QuickAccessWindow by AutomationId
            var quickAccessWindow = allWindows
                .FirstOrDefault(w => IsQuickAccessWindow(w));

            if (quickAccessWindow != null)
            {
                return quickAccessWindow.AsWindow();
            }

            return null;
        }

        private static bool IsQuickAccessWindow(AutomationElement window)
        {
            try
            {
                return window.Properties.AutomationId == QuickAccessWindowAutomationId;
            }
            catch
            {
                return false;
            }
        }
    }
}
