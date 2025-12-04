using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Linq;
using System;
using System.Threading;

namespace SmokeTestsAgentWin.Tests
{
    public class QuitQuickAccessTests
    {
        private const string LogPrefix = "[Quit] ";
        private const int InitialDialogWaitMs = 2000;
        private const int ConfirmationDialogTimeoutSeconds = 6;

        private const string QuitButtonAutomationId = "QuickAccessQuitButton";
        private const string QuitConfirmButtonAutomationId = "QuitConfirmButton";
        private const string ConfirmationDialogAutomationId = "QuitConfirmationDialog";

        private const string Step1 = "Click Quit button in Quick Access";
        private const string Step2 = "Wait for confirmation dialog";
        private const string Step3 = "Click Quit in confirmation dialog";

        public static bool RunQuitTestWithReport(Window quickAccessWindow, TestReport report)
        {
            var automation = quickAccessWindow.Automation;
            bool overallSuccess = true;

            // Step 1: Click initial Quit button From Quick Access
            overallSuccess &= report.ExecuteStep(
                Step1,
                () => UIHelpers.FindAndClickButtonByAutomationId(quickAccessWindow, QuitButtonAutomationId, LogPrefix),
                "Successfully clicked Quit button",
                "Failed to find or click Quit button");

            if (!overallSuccess)
            {
                return false;
            }

            // Step 2: Wait for confirmation dialog
            Window? dialogWindow = null;
            overallSuccess &= report.ExecuteStep(
                Step2,
                () =>
                {
                    Console.WriteLine($"{LogPrefix}Waiting for confirmation dialog...");
                    Thread.Sleep(InitialDialogWaitMs);
                    dialogWindow = UIHelpers.WaitForWindowByAutomationId(
                        automation,
                        ConfirmationDialogAutomationId,
                        ConfirmationDialogTimeoutSeconds,
                        LogPrefix);
                    return dialogWindow != null;
                },
                "Confirmation dialog appeared",
                "Confirmation dialog did not appear within timeout");

            if (!overallSuccess || dialogWindow == null)
            {
                return false;
            }

            // Step 3: Click Quit button in confirmation dialog
            Console.WriteLine($"{LogPrefix}Confirmation dialog verified.");
            overallSuccess &= report.ExecuteStep(
                Step3,
                () => UIHelpers.FindAndClickButtonByAutomationId(dialogWindow, QuitConfirmButtonAutomationId, LogPrefix),
                "Successfully clicked Quit in confirmation dialog",
                "Failed to find or click Quit button in dialog");

            if (overallSuccess)
            {
                Console.WriteLine($"{LogPrefix}Quit flow completed successfully.");
            }

            return overallSuccess;
        }
    }
}
