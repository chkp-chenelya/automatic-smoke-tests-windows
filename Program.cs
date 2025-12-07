using SmokeTestsAgentWin.Helpers;
using SmokeTestsAgentWin.Tests;
using System;
using System.Diagnostics;
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
    
    [STAThread]
    static void Main(string[] args)
    {
        const string testSuiteName = "Harmony SASE Smoke Tests Suite";
        const string swgBlockTest = "SWG Block";
        const string quitFromQuickAccessWindow = "Quit From Quick Access Window";


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
                testCase1.Passed = false;
                testCase1.ErrorMessage = testEx.Message;
                Console.WriteLine($"Test failed: {testEx.Message}");
            }
            finally
            {
                testCase1.EndTime = DateTime.Now;
                report.TestCases.Add(testCase1);
                totalTests++;
            }

            //// Test 2: Quit From Quick Access Window
            //var mainWindow = ApplicationLauncher.LaunchHarmonySaseApp();
            //Console.WriteLine("\n═══════════════════════════════════════");
            //Console.WriteLine($"Test 2: {quitFromQuickAccessWindow}");
            //Console.WriteLine("═══════════════════════════════════════");
            //var testCase2 = new TestReport.TestCase
            //{
            //    Name = quitFromQuickAccessWindow,
            //    StartTime = DateTime.Now
            //};
            //bool quitQuickAccessSuccess = QuitQuickAccessTests.RunQuitTestWithReport(mainWindow, report);
            //testCase2.EndTime = DateTime.Now;
            //testCase2.Passed = quitQuickAccessSuccess;
            //testCase2.Steps = report.Steps.ToList();
            //report.TestCases.Add(testCase2);
            //report.Steps.Clear();
            //totalTests++;
            //if (quitQuickAccessSuccess)
            //{
            //    passedTests++;
            //} 
            //Console.WriteLine($"Result: {(quitQuickAccessSuccess ? "PASS ✓" : "FAIL ✗")}");

            // Generate and open HTML report
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine($"Overall Result: {passedTests}/{totalTests} tests passed");
            Console.WriteLine("═══════════════════════════════════════");
            
            report.EndTime = DateTime.Now;
            report.Passed = passedTests == totalTests;
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
            EnsureApplicationClosed();
        }
    }

    /// <summary>
    /// Ensures the Harmony SASE application is closed, leaving the system in a clean state.
    /// Closes all related processes: main application, services, and helper processes.Ensure tests can run repeatedly.
    /// </summary>
    private static void EnsureApplicationClosed()
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
                            Console.WriteLine($"CloseMainWindow failed, killing process...");
                            process.Kill();
                            process.WaitForExit(ProcessExitTimeoutMs);
                        }
                        else
                        {
                            Console.WriteLine($"Graceful close initiated, waiting for exit...");
                            process.WaitForExit(ProcessExitTimeoutMs);
                        }
                        
                        if (!process.HasExited)
                        {
                            Console.WriteLine($"Process still running, forcing termination...");
                            process.Kill();
                            process.WaitForExit(ProcessExitTimeoutMs);
                        }
                        
                        Console.WriteLine($"Process closed successfully.");
                    }
                    catch (Exception processEx)
                    {
                        Console.WriteLine($"Error closing process: {processEx.Message}");
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in EnsureApplicationClosed: {ex.Message}");
        }
    }
}