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
    }
}
