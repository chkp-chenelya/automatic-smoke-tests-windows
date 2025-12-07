using SmokeTestsAgentWin.Tests;
using System;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        const string testSuiteName = "Harmony SASE Smoke Tests Suite";
        const string swgBlockTest = "SWG Block";
        
        
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
    }
}