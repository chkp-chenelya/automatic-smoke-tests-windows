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

        public void GenerateAndOpenReport()
        {
            var template = new HtmlReportTemplate();
            var htmlContent = template.Generate(this);
            
            var fileManager = new ReportFileManager();
            var reportPath = fileManager.SaveReport(htmlContent);
            
            Console.WriteLine($"\n📊 Report generated: {reportPath}");
            
            fileManager.OpenInBrowser(reportPath);
        }

        /// <summary>
        /// Executes a test step with standardized error handling and reporting.
        /// </summary>
        /// <param name="stepName">The name of the step.</param>
        /// <param name="action">The action to execute that returns true on success.</param>
        /// <param name="successMessage">Message to log on success.</param>
        /// <param name="failureMessage">Message to log on failure.</param>
        /// <returns>True if the step passed, false otherwise.</returns>
        public bool ExecuteStep(string stepName, Func<bool> action, string successMessage, string failureMessage)
        {
            var step = new TestStep
            {
                Name = stepName,
                StartTime = DateTime.Now
            };

            try
            {
                step.Passed = action();
                step.Details = step.Passed ? successMessage : failureMessage;
            }
            catch (Exception ex)
            {
                step.Passed = false;
                step.Details = $"Exception: {ex.Message}";
            }
            finally
            {
                step.EndTime = DateTime.Now;
                Steps.Add(step);
            }

            return step.Passed;
        }
    }
}
