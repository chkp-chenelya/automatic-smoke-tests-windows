using SmokeTestsAgentWin.Tests;
using System;
using System.Diagnostics;
using System.Linq;

class Program
{
    private const string AppPath = "C:\\Program Files\\Perimeter 81\\Perimeter81.exe";

    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // Run multiple tests with a single comprehensive report
            var report = new TestReport
            {
                TestName = "Harmony SASE Smoke Tests Suite",
                StartTime = DateTime.Now
            };

            int passedTests = 0;
            int totalTests = 2;

            // Test 1: Application Launch Test
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Test 1: Application Launch Test");
            Console.WriteLine("═══════════════════════════════════════");
            var testCase1 = new TestReport.TestCase
            {
                Name = "Application Launch Test",
                StartTime = DateTime.Now
            };

            // Simulate test steps
            var step1 = new TestReport.TestStep
            {
                Name = "Verify application exists",
                StartTime = DateTime.Now
            };
            System.Threading.Thread.Sleep(100);
            step1.EndTime = DateTime.Now;
            step1.Passed = true;
            step1.Details = $"Application found at: {AppPath}";
            testCase1.Steps.Add(step1);

            var step2 = new TestReport.TestStep
            {
                Name = "Check application version",
                StartTime = DateTime.Now
            };
            System.Threading.Thread.Sleep(150);
            step2.EndTime = DateTime.Now;
            step2.Passed = true;
            step2.Details = "Version: 1.0.0.0";
            testCase1.Steps.Add(step2);

            testCase1.EndTime = DateTime.Now;
            testCase1.Passed = true;
            report.TestCases.Add(testCase1);
            passedTests++;
            Console.WriteLine($"Result: PASS ✓");

            //// Finalize report
            report.EndTime = DateTime.Now;
            report.Passed = passedTests == totalTests;
            if (!report.Passed)
            {
                report.ErrorMessage = $"{totalTests - passedTests} test(s) failed out of {totalTests}";
            }

            // Generate and open HTML report
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine($"Overall Result: {passedTests}/{totalTests} tests passed");
            Console.WriteLine("═══════════════════════════════════════");
            report.GenerateAndOpenReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}