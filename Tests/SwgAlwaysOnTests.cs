using FlaUI.Core.AutomationElements;
using System;
using System.Threading;

namespace SmokeTestsAgentWin.Tests
{
    public static class SwgAlwaysOnTests
    {
        private const string LogPrefix = "[SwgAlwaysOn] ";
        private const string QuickAccessSignOutButtonAutomationId = "QuickAccessSignOutButton";
        private const string SignOutConfirmationDialogAutomationId = "SignOutConfirmationDialog";
        private const string SignOutConfirmButtonAutomationId = "SignOutConfirmButton";
        
        private const int InitialDialogWaitMs = 2000;
        private const int ConfirmationDialogTimeoutSeconds = 6;

        // Step names
        private const string Step1 = "Click Sign Out button in Quick Access";
        private const string Step2 = "Wait for Sign Out confirmation dialog";
        private const string Step3 = "Click Sign Out in confirmation dialog";
        private const string Step4 = "Verify website is blocked by VPN";

        /// <summary>
        /// Runs the SWG Always On test and adds results to the report.
        /// </summary>
        /// <param name="quickAccessWindow">The Quick Access window</param>
        /// <param name="report">The test report to add steps to</param>
        /// <returns>True if all steps passed, false otherwise</returns>
        public static bool RunSwgAlwaysOnTestWithReport(Window quickAccessWindow, TestReport report)
        {
            var automation = quickAccessWindow.Automation;
            bool overallSuccess = true;

            // Step 1: Click Sign Out button in Quick Access
            overallSuccess &= report.ExecuteStep(
                Step1,
                () => UIHelpers.FindAndClickButtonByAutomationId(quickAccessWindow, QuickAccessSignOutButtonAutomationId, LogPrefix),
                "Successfully clicked Sign Out button",
                "Failed to find or click Sign Out button");

            if (!overallSuccess)
            {
                return false;
            } 

            // Step 2: Wait for Sign Out confirmation dialog
            Window? dialogWindow = null;
            overallSuccess &= report.ExecuteStep(
                Step2,
                () =>
                {
                    Console.WriteLine($"{LogPrefix}Waiting for Sign Out confirmation dialog...");
                    Thread.Sleep(InitialDialogWaitMs);
                    dialogWindow = UIHelpers.WaitForWindowByAutomationId(
                        automation,
                        SignOutConfirmationDialogAutomationId,
                        ConfirmationDialogTimeoutSeconds,
                        LogPrefix);
                    return dialogWindow != null;
                },
                "Sign Out confirmation dialog appeared",
                "Sign Out confirmation dialog did not appear within timeout");

            if (!overallSuccess || dialogWindow == null)
            {
                return false;
            } 

            // Step 3: Click Sign Out button in confirmation dialog
            Console.WriteLine($"{LogPrefix}Sign Out confirmation dialog verified.");
            overallSuccess &= report.ExecuteStep(
                Step3,
                () => UIHelpers.FindAndClickButtonByAutomationId(dialogWindow, SignOutConfirmButtonAutomationId, LogPrefix),
                "Successfully clicked Sign Out in confirmation dialog",
                "Failed to find or click Sign Out button in dialog");

            if (!overallSuccess)
            {
                return false;
            }
            
            // Step 4: Verify website is blocked by VPN
            overallSuccess &= report.ExecuteStep(
                Step4,
                () => SwgBlockTests.VerifyWebsiteBlocked(logPrefix: LogPrefix),
                "Website is blocked by VPN (verified via curl - 403 Forbidden)",
                "Failed to verify website blocking");

            if (!overallSuccess)
            {
                return false;
            } 

            return overallSuccess;
        }
    }
}
