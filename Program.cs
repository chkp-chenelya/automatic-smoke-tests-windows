using FlaUI.Core.AutomationElements;
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
        const string swgAlwaysOnTest = "SWG Always On";
        const string quitFromQuickAccessWindow = "Quit From Quick Access Window";

        try
        {
            var report = new TestReport
            {
                TestName = testSuiteName,
                StartTime = DateTime.Now
            };

            int passedTests = 0;
            int totalTests = 0;

            // Test 1: SWG Block Test
            passedTests += RunTest(
                testName: swgBlockTest,
                testNumber: 1,
                report: report,
                testRunner: (mainWindow) => SwgBlockTests.RunSwgBlockTestWithReport(mainWindow, report),
                ref totalTests
            );

            // Test 2: SWG Always On Test
            passedTests += RunTest(
                testName: swgAlwaysOnTest,
                testNumber: 2,
                report: report,
                testRunner: (mainWindow) => SwgAlwaysOnTests.RunSwgAlwaysOnTestWithReport(mainWindow, report),
                ref totalTests
            );

            // Test 3: Quit From Quick Access Window
            passedTests += RunTest(
                testName: quitFromQuickAccessWindow,
                testNumber: 3,
                report: report,
                testRunner: (mainWindow) => QuitOnboardingTests.RunQuitTestWithReport(mainWindow, report),
                ref totalTests,
                useOnboardingWindow: true);

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
    /// Runs a single test and updates the test report.
    /// </summary>
    /// <param name="testName">The name of the test to run</param>
    /// <param name="testNumber">The sequential number of the test</param>
    /// <param name="report">The test report to update</param>
    /// <param name="testRunner">The function that executes the test</param>
    /// <param name="totalTests">Reference to the total test counter</param>
    /// <param name="useOnboardingWindow">If true, waits for onboarding window instead of quick access</param>
    /// <returns>1 if the test passed, 0 otherwise</returns>
    private static int RunTest(
        string testName,
        int testNumber,
        TestReport report,
        Func<Window, bool> testRunner,
        ref int totalTests,
        bool useOnboardingWindow = false)
    {
        var mainWindow = useOnboardingWindow 
            ? ApplicationLauncher.LaunchHarmonySaseAppForOnboarding()
            : ApplicationLauncher.LaunchHarmonySaseApp();
        Console.WriteLine("\n═══════════════════════════════════════");
        Console.WriteLine($"Test {testNumber}: {testName}");
        Console.WriteLine("═══════════════════════════════════════");

        var testCase = new TestReport.TestCase
        {
            Name = testName,
            StartTime = DateTime.Now
        };

        bool testPassed = false;

        try
        {
            testPassed = testRunner(mainWindow);
            testCase.Passed = testPassed;
            testCase.Steps = report.Steps.ToList();
        }
        catch (Exception testEx)
        {
            testCase.Passed = false;
            testCase.ErrorMessage = testEx.Message;
            Console.WriteLine($"Test failed: {testEx.Message}");
        }
        finally
        {
            testCase.EndTime = DateTime.Now;
            report.TestCases.Add(testCase);
            report.Steps.Clear();
            totalTests++;
        }

        Console.WriteLine($"Result: {(testPassed ? "PASS ✓" : "FAIL ✗")}");
        return testPassed ? 1 : 0;
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