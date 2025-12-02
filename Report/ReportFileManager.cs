using System;
using System.Diagnostics;
using System.IO;

namespace SmokeTestsAgentWin.Tests
{
    public class ReportFileManager
    {
        private readonly string _reportDirectory;
        public ReportFileManager(string? reportDirectory = null)
        {
            _reportDirectory = reportDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        }

        public string SaveReport(string htmlContent, string reportName = "TestReport")
        {
            Directory.CreateDirectory(_reportDirectory);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var reportPath = Path.Combine(_reportDirectory, $"{reportName}_{timestamp}.html");
            
            File.WriteAllText(reportPath, htmlContent);
            
            return reportPath;
        }

        public void OpenInBrowser(string reportPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = reportPath,
                    UseShellExecute = true
                });
                Console.WriteLine("✅ Report opened in browser");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not open report automatically: {ex.Message}");
            }
        }
    }
}
