using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;
using System.Linq;

namespace SmokeTestsAgentWin.Tests
{
    public static class UIHelpers
    {
        private const int WindowSearchRetryIntervalMs = 500; // Interval between retry attempts when waiting for window
        
        /// <summary>
        /// Finds and clicks a button by AutomationId in one operation.
        /// </summary>
        /// <param name="parent">The parent element to search within.</param>
        /// <param name="automationId">The AutomationId of the button to find and click.</param>
        /// <param name="logPrefix">Optional prefix for log messages.</param>
        /// <returns>True if the button was found and clicked, false otherwise.</returns>
        public static bool FindAndClickButtonByAutomationId(AutomationElement parent, string automationId, string logPrefix = "")
        {
            Console.WriteLine($"{logPrefix}Searching for button with AutomationId '{automationId}'...");
            
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var button = parent.FindFirstDescendant(
                cf.ByControlType(ControlType.Button).And(cf.ByAutomationId(automationId)));

            if (button == null)
            {
                Console.WriteLine($"{logPrefix}Button with AutomationId '{automationId}' not found.");
                DumpAvailableButtons(parent, logPrefix);
                return false;
            }

            Console.WriteLine($"{logPrefix}Button with AutomationId '{automationId}' found.");
            return TryClickButton(button, logPrefix);
        }

        /// <summary>
        /// Waits for a window by AutomationId.
        /// </summary>
        /// <param name="automation">The automation instance.</param>
        /// <param name="automationId">The AutomationId of the window to find.</param>
        /// <param name="timeoutSeconds">Maximum time to wait in seconds.</param>
        /// <param name="logPrefix">Optional prefix for log messages.</param>
        /// <returns>The found window or null if timeout occurred.</returns>
        public static Window? WaitForWindowByAutomationId(AutomationBase automation, string automationId, int timeoutSeconds = 6, string logPrefix = "")
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            
            return WaitForWindow(automation, () =>
            {
                var desktop = automation.GetDesktop();
                var windowById = desktop
                    .FindAllChildren(cf.ByControlType(ControlType.Window))
                    .FirstOrDefault(w => w.Properties.AutomationId == automationId);
                
                if (windowById != null)
                {
                    Console.WriteLine($"{logPrefix}Window found by AutomationId '{automationId}'");
                    return windowById.AsWindow();
                }
                
                return null;
            }, timeoutSeconds, logPrefix);
        }

        /// <summary>
        /// Tries to click a button element using Invoke pattern first, then Click as fallback.
        /// </summary>
        /// <param name="element">The button element to click.</param>
        /// <param name="logPrefix">Optional prefix for log messages.</param>
        /// <returns>True if the button was clicked successfully, false otherwise.</returns>
        public static bool TryClickButton(AutomationElement element, string logPrefix = "")
        {
            // Try Invoke pattern first
            try
            {
                var invokePattern = element.Patterns.Invoke.Pattern;
                Console.WriteLine($"{logPrefix}Using Invoke pattern...");
                invokePattern.Invoke();
                return true;
            }
            catch (Exception invokeEx)
            {
                Console.WriteLine($"{logPrefix}Invoke pattern failed: {invokeEx.Message}, trying Click...");
                try
                {
                    Console.WriteLine($"{logPrefix}Using Click()...");
                    element.Click();
                    return true;
                }
                catch (Exception clickEx)
                {
                    Console.WriteLine($"{logPrefix}Click also failed: {clickEx.Message}");
                    return false;
                }
            }
        }

        private static void DumpAvailableButtons(AutomationElement parent, string logPrefix = "")
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            var allButtons = parent.FindAllDescendants(cf.ByControlType(ControlType.Button));
            
            Console.WriteLine($"{logPrefix}Available buttons ({allButtons.Length}):");
            foreach (var btn in allButtons)
            {
                try
                {
                    Console.WriteLine($"{logPrefix}  - Name: '{btn.Name}', AutomationId: '{btn.AutomationId}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{logPrefix}  - Error reading button: {ex.Message}");
                }
            }
        }

        private static Window? WaitForWindow(AutomationBase automation, Func<Window?> windowSearchFunc, int timeoutSeconds, string logPrefix)
        {
            Window? foundWindow = null;

            Console.WriteLine($"{logPrefix}Waiting for window (timeout: {timeoutSeconds}s)...");
            
            Retry.WhileTrue(() =>
            {
                foundWindow = windowSearchFunc();
                return foundWindow == null;
            }, TimeSpan.FromMilliseconds(WindowSearchRetryIntervalMs), TimeSpan.FromSeconds(timeoutSeconds));

            if (foundWindow != null)
            {
                Console.WriteLine($"{logPrefix}Window found: {foundWindow.Name}");
            }
            else
            {
                Console.WriteLine($"{logPrefix}Window not found after {timeoutSeconds}s timeout.");
            }

            return foundWindow;
        }
    }
}
