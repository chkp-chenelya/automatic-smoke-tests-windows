using SmokeTestsAgentWin.Helpers;
using SmokeTestsAgentWin.Tests;
using System;
using System.Linq;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        const string testSuiteName = "Harmony SASE Smoke Tests Suite";
        const string quitFromHomePage = "Quit From Home Page";


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

            var mainWindow = ApplicationLauncher.LaunchHarmonySaseApp();

            // Test 1: Quit From Home Page
            mainWindow = ApplicationLauncher.LaunchHarmonySaseApp();
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Test 1: Quit From Home Page");
            Console.WriteLine("═══════════════════════════════════════");
            var testCase1 = new TestReport.TestCase
            {
                Name = quitFromHomePage,
                StartTime = DateTime.Now
            };
            bool quitHomeSuccess = QuitQuickAccessTests.RunQuitTestWithReport(mainWindow, report);
            testCase1.EndTime = DateTime.Now;
            testCase1.Passed = quitHomeSuccess;
            testCase1.Steps = report.Steps.ToList();
            report.TestCases.Add(testCase1);
            report.Steps.Clear();
            if (quitHomeSuccess) passedTests++;
            Console.WriteLine($"Result: {(quitHomeSuccess ? "PASS ✓" : "FAIL ✗")}");

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
    }
}