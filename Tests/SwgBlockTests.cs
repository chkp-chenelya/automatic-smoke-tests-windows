using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using SmokeTestsAgentWin.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace SmokeTestsAgentWin.Tests
{
    /// <summary>
    /// Tests for SWG (Secure Web Gateway) blocking functionality.
    /// Verifies that the VPN properly blocks access to restricted websites.
    /// </summary>
    public static class SwgBlockTests
    {
        private const string LogPrefix = "[SwgBlockTests] ";
        private const string SupportButtonAutomationId = "QuickAccessSupportButton";
        private const string HarmonySaseMainWindowAutomationId = "HarmonySASEMainWindow";
        private const string MainWindowHomeButtonAutomationId = "MainWindowHomeButton";
        private const string HomeConnectButtonAutomationId = "HomeConnectButton";

        // Step names
        private const string Step1 = "Click Support button in Quick Access";
        private const string Step2 = "Wait for Support screen to load";
        private const string Step3 = "Click Home button";
        private const string Step4 = "Click Connect button to establish VPN";
        private const string Step5 = "Wait for VPN connection (button changes to Disconnect)";
        private const string Step6 = "Verify website is blocked by VPN";
        private const string Step7 = "Click Close button to close app";

        // Window and button names
        private const string HarmonySaseWindowName = "Harmony SASE";
        private const string DisconnectButtonName = "Disconnect";
        private const string CloseButtonAutomationId = "MainWindowCloseButton";

        // URLs and timeouts
        private const int MaxVpnConnectionWaitMs = 120000; // 2 minutes
        private const int VpnCheckIntervalMs = 2000;
        private const int SupportScreenTimeoutSeconds = 6;
        private const int HomePageLoadDelayMs = 2000;
        private const int AppCloseDelayMs = 1000;

        /// <summary>
        /// Runs the SWG block test and adds results to the report.
        /// Steps:
        /// 1. Click Support button in Quick Access
        /// 2. Wait for Support screen to load
        /// 3. Click Home button
        /// 4. Click Connect button to establish VPN
        /// 5. Wait for VPN connection (Disconnect button appears)
        /// 6. Verify website is blocked (https://www.888.com/ should be redirected to block page)
        /// </summary>
        /// <param name="quickAccessWindow">The Quick Access window</param>
        /// <param name="report">The test report to add steps to</param>
        /// <returns>True if all steps passed, false otherwise</returns>
        public static bool RunSwgBlockTestWithReport(Window quickAccessWindow, TestReport report)
        {
            var automation = quickAccessWindow.Automation;
            bool overallSuccess = true;

            // Step 1: Click Support button in Quick Access
            overallSuccess &= report.ExecuteStep(
                Step1,
                () => UIHelpers.FindAndClickButtonByAutomationId(quickAccessWindow, SupportButtonAutomationId, LogPrefix),
                "Successfully clicked Quit button",
                "Failed to find or click Quit button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 2: Wait for Support screen to load
            Window? harmonySaseWindow = null;
            overallSuccess &= report.ExecuteStep(
                Step2,
                () =>
                {
                    Thread.Sleep(HomePageLoadDelayMs);
                    harmonySaseWindow = UIHelpers.WaitForWindowByAutomationId(automation, HarmonySaseMainWindowAutomationId, SupportScreenTimeoutSeconds, LogPrefix);
                    return harmonySaseWindow != null;
                },
                "Support screen loaded successfully",
                "Timeout waiting for Support screen");

            if (!overallSuccess || harmonySaseWindow == null)
            {
               return false;
            }

            // Step 3: Click Home button
            overallSuccess &= report.ExecuteStep(
                Step3,
                () => ClickButtonInMainWindow(automation, MainWindowHomeButtonAutomationId),
                "Home button clicked successfully",
                "Failed to find or click Home button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 4: Click Connect button
            overallSuccess &= report.ExecuteStep(
                Step4,
                () => ClickConnectButton(automation),
                "Connect button clicked successfully",
                "Failed to find or click Connect button");

            if (!overallSuccess)
            {
                return false;
            } 

            // Step 5: Wait for VPN connection
            overallSuccess &= report.ExecuteStep(
                Step5,
                () => WaitForVpnConnection(automation),
                "VPN connected successfully",
                "Timeout: Disconnect button did not appear within 2 minutes");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 6: Verify website is blocked by VPN
            overallSuccess &= report.ExecuteStep(
                Step6,
                () => VerifyWebsiteBlocked(),
                "Website is blocked by VPN (verified via curl - 403 Forbidden)",
                "Failed to verify website blocking");

            if (!overallSuccess)
            {
               return false;
            }

            // Step 7: Click Close button to close the app
            overallSuccess &= report.ExecuteStep(
                Step7,
                () => CloseMainWindow(automation),
                "Close button clicked successfully, app closed",
                "Failed to find or click Close button");

            return overallSuccess;
        }

        /// <summary>
        /// Finds the Harmony SASE main window by automation ID.
        /// </summary>
        private static Window? FindMainWindow(AutomationBase automation)
        {
            var desktop = automation.GetDesktop();
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            return desktop.FindFirstChild(cf => cf.ByAutomationId(HarmonySaseMainWindowAutomationId)
                .And(cf.ByControlType(ControlType.Window)))?.AsWindow();
        }

        /// <summary>
        /// Finds the Tab -> Custom element structure used for VPN buttons.
        /// </summary>
        private static AutomationElement? FindTabCustomElement(Window window)
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var tabElement = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab));
            if (tabElement == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Tab element");
                return null;
            }

            var customElement = tabElement.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
            if (customElement == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Custom element under Tab");
                return null;
            }

            return customElement;
        }

        /// <summary>
        /// Clicks a button in the main window by automation ID.
        /// </summary>
        private static bool ClickButtonInMainWindow(AutomationBase automation, string buttonAutomationId)
        {
            var window = FindMainWindow(automation);
            if (window == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                return false;
            }

            Console.WriteLine($"{LogPrefix}Window found: {window.Name}, AutomationId: {window.AutomationId}");
            var result = UIHelpers.FindAndClickButtonByAutomationId(window, buttonAutomationId, LogPrefix);
            
            if (!result)
            {
                Console.WriteLine($"{LogPrefix}Failed to find button by AutomationId '{buttonAutomationId}'");
            }
            
            return result;
        }

        /// <summary>
        /// Clicks the Connect button to establish VPN connection.
        /// </summary>
        private static bool ClickConnectButton(AutomationBase automation)
        {
            Thread.Sleep(HomePageLoadDelayMs);

            var window = FindMainWindow(automation);
            if (window == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                return false;
            }

            var customElement = FindTabCustomElement(window);
            if (customElement == null) return false;

            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var connectButton = customElement.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Button).And(cf.ByAutomationId(HomeConnectButtonAutomationId)));

            if (connectButton == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Connect button with AutomationId '{HomeConnectButtonAutomationId}'");
                return false;
            }

            Console.WriteLine($"{LogPrefix}Found Connect button, attempting to click...");
            return UIHelpers.TryClickButton(connectButton, LogPrefix);
        }

        /// <summary>
        /// Waits for VPN connection by checking for the Disconnect button.
        /// </summary>
        private static bool WaitForVpnConnection(AutomationBase automation)
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < MaxVpnConnectionWaitMs)
            {
                var window = FindMainWindow(automation);
                if (window != null)
                {
                    var customElement = FindTabCustomElement(window);
                    if (customElement != null)
                    {
                        var disconnectButton = customElement.FindFirstDescendant(cf =>
                            cf.ByControlType(ControlType.Button).And(cf.ByName(DisconnectButtonName)));

                        if (disconnectButton != null)
                        {
                            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            Console.WriteLine($"{LogPrefix}VPN connected! Disconnect button appeared after {elapsedSeconds:F1} seconds");
                            return true;
                        }
                    }
                }

                Thread.Sleep(VpnCheckIntervalMs);
            }

            Console.WriteLine($"{LogPrefix}Timeout: Disconnect button did not appear within 2 minutes");
            return false;
        }

        /// <summary>
        /// Verifies that a website is blocked by the VPN using curl.
        /// </summary>
        public static bool VerifyWebsiteBlocked(string BlockedTestUrl = "https://www.888.com/", string LogPrefix = "[SwgBlockTests] ")
        {
            Console.WriteLine($"{LogPrefix}Testing HTTP request to {BlockedTestUrl} using curl to verify VPN blocking...");

            try
            {
                var curlProcess = new ProcessStartInfo
                {
                    FileName = "curl",
                    Arguments = $"-I {BlockedTestUrl}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(curlProcess))
                {
                    if (process == null)
                    {
                        Console.WriteLine($"{LogPrefix}Failed to start curl process");
                        return false;
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit(15000);

                    Console.WriteLine($"{LogPrefix}Curl output:\n{output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"{LogPrefix}Curl error: {error}");
                    }

                    // Check for 403 Forbidden (blocked)
                    if (output.Contains("403 Forbidden") || output.Contains("Firefly-Pep-Sessionid"))
                    {
                        Console.WriteLine($"{LogPrefix}✓ BLOCKED: Received 403 Forbidden response detected (Harmony SASE block)");
                        return true;
                    }
                    // Check for 200 OK (not blocked)
                    else if (output.Contains("200 OK") || output.Contains("HTTP/1.1 200"))
                    {
                        Console.WriteLine($"{LogPrefix}✗ NOT BLOCKED: Received 200 OK response - website is accessible");
                        return false;
                    }
                    // Connection errors mean blocked
                    else if (!string.IsNullOrEmpty(error) || output.Contains("Could not resolve host"))
                    {
                        Console.WriteLine($"{LogPrefix}✓ BLOCKED: Connection error");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"{LogPrefix}Unexpected response - check output above");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix}Error executing curl: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Closes the main Harmony SASE window.
        /// </summary>
        public static bool CloseMainWindow(AutomationBase automation)
        {
            var desktop = automation.GetDesktop();
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var window = desktop.FindFirstChild(cf => cf.ByName(HarmonySaseWindowName)
                .And(cf.ByControlType(ControlType.Window)))?.AsWindow();

            if (window == null)
            {
                Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                return false;
            }

            var result = UIHelpers.FindAndClickButtonByAutomationId(window, CloseButtonAutomationId, LogPrefix);
            
            if (result)
            {
                Console.WriteLine($"{LogPrefix}Close button clicked successfully");
                Thread.Sleep(AppCloseDelayMs);
            }
            else
            {
                Console.WriteLine($"{LogPrefix}Failed to find or click Close button");
            }
            
            return result;
        }
    }
}