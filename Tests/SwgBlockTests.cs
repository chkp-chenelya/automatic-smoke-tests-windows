using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Linq;
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
        private const string Step1Name = "Click Support button in Quick Access";
        private const string Step2Name = "Wait for Support screen to load";
        private const string Step3Name = "Click Home button";
        private const string Step4Name = "Click Connect button to establish VPN";
        private const string Step5Name = "Wait for VPN connection (button changes to Disconnect)";
        private const string Step6Name = "Verify website is blocked by VPN";
        private const string Step7Name = "Click empty button to close app";
        
        // Window and button names
        private const string HarmonySaseWindowName = "Harmony SASE";
        private const string HomeButtonName = "Home";
        private const string ConnectButtonName = "Connect";
        private const string DisconnectButtonName = "Disconnect";
        
        // URLs and timeouts
        private const string BlockedTestUrl = "https://www.888.com/";
        private const int MaxVpnConnectionWaitMs = 120000; // 2 minutes
        private const int VpnCheckIntervalMs = 2000;
        private const int BrowserVerificationWaitMs = 10000;
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
            var automation = new UIA3Automation();
            bool overallSuccess = true;

            // Step 1: Click Support button in Quick Access
            overallSuccess &= report.ExecuteStep(
                Step1Name,
                () => UIHelpers.FindAndClickButtonByAutomationId(quickAccessWindow, SupportButtonAutomationId, LogPrefix),
                "Support button clicked successfully",
                "Failed to find or click Support button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 2: Wait for Support screen to load (find Harmony SASE window)
            Window? harmonySaseWindow = null;
            overallSuccess &= report.ExecuteStep(
                Step2Name,
                () =>
                {
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
                Step3Name,
                () =>
                {
                    var desktop = automation.GetDesktop();
                    var cf = new ConditionFactory(new UIA3PropertyLibrary());
                    var window = desktop.FindFirstChild(cf => cf.ByAutomationId(HarmonySaseMainWindowAutomationId).And(cf.ByControlType(ControlType.Window)))?.AsWindow();
                    
                    if (window == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                        return false;
                    }
                    
                    // Dump UI tree for debugging
                    Console.WriteLine($"{LogPrefix}Dumping Harmony SASE window structure before clicking Home:");
                    UITreeDumper.DumpUITree(window, LogPrefix, 0, 6);
                    
                    return UIHelpers.FindAndClickButtonByAutomationId(window, MainWindowHomeButtonAutomationId, LogPrefix);
                },
                "Home button clicked successfully",
                "Failed to find or click Home button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 4: Click Connect button
            overallSuccess &= report.ExecuteStep(
                Step4Name,
                () =>
                {
                    Thread.Sleep(HomePageLoadDelayMs); // Wait for Home page to load
                    
                    var desktop = automation.GetDesktop();
                    var cf = new ConditionFactory(new UIA3PropertyLibrary());
                    var window = desktop.FindFirstChild(cf => cf.ByAutomationId(HarmonySaseMainWindowAutomationId).And(cf.ByControlType(ControlType.Window)))?.AsWindow();
                    
                    if (window == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                        return false;
                    }
                    
                    // Navigate: Window -> Tab -> Custom -> Button with AutomationId
                    var tabElement = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab));
                    if (tabElement == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Tab element");
                        return false;
                    }
                    
                    var customElement = tabElement.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
                    if (customElement == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Custom element under Tab");
                        return false;
                    }
                    
                    var connectButton = customElement.FindFirstDescendant(cf => 
                        cf.ByControlType(ControlType.Button).And(cf.ByAutomationId(HomeConnectButtonAutomationId)));
                    
                    if (connectButton == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Connect button with AutomationId '{HomeConnectButtonAutomationId}'");
                        return false;
                    }
                    
                    Console.WriteLine($"{LogPrefix}Found Connect button, attempting to invoke...");
                    
                    // Try Invoke pattern first
                    try
                    {
                        var invokePattern = connectButton.Patterns.Invoke.Pattern;
                        invokePattern.Invoke();
                        Console.WriteLine($"{LogPrefix}Connect button invoked successfully");
                        return true;
                    }
                    catch (Exception invokeEx)
                    {
                        Console.WriteLine($"{LogPrefix}Invoke failed: {invokeEx.Message}, trying Click...");
                        try
                        {
                            connectButton.Click();
                            Console.WriteLine($"{LogPrefix}Connect button clicked");
                            return true;
                        }
                        catch (Exception clickEx)
                        {
                            Console.WriteLine($"{LogPrefix}Click also failed: {clickEx.Message}");
                            return false;
                        }
                    }
                },
                "Connect button clicked successfully",
                "Failed to find or click Connect button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 5: Wait for VPN connection (button changes to Disconnect)
            overallSuccess &= report.ExecuteStep(
                Step5Name,
                () =>
                {
                    var cf = new ConditionFactory(new UIA3PropertyLibrary());
                    var stopwatch = Stopwatch.StartNew();

                    while (stopwatch.ElapsedMilliseconds < MaxVpnConnectionWaitMs)
                    {
                        var desktop = automation.GetDesktop();
                        var freshWindow = desktop
                            .FindAllChildren(cf.ByControlType(ControlType.Window))
                            .FirstOrDefault(w => w.Name == HarmonySaseWindowName)?.AsWindow();

                        if (freshWindow != null)
                        {
                            var tabElement = freshWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab));
                            if (tabElement != null)
                            {
                                var customElement = tabElement.FindFirstDescendant(cf => cf.ByControlType(ControlType.Custom));
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
                        }

                        Thread.Sleep(VpnCheckIntervalMs); // Check every 2 seconds
                    }
                    
                    Console.WriteLine($"{LogPrefix}Timeout: Disconnect button did not appear within 2 minutes");
                    return false;
                },
                $"VPN connected successfully",
                "Timeout: Disconnect button did not appear within 2 minutes");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 6: Verify website is blocked by VPN
            overallSuccess &= report.ExecuteStep(
                Step6Name,
                () =>
                {
                    Console.WriteLine($"{LogPrefix}Opening browser to {BlockedTestUrl} to verify VPN blocking...");
                    
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = BlockedTestUrl,
                        UseShellExecute = true
                    };
                    
                    Process.Start(processStartInfo);
                    
                    // Wait for browser to load and user to verify
                    Console.WriteLine($"{LogPrefix}Browser opened. Waiting {BrowserVerificationWaitMs / 1000} seconds to check for blocked page redirect...");
                    Thread.Sleep(BrowserVerificationWaitMs);
                    
                    Console.WriteLine($"{LogPrefix}Please manually verify: If you see a 'blocked' page, VPN is working. If 888.com loaded, VPN is NOT blocking.");
                    return true;
                },
                "Browser opened to test URL. Manual verification required: Test PASSES if redirected to blocked page, FAILS if 888.com loads directly.",
                "Failed to open browser");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 7: Click empty button to close the app
            overallSuccess &= report.ExecuteStep(
                Step7Name,
                () =>
                {
                    var desktop = automation.GetDesktop();
                    var cf = new ConditionFactory(new UIA3PropertyLibrary());
                    var window = desktop.FindFirstChild(cf => cf.ByName(HarmonySaseWindowName).And(cf.ByControlType(ControlType.Window)))?.AsWindow();
                    
                    if (window == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find Harmony SASE window");
                        return false;
                    }
                    
                    // Find the empty button (the one without text, after the other buttons)
                    var allButtons = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
                    var emptyButton = allButtons.FirstOrDefault(b => string.IsNullOrEmpty(b.Name));
                    
                    if (emptyButton == null)
                    {
                        Console.WriteLine($"{LogPrefix}Could not find empty button");
                        return false;
                    }
                    
                    emptyButton.Click();
                    Console.WriteLine($"{LogPrefix}Empty button clicked to close app");
                    Thread.Sleep(AppCloseDelayMs); // Wait for app to close
                    return true;
                },
                "Empty button clicked successfully, app closed",
                "Failed to find or click empty button");

            return overallSuccess;
        }
    }
}
