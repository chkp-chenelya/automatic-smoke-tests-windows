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
        private const string PassColor = "#10b981";
        private const string FailColor = "#ef4444";

        public string Generate(TestReport report)
        {
            var statusColor = report.Passed ? PassColor : FailColor;
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
            var statusColor = testCase.Passed ? PassColor : FailColor;
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
            var statusColor = step.Passed ? PassColor : FailColor;
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
                    {GetCssStyles(statusColor)}
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

        private string GetCssStyles(string statusColor)
        {
            return $@"
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 40px 20px;
        }}
        
        .container {{
            max-width: 1000px;
            margin: 0 auto;
            background: white;
            border-radius: 16px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        
        .test-name {{
            font-size: 32px;
            font-weight: 700;
            margin-bottom: 20px;
        }}
        
        .status-badge {{
            display: inline-block;
            background: {statusColor};
            color: white;
            padding: 12px 30px;
            border-radius: 50px;
            font-size: 24px;
            font-weight: 600;
            margin: 10px 0;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
        }}
        
        .status-icon {{
            font-size: 28px;
            margin-right: 8px;
        }}
        
        .summary {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            padding: 40px;
            background: #f8fafc;
        }}
        
        .summary-item {{
            background: white;
            padding: 25px;
            border-radius: 12px;
            text-align: center;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            transition: transform 0.2s;
        }}
        
        .summary-item:hover {{
            transform: translateY(-5px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        
        .summary-label {{
            color: #64748b;
            font-size: 14px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 10px;
        }}
        
        .summary-value {{
            color: #1e293b;
            font-size: 32px;
            font-weight: 700;
        }}
        
        .steps-section {{
            padding: 40px;
        }}
        
        .section-title {{
            font-size: 24px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 30px;
            padding-bottom: 15px;
            border-bottom: 3px solid #667eea;
        }}
        
        .test-case {{
            background: white;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            margin-bottom: 20px;
            overflow: hidden;
            transition: all 0.3s;
        }}
        
        .test-case:hover {{
            border-color: #667eea;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
        }}
        
        .test-case-header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 25px;
            background: #f8fafc;
            cursor: pointer;
            user-select: none;
        }}
        
        .test-case-header:hover {{
            background: #f1f5f9;
        }}
        
        .test-case-left {{
            display: flex;
            align-items: center;
            gap: 15px;
            flex: 1;
        }}
        
        .test-case-right {{
            display: flex;
            align-items: center;
            gap: 15px;
        }}
        
        .test-case-number {{
            width: 50px;
            height: 50px;
            background: #667eea;
            color: white;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            font-size: 24px;
        }}
        
        .test-case-name {{
            font-size: 20px;
            font-weight: 700;
            color: #1e293b;
        }}
        
        .test-case-status {{
            padding: 10px 20px;
            border-radius: 20px;
            color: white;
            font-weight: 600;
            font-size: 16px;
            min-width: 50px;
            text-align: center;
        }}
        
        .test-case-duration {{
            color: #64748b;
            font-weight: 700;
            font-size: 16px;
            padding: 10px 20px;
            background: white;
            border-radius: 20px;
        }}
        
        .test-case-steps-count {{
            color: #667eea;
            font-weight: 600;
            font-size: 14px;
            padding: 8px 16px;
            background: #ede9fe;
            border-radius: 20px;
        }}
        
        .test-case-chevron {{
            color: #667eea;
            font-size: 20px;
            font-weight: 700;
            transition: transform 0.3s;
            width: 30px;
            text-align: center;
        }}
        
        .test-case-chevron.rotated {{
            transform: rotate(180deg);
        }}
        
        .test-case-steps {{
            padding: 25px;
            background: white;
            border-top: 2px solid #e2e8f0;
        }}
        
        .step {{
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            margin-bottom: 15px;
            overflow: hidden;
        }}
        
        .step-header {{
            display: grid;
            grid-template-columns: 50px 1fr auto auto;
            align-items: center;
            gap: 15px;
            padding: 15px;
        }}
        
        .step-number {{
            width: 35px;
            height: 35px;
            background: #667eea;
            color: white;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 700;
            font-size: 16px;
        }}
        
        .step-name {{
            font-size: 15px;
            font-weight: 600;
            color: #1e293b;
        }}
        
        .step-status {{
            padding: 6px 14px;
            border-radius: 15px;
            color: white;
            font-weight: 600;
            font-size: 13px;
            min-width: 35px;
            text-align: center;
        }}
        
        .step-duration {{
            color: #64748b;
            font-weight: 600;
            font-size: 13px;
            padding: 6px 14px;
            background: white;
            border-radius: 15px;
        }}
        
        .step-details {{
            padding: 15px;
            color: #475569;
            background: white;
            border-top: 1px solid #e2e8f0;
            font-family: 'Courier New', monospace;
            font-size: 13px;
            line-height: 1.6;
            white-space: pre-wrap;
        }}
        
        .error-section {{
            margin: 20px 40px;
            padding: 25px;
            background: #fef2f2;
            border-left: 4px solid #ef4444;
            border-radius: 8px;
        }}
        
        .error-header {{
            font-size: 18px;
            font-weight: 700;
            color: #dc2626;
            margin-bottom: 15px;
        }}
        
        .error-message {{
            color: #991b1b;
            font-family: 'Courier New', monospace;
            font-size: 14px;
            line-height: 1.6;
            white-space: pre-wrap;
        }}
        
        .footer {{
            padding: 30px 40px;
            text-align: center;
            color: #64748b;
            font-size: 14px;
            background: #f8fafc;
            border-top: 1px solid #e2e8f0;
        }}
        
        @media print {{
            body {{
                background: white;
                padding: 0;
            }}
            
            .container {{
                box-shadow: none;
            }}
        }}";
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
