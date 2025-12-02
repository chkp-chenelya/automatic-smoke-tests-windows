using System;
using System.Linq;
using System.Text;

namespace SmokeTestsAgentWin.Tests
{
    /// <summary>
    /// Generates HTML templates for test reports.
    /// </summary>
    public class HtmlReportTemplate
    {
        public string Generate(TestReport report)
        {
            var statusColor = report.Passed ? ReportStyles.PassColor : ReportStyles.FailColor;
            var statusIcon = report.Passed ? "✓" : "✗";
            var statusText = report.Passed ? "PASSED" : "FAILED";

            var testCasesHtml = GenerateTestCasesHtml(report.TestCases);
            var errorSection = GenerateErrorSection(report);

            return BuildHtmlDocument(report, statusColor, statusIcon, statusText, testCasesHtml, errorSection);
        }

        private string GenerateTestCasesHtml(System.Collections.Generic.List<TestReport.TestCase> testCases)
        {
            var html = new StringBuilder();

            for (int i = 0; i < testCases.Count; i++)
            {
                var testCase = testCases[i];
                var testId = $"test-{i}";
                html.AppendLine(GenerateTestCaseHtml(testCase, i + 1, testId));
            }

            return html.ToString();
        }

        private string GenerateTestCaseHtml(TestReport.TestCase testCase, int index, string testId)
        {
            var statusColor = testCase.Passed ? ReportStyles.PassColor : ReportStyles.FailColor;
            var statusIcon = testCase.Passed ? "✓" : "✗";
            var stepsHtml = GenerateStepsHtml(testCase.Steps);

            return $@"
                <div class=""test-case"">
                    <div class=""test-case-header"" onclick=""toggleTest('{testId}')"">
                        <div class=""test-case-left"">
                            <div class=""test-case-number"">{index}</div>
                            <div class=""test-case-name"">{testCase.Name}</div>
                        </div>
                        <div class=""test-case-right"">
                            <div class=""test-case-status"" style=""background: {statusColor};"">
                                <span class=""status-icon"">{statusIcon}</span>
                            </div>
                            <div class=""test-case-duration"">{FormatDuration(testCase.Duration)}</div>
                            <div class=""test-case-steps-count"">{testCase.Steps.Count} steps</div>
                            <div class=""test-case-chevron"" id=""chevron-{testId}"">▼</div>
                        </div>
                    </div>
                    <div class=""test-case-steps"" id=""{testId}"" style=""display: none;"">
                        {stepsHtml}
                    </div>
                </div>";
        }

        private string GenerateStepsHtml(System.Collections.Generic.List<TestReport.TestStep> steps)
        {
            var html = new StringBuilder();

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                html.AppendLine(GenerateStepHtml(step, i + 1));
            }

            return html.ToString();
        }

        private string GenerateStepHtml(TestReport.TestStep step, int index)
        {
            var statusColor = step.Passed ? ReportStyles.PassColor : ReportStyles.FailColor;
            var statusIcon = step.Passed ? "✓" : "✗";
            var detailsHtml = string.IsNullOrEmpty(step.Details) ? "" : 
                $@"<div class=""step-details"">{System.Net.WebUtility.HtmlEncode(step.Details)}</div>";

            return $@"
                <div class=""step"">
                    <div class=""step-header"">
                        <div class=""step-number"">{index}</div>
                        <div class=""step-name"">{step.Name}</div>
                        <div class=""step-status"" style=""background: {statusColor};"">
                            <span class=""status-icon"">{statusIcon}</span>
                        </div>
                        <div class=""step-duration"">{FormatDuration(step.Duration)}</div>
                    </div>
                    {detailsHtml}
                </div>";
        }

        private string GenerateErrorSection(TestReport report)
        {
            if (report.Passed || string.IsNullOrEmpty(report.ErrorMessage))
                return "";

            return $@"
                <div class=""error-section"">
                    <div class=""error-header"">❌ Error Details</div>
                    <div class=""error-message"">{System.Net.WebUtility.HtmlEncode(report.ErrorMessage)}</div>
                </div>";
        }

        private string BuildHtmlDocument(TestReport report, string statusColor, string statusIcon, 
            string statusText, string testCasesHtml, string errorSection)
        {
            return $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Test Report - {report.TestName}</title>
                <style>
                    {ReportStyles.GetStyles(statusColor)}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""header"">
                        <div class=""test-name"">{report.TestName}</div>
                        <div class=""status-badge"">
                            <span class=""status-icon"">{statusIcon}</span>
                            {statusText}
                        </div>
                    </div>
        
                    <div class=""summary"">
                        <div class=""summary-item"">
                            <div class=""summary-label"">Duration</div>
                            <div class=""summary-value"">{FormatDuration(report.Duration)}</div>
                        </div>
                        <div class=""summary-item"">
                            <div class=""summary-label"">Total Tests</div>
                            <div class=""summary-value"">{report.TestCases.Count}</div>
                        </div>
                        <div class=""summary-item"">
                            <div class=""summary-label"">Passed Tests</div>
                            <div class=""summary-value"">{report.TestCases.Count(tc => tc.Passed)}</div>
                        </div>
                        <div class=""summary-item"">
                            <div class=""summary-label"">Start Time</div>
                            <div class=""summary-value"" style=""font-size: 18px;"">{report.StartTime:HH:mm:ss}</div>
                        </div>
                    </div>
        
                    {errorSection}
        
                    <div class=""steps-section"">
                        <div class=""section-title"">🧪 Test Cases (Click to expand)</div>
                        {testCasesHtml}
                    </div>
        
                    <div class=""footer"">
                        Generated on {DateTime.Now:dddd, MMMM dd, yyyy 'at' HH:mm:ss}
                    </div>
                </div>
    
                <script>
                    {GetJavaScript()}
                </script>
            </body>
            </html>";
        }

        private string GetJavaScript()
        {
            return @"
                function toggleTest(testId) {
                    var stepsDiv = document.getElementById(testId);
                    var chevron = document.getElementById('chevron-' + testId);
            
                    if (stepsDiv.style.display === 'none') {
                        stepsDiv.style.display = 'block';
                        chevron.classList.add('rotated');
                    } else {
                        stepsDiv.style.display = 'none';
                        chevron.classList.remove('rotated');
                    }
                }";
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalSeconds < 1)
                return $"{duration.TotalMilliseconds:F0}ms";
            else if (duration.TotalMinutes < 1)
                return $"{duration.TotalSeconds:F1}s";
            else
                return $"{duration.Minutes}m {duration.Seconds}s";
        }
    }
}
