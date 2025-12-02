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
            int totalTests = 0;

            // Test 1
            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Test 1: Test");
            Console.WriteLine("═══════════════════════════════════════");
            var testCase1 = new TestReport.TestCase
            {
                Name = "Test",
                StartTime = DateTime.Now
            };

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