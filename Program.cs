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
        const string swgBlockTest = "SWG Block Test";
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

            var mainWindow = ApplicationLauncher.LaunchHarmonySaseApp();

            // Test 1: SWG Block Test
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Test 1: SWG Block Test");
            Console.WriteLine("═══════════════════════════════════════");
            var testCase1 = new TestReport.TestCase
            {
                Name = swgBlockTest,
                StartTime = DateTime.Now
            };
            bool swgBlockSuccess = SwgBlockTests.RunSwgBlockTestWithReport(mainWindow, report);
            testCase1.EndTime = DateTime.Now;
            testCase1.Passed = swgBlockSuccess;
            testCase1.Steps = report.Steps.ToList();
            report.TestCases.Add(testCase1);
            report.Steps.Clear();
            totalTests++;
            if (swgBlockSuccess)
            {
                passedTests++;
            }
            Console.WriteLine($"Result: {(swgBlockSuccess ? "PASS ✓" : "FAIL ✗")}");

            //// Test 2: Quit From Quick Access Window
            //mainWindow = ApplicationLauncher.LaunchHarmonySaseApp();
            //Console.WriteLine("\n═══════════════════════════════════════");
            //Console.WriteLine("Test 2: Quit From Quick Access Window");
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
    }
}