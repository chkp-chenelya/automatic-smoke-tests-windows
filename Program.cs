using SmokeTestsAgentWin.Tests;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    private static readonly string[] HarmonySaseProcessNames = new[]
    {
        "Perimeter81",           // Main application
        "Perimeter81.Service",   // Background service
        "Perimeter81.HelperService" // Helper service
    };
    
    private const int ProcessExitTimeoutMs = 5000; // 5 seconds to wait for process to exit
    private const string AppPath = "C:\\Program Files\\Perimeter 81\\Perimeter81.exe";
    
    [STAThread]
    static void Main(string[] args)
    {
        const string testSuiteName = "Harmony SASE Smoke Tests Suite";
        const string swgBlockTest = "SWG Block";

        // Check if Perimeter81 was already installed before we started
        bool wasInstalledInitially = File.Exists(AppPath);
        Console.WriteLine($"Initial state: Perimeter81 {(wasInstalledInitially ? "already installed" : "not installed")}");


        try
        {
            // Run multiple tests with a single comprehensive report
            var report = new TestReport
            {
                TestName = testSuiteName,
                StartTime = DateTime.Now
            };

            int passedTests = 0;
            int totalTests = 0;

            // Test 1: SWG Block Test
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine($"Test 1: {swgBlockTest}");
            Console.WriteLine("═══════════════════════════════════════");
            var testCase1 = new TestReport.TestCase
            {
                Name = swgBlockTest,
                StartTime = DateTime.Now
            };

            try
            {
                SwgBlockTests.RunTest(testCase1);
                passedTests++;
            }
            catch (Exception testEx)
            {
                testCase1.Success = false;
                testCase1.ErrorMessage = testEx.Message;
                Console.WriteLine($"Test failed: {testEx.Message}");
            }
            finally
            {
                testCase1.EndTime = DateTime.Now;
                report.TestCases.Add(testCase1);
                totalTests++;
            }

            // Generate and open HTML report
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine($"Overall Result: {passedTests}/{totalTests} tests passed");
            Console.WriteLine("═══════════════════════════════════════");
            
            report.EndTime = DateTime.Now;
            report.GenerateAndOpenReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            // Ensure application is closed whether tests pass or fail
            EnsureApplicationClosed(wasInstalledInitially);
        }
    }

    /// <summary>
    /// Ensures the Harmony SASE application is closed, leaving the system in a clean state.
    /// Closes all related processes: main application, services, and helper processes.
    /// Ensure tests can run repeatedly.
    /// </summary>
    /// <param name="wasInstalledInitially">Whether Perimeter81 was installed before tests started</param>
    private static void EnsureApplicationClosed(bool wasInstalledInitially)
    {
        try
        {
            Console.WriteLine("\nEnsuring all Harmony SASE processes are closed...");
            bool anyProcessFound = false;
            
            foreach (var processName in HarmonySaseProcessNames)
            {
                var processes = Process.GetProcessesByName(processName);
                
                if (processes.Length == 0)
                {
                    continue;
                }
                
                anyProcessFound = true;
                Console.WriteLine($"Found {processes.Length} instance(s) of {processName}");
                
                foreach (var process in processes)
                {
                    try
                    {
                        Console.WriteLine($"  Closing {process.ProcessName} (PID: {process.Id})...");
                        
                        // Try graceful close first
                        if (!process.CloseMainWindow())
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(ProcessExitTimeoutMs);
                            }
                            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5) // ERROR_ACCESS_DENIED
                            {
                                // Service process requires admin privileges, silently skip
                                continue;
                            }
                        }
                        else
                        {
                            process.WaitForExit(ProcessExitTimeoutMs);
                        }
                        
                        if (!process.HasExited)
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(ProcessExitTimeoutMs);
                            }
                            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5) // ERROR_ACCESS_DENIED
                            {
                                // Service process requires admin privileges, silently skip
                                continue;
                            }
                        }
                        
                        Console.WriteLine($"    Process closed successfully.");
                    }
                    catch (Exception processEx)
                    {
                        // Only log unexpected errors, not access denied
                        if (!(processEx is System.ComponentModel.Win32Exception win32Ex && win32Ex.NativeErrorCode == 5))
                        {
                            Console.WriteLine($"    Error closing process: {processEx.Message}");
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            
            if (!anyProcessFound)
            {
                Console.WriteLine("No Harmony SASE processes found running.");
            }
            else
            {
                Console.WriteLine("All Harmony SASE processes have been closed.");
            }
            
            // If Perimeter81 wasn't installed before tests, uninstall it to restore original state
            if (!wasInstalledInitially && File.Exists(AppPath))
            {
                Console.WriteLine("\nRestoring original state: Uninstalling Perimeter81...");
                UninstallPerimeter81();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in EnsureApplicationClosed: {ex.Message}");
        }
    }
    
    private static void UninstallPerimeter81()
    {
        try
        {
            // Use Windows uninstall via registry or WMI
            var uninstallProcess = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "product where name='Perimeter 81' call uninstall",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(uninstallProcess))
            {
                process?.WaitForExit(30000); // Wait up to 30 seconds for uninstall
                Console.WriteLine("Uninstall completed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to uninstall Perimeter81: {ex.Message}");
            Console.WriteLine("You may need to uninstall manually.");
        }
    }
}