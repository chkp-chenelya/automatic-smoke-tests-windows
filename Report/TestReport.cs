using System;
using System.Collections.Generic;

namespace SmokeTestsAgentWin.Tests
{
    public class TestReport
    {
        public string TestName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        public bool Passed { get; set; }

        public List<TestStep> Steps { get; set; } = new List<TestStep>();

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();

        public string ErrorMessage { get; set; } = string.Empty;

        public class TestCase
        {
            public string Name { get; set; } = string.Empty;

            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public TimeSpan Duration => EndTime - StartTime;

            public bool Passed { get; set; }

            public bool Success { get; set; } = true;

            public string ErrorMessage { get; set; } = string.Empty;

            public List<TestStep> Steps { get; set; } = new List<TestStep>();
        }

        public class TestStep
        {
            public string Name { get; set; } = string.Empty;

            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public TimeSpan Duration => EndTime - StartTime;

            public bool Passed { get; set; }

            public string Details { get; set; } = string.Empty;
        }

        public bool ExecuteStep(string stepName, Action action, string successMessage = "", string failureMessage = "")
        {
            var step = new TestStep
            {
                Name = stepName,
                StartTime = DateTime.Now
            };

            try
            {
                Console.WriteLine($"  Executing: {stepName}");
                action();
                step.Passed = true;
                step.Details = string.IsNullOrEmpty(successMessage) ? "Success" : successMessage;
                Console.WriteLine($"    ✓ {step.Details}");
                return true;
            }
            catch (Exception ex)
            {
                step.Passed = false;
                step.Details = string.IsNullOrEmpty(failureMessage) ? ex.Message : $"{failureMessage}: {ex.Message}";
                Console.WriteLine($"    ✗ {step.Details}");
                return false;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                Steps.Add(step);
            }
        }

        public bool ExecuteStep(string stepName, Func<bool> action, string successMessage = "", string failureMessage = "")
        {
            var step = new TestStep
            {
                Name = stepName,
                StartTime = DateTime.Now
            };

            try
            {
                Console.WriteLine($"  Executing: {stepName}");
                bool result = action();
                if (!result)
                {
                    throw new Exception(string.IsNullOrEmpty(failureMessage) ? "Step returned false" : failureMessage);
                }
                step.Passed = true;
                step.Details = string.IsNullOrEmpty(successMessage) ? "Success" : successMessage;
                Console.WriteLine($"    ✓ {step.Details}");
                return true;
            }
            catch (Exception ex)
            {
                step.Passed = false;
                step.Details = string.IsNullOrEmpty(failureMessage) ? ex.Message : $"{failureMessage}: {ex.Message}";
                Console.WriteLine($"    ✗ {step.Details}");
                return false;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                Steps.Add(step);
            }
        }

        public void GenerateAndOpenReport()
        {
            var template = new HtmlReportTemplate();
            var htmlContent = template.Generate(this);
            
            var fileManager = new ReportFileManager();
            var reportPath = fileManager.SaveReport(htmlContent);
            
            Console.WriteLine($"\n📊 Report generated: {reportPath}");
            
            fileManager.OpenInBrowser(reportPath);
        }
    }
}
