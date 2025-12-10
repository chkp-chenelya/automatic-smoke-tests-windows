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
        private const string OnboardingWindowAutomationId = "OnboardingWindow";
        private const int MaxLaunchRetries = 3;
        private const int RetryDelayMs = 3000; // 3 seconds between retries

        public enum WindowType
        {
            QuickAccess,
            Onboarding
        }

        /// Launches the Harmony SASE application and returns the Quick Access window.
        public static Window LaunchHarmonySaseApp()
        {
            return LaunchHarmonySaseApp(WindowType.QuickAccess);
        }

        /// Launches the Harmony SASE application and returns the Onboarding window.
        public static Window LaunchHarmonySaseAppForOnboarding()
        {
            return LaunchHarmonySaseApp(WindowType.Onboarding);
        }

        /// Launches the Harmony SASE application and returns the specified window type.
        private static Window LaunchHarmonySaseApp(WindowType windowType)
        {
            string windowTypeName = windowType == WindowType.QuickAccess ? "Quick Access" : "Onboarding";
            Window? window = null;
            int attemptNumber = 0;

            while (attemptNumber < MaxLaunchRetries && window == null)
            {
                attemptNumber++;
                
                try
                {
                    if (attemptNumber > 1)
                    {
                        Console.WriteLine($"\nRetrying application launch (attempt {attemptNumber}/{MaxLaunchRetries})...");
                        Thread.Sleep(RetryDelayMs);
                    }

                    LaunchApplication();

                    Console.WriteLine($"Waiting for Perimeter81 {windowTypeName} window to start (max 2 minutes)...");
                    var automation = new UIA3Automation();
                    var stopwatch = Stopwatch.StartNew();

                    window = WaitForWindowWithExponentialBackoff(automation, stopwatch, windowType);

                    if (window != null)
                    {
                        Console.WriteLine($"Found {windowTypeName} window after {stopwatch.ElapsedMilliseconds}ms");
                        Console.WriteLine($"Working with window: {window.Name}");
                        return window;
                    }
                    else if (attemptNumber < MaxLaunchRetries)
                    {
                        Console.WriteLine($"Failed to find {windowTypeName} window on attempt {attemptNumber}. Will retry...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during launch attempt {attemptNumber}: {ex.Message}");
                    if (attemptNumber >= MaxLaunchRetries)
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException(
                $"Failed to find Harmony SASE {windowTypeName} window after {MaxLaunchRetries} attempts. " +
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

        private static Window? WaitForWindowWithExponentialBackoff(UIA3Automation automation, Stopwatch stopwatch, WindowType windowType)
        {
            int checkInterval = InitialCheckIntervalMs;

            while (stopwatch.ElapsedMilliseconds < StartupWaitTimeMs)
            {
                var window = windowType == WindowType.QuickAccess 
                    ? FindQuickAccessWindow(automation) 
                    : FindOnboardingWindow(automation);
                    
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

        /// Searches for the OnboardingWindow by AutomationId.
        private static Window? FindOnboardingWindow(UIA3Automation automation)
        {
            var desktop = automation.GetDesktop();
            var allWindows = desktop.FindAllChildren();

            // Search for OnboardingWindow by AutomationId
            var onboardingWindow = allWindows
                .FirstOrDefault(w => IsOnboardingWindow(w));

            if (onboardingWindow != null)
            {
                return onboardingWindow.AsWindow();
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

        private static bool IsOnboardingWindow(AutomationElement window)
        {
            try
            {
                return window.Properties.AutomationId == OnboardingWindowAutomationId;
            }
            catch
            {
                return false;
            }
        }
    }
}