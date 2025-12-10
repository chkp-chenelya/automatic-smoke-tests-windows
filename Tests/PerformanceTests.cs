using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SmokeTestsAgentWin.Tests
{
    /// <summary>
    /// Tests for performance metrics of the Harmony SASE application.
    /// Verifies CPU and memory usage are within acceptable thresholds.
    /// </summary>
    public static class PerformanceTests
    {
        private const string LogPrefix = "[PerformanceTests] ";
        private const string ProcessName = "Perimeter81";

        // Step names
        private const string Step1 = "Wait for application to stabilize";
        private const string Step2 = "Verify CPU usage < 10%";
        private const string Step3 = "Verify Memory usage < 200MB";
        private const string Step4 = "Close main window";

        // Thresholds
        private const double MaxCpuPercentage = 10.0;
        private const double MaxMemoryMB = 200.0;
        private const int StabilizationDelayMs = 5000; // 5 seconds
        private const int MeasurementDurationMs = 3000; // 3 seconds
        private const int SampleIntervalMs = 500;

        /// <summary>
        /// Runs the performance test and adds results to the report.
        /// </summary>
        public static bool RunPerformanceTestWithReport(Window mainWindow, TestReport report)
        {
            Console.WriteLine($"\n{LogPrefix}Starting performance test...");

            try
            {
                // Step 1: Wait for application to stabilize
                bool step1Success = report.ExecuteStep(
                    Step1, 
                    () => WaitForStabilization(),
                    "Application stabilized successfully",
                    "Failed to stabilize application");
                if (!step1Success) return false;

                // Step 2: Verify CPU usage
                string cpuDetails = "";
                bool step2Success = report.ExecuteStep(
                    Step2, 
                    () => VerifyCpuUsage(out cpuDetails),
                    cpuDetails,
                    cpuDetails);
                if (!step2Success) return false;

                // Step 3: Verify Memory usage
                string memoryDetails = "";
                bool step3Success = report.ExecuteStep(
                    Step3, 
                    () => VerifyMemoryUsage(out memoryDetails),
                    memoryDetails,
                    memoryDetails);
                if (!step3Success) return false;

                //// Step 4: Close main window
                //var automation = new UIA3Automation();
                //bool step4Success = report.ExecuteStep(
                //    Step4,
                //    () => SwgBlockTests.CloseMainWindow(automation),
                //    "Main window closed successfully",
                //    "Failed to close main window");
                //if (!step4Success) return false;

                Console.WriteLine($"{LogPrefix}Performance test completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix}Unexpected error: {ex.Message}");
                return false;
            }
        }

        private static bool WaitForStabilization()
        {
            Console.WriteLine($"{LogPrefix}Waiting {StabilizationDelayMs}ms for application to stabilize...");
            Thread.Sleep(StabilizationDelayMs);
            return true;
        }

        private static bool VerifyCpuUsage(out string details)
        {
            var cpuDetails = new System.Text.StringBuilder();
            
            try
            {
                var processes = Process.GetProcessesByName(ProcessName);
                if (processes.Length == 0)
                {
                    Console.WriteLine($"{LogPrefix}ERROR: Process '{ProcessName}' not found");
                    details = "ERROR: Process not found";
                    return false;
                }

                double totalCpu = 0;
                cpuDetails.AppendLine($"CPU Usage Details:");
                
                Console.WriteLine($"{LogPrefix}Measuring CPU usage over {MeasurementDurationMs}ms...");

                foreach (var process in processes)
                {
                    var startTime = DateTime.UtcNow;
                    var startCpuTime = process.TotalProcessorTime;

                    Thread.Sleep(MeasurementDurationMs);

                    var endTime = DateTime.UtcNow;
                    var endCpuTime = process.TotalProcessorTime;

                    var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
                    var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                    var cpuPercentage = cpuUsageTotal * 100;

                    totalCpu += cpuPercentage;
                    Console.WriteLine($"{LogPrefix}Process ID {process.Id}: CPU = {cpuPercentage:F2}%");
                    cpuDetails.AppendLine($"  Process ID {process.Id}: {cpuPercentage:F2}%");
                }

                cpuDetails.AppendLine($"Total CPU: {totalCpu:F2}%");
                cpuDetails.AppendLine($"Threshold: {MaxCpuPercentage}%");
                Console.WriteLine($"{LogPrefix}Total CPU usage: {totalCpu:F2}%");

                if (totalCpu > MaxCpuPercentage)
                {
                    Console.WriteLine($"{LogPrefix}FAILED: CPU usage {totalCpu:F2}% exceeds threshold of {MaxCpuPercentage}%");
                    details = cpuDetails.ToString();
                    return false;
                }

                Console.WriteLine($"{LogPrefix}PASSED: CPU usage {totalCpu:F2}% is below threshold of {MaxCpuPercentage}%");
                details = cpuDetails.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix}ERROR measuring CPU: {ex.Message}");
                details = $"ERROR: {ex.Message}";
                return false;
            }
        }

        private static bool VerifyMemoryUsage(out string details)
        {
            var memoryDetails = new System.Text.StringBuilder();
            
            try
            {
                var processes = Process.GetProcessesByName(ProcessName);
                if (processes.Length == 0)
                {
                    Console.WriteLine($"{LogPrefix}ERROR: Process '{ProcessName}' not found");
                    details = "ERROR: Process not found";
                    return false;
                }

                double totalMemoryMB = 0;
                memoryDetails.AppendLine($"Memory Usage Details:");

                foreach (var process in processes)
                {
                    // Refresh to get current values
                    process.Refresh();
                    
                    double memoryMB = process.WorkingSet64 / (1024.0 * 1024.0);
                    totalMemoryMB += memoryMB;
                    Console.WriteLine($"{LogPrefix}Process ID {process.Id}: Memory = {memoryMB:F2} MB");
                    memoryDetails.AppendLine($"  Process ID {process.Id}: {memoryMB:F2} MB");
                }

                memoryDetails.AppendLine($"Total Memory: {totalMemoryMB:F2} MB");
                memoryDetails.AppendLine($"Threshold: {MaxMemoryMB} MB");
                Console.WriteLine($"{LogPrefix}Total Memory usage: {totalMemoryMB:F2} MB");

                if (totalMemoryMB > MaxMemoryMB)
                {
                    Console.WriteLine($"{LogPrefix}FAILED: Memory usage {totalMemoryMB:F2} MB exceeds threshold of {MaxMemoryMB} MB");
                    details = memoryDetails.ToString();
                    return false;
                }

                Console.WriteLine($"{LogPrefix}PASSED: Memory usage {totalMemoryMB:F2} MB is below threshold of {MaxMemoryMB} MB");
                details = memoryDetails.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix}ERROR measuring memory: {ex.Message}");
                details = $"ERROR: {ex.Message}";
                return false;
            }
        }
    }
}
