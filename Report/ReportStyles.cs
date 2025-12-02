namespace SmokeTestsAgentWin.Tests
{

    public static class ReportStyles
    {
        public const string PassColor = "#10b981";

        public const string FailColor = "#ef4444";

        public static string GetStyles(string statusColor)
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
    }
}
