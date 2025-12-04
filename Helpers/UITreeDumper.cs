using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace SmokeTestsAgentWin.Tests
{
    /// <summary>
    /// Utility class for dumping UI tree structures for debugging.
    /// </summary>
    public static class UITreeDumper
    {
        /// <summary>
        /// Dumps all windows on the desktop with detailed information.
        /// For Harmony SASE windows, also dumps their complete UI tree.
        /// </summary>
        public static void DumpAllWindowsWithDetails(AutomationBase automation, ConditionFactory cf, string logPrefix)
        {
            try
            {
                var desktop = automation.GetDesktop();
                var windows = desktop.FindAllChildren(cf.ByControlType(ControlType.Window));
                Console.WriteLine($"{logPrefix}Dumping all {windows.Length} windows on desktop:");
                for (int i = 0; i < windows.Length; i++)
                {
                    var win = windows[i];
                    try
                    {
                        string name = win.Name ?? "(null)";
                        string autoId = "(not supported)";
                        try
                        {
                            autoId = win.AutomationId ?? "(null)";
                        }
                        catch { }
                        
                        Console.WriteLine($"{logPrefix}  [{i}] Window Name='{name}' AutoId='{autoId}'");
                        
                        // If this is Harmony SASE window, dump its tree
                        if (name.Contains("Harmony SASE"))
                        {
                            Console.WriteLine($"{logPrefix}    *** This is a Harmony SASE window! Dumping its structure ***");
                            DumpUITree(win, logPrefix + "    ", 0, 6);
                        }
                    }
                    catch (Exception winEx)
                    {
                        Console.WriteLine($"{logPrefix}  [{i}] Error reading window: {winEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{logPrefix}Error dumping windows: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively dumps the entire UI tree structure.
        /// </summary>
        public static void DumpUITree(AutomationElement element, string logPrefix, int depth = 0, int maxDepth = 10)
        {
            if (depth > maxDepth) return;

            string indent = new string(' ', depth * 2);
            try
            {
                string name = string.IsNullOrEmpty(element.Name) ? "\"\"" : $"\"{element.Name}\"";
                string automationId = string.IsNullOrEmpty(element.AutomationId) ? "\"\"" : $"\"{element.AutomationId}\"";
                string className = string.IsNullOrEmpty(element.ClassName) ? "\"\"" : $"\"{element.ClassName}\"";
                
                Console.WriteLine($"{logPrefix}{indent}{element.ControlType} Name={name} AutoId={automationId} ClassName={className}");

                // Get all children and recurse
                var children = element.FindAllChildren();
                if (children != null && children.Length > 0)
                {
                    foreach (var child in children)
                    {
                        DumpUITree(child, logPrefix, depth + 1, maxDepth);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{logPrefix}{indent}[Error reading element: {ex.Message}]");
            }
        }
    }
}
