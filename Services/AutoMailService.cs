// Services/AutoMailService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using t23p0.Models;

namespace t23p0.Services
{
    public interface IAutoMailService
    {
        Task<MailSendResult> GenerateAndSendMailAsync(MailRequest request);
        Task<MailContent> GenerateMailContentAsync(string code, DateTime date);
        Task<DataResponse> GetDataForCodeAsync(string code, DateTime date);
    }

    public class AutoMailService : IAutoMailService
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, MailRecipient> _recipients;
        private readonly Dictionary<string, MailTemplate> _templates;

        public AutoMailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _recipients = InitializeRecipients();
            _templates = InitializeTemplates();
        }

        public async Task<MailSendResult> GenerateAndSendMailAsync(MailRequest request)
        {
            try
            {
                // Generate mail content
                var mailContent = await GenerateMailContentAsync(request.Code, request.SelectedDate);
                
                if (mailContent == null)
                {
                    return new MailSendResult
                    {
                        Success = false,
                        Message = "Failed to generate mail content for the provided code."
                    };
                }

                // Add additional message if provided
                if (!string.IsNullOrEmpty(request.AdditionalMessage))
                {
                    mailContent.Body += $"\n\nAdditional Message:\n{request.AdditionalMessage}";
                }

                // Send email
                await SendEmailAsync(mailContent);

                return new MailSendResult
                {
                    Success = true,
                    Message = "Mail sent successfully",
                    SentAt = DateTime.Now,
                    RecipientEmail = mailContent.RecipientEmail
                };
            }
            catch (Exception ex)
            {
                return new MailSendResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<MailContent> GenerateMailContentAsync(string code, DateTime date)
        {
            // Get recipient information
            if (!_recipients.ContainsKey(code))
                return null;

            var recipient = _recipients[code];

            // Get mail template
            if (!_templates.ContainsKey(code))
                return null;

            var template = _templates[code];

            // Get data for the specific code and date
            var dataResponse = await GetDataForCodeAsync(code, date);
            
            if (!dataResponse.Success)
                return null;

            // Generate mail content
            var subject = ProcessTemplate(template.Subject, dataResponse.Data, date, recipient);
            var body = ProcessTemplate(template.BodyTemplate, dataResponse.Data, date, recipient);

            return new MailContent
            {
                Subject = subject,
                Body = body,
                RecipientEmail = recipient.Email,
                RecipientName = recipient.Name,
                SenderEmail = _configuration["EmailSettings:SenderEmail"] ?? "system@company.com",
                SenderName = _configuration["EmailSettings:SenderName"] ?? "System Administrator"
            };
        }

        public async Task<DataResponse> GetDataForCodeAsync(string code, DateTime date)
        {
            // Simulate fetching data from external source based on code
            // In real implementation, this would call APIs, databases, etc.
            
            await Task.Delay(100); // Simulate async operation

            var data = new Dictionary<string, object>();

            switch (code.ToUpper())
            {
                case "PE": // Production Employee
                    data = GetProductionEmployeeData(date);
                    break;
                case "PM": // Production Manager
                    data = GetProductionManagerData(date);
                    break;
                case "HR": // Human Resource
                    data = GetHRData(date);
                    break;
                case "FN": // Finance Department
                    data = GetFinanceData(date);
                    break;
                case "IT": // IT Department
                    data = GetITData(date);
                    break;
                default:
                    return new DataResponse
                    {
                        Success = false,
                        ErrorMessage = "Unknown code provided"
                    };
            }

            return new DataResponse
            {
                Data = data,
                Success = true
            };
        }

        private async Task SendEmailAsync(MailContent mailContent)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mailContent.SenderEmail, mailContent.SenderName),
                    Subject = mailContent.Subject,
                    Body = mailContent.Body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(new MailAddress(mailContent.RecipientEmail, mailContent.RecipientName));

                await client.SendMailAsync(mailMessage);
            }
        }

        private string ProcessTemplate(string template, Dictionary<string, object> data, DateTime date, MailRecipient recipient)
        {
            var result = template;

            // Replace placeholders with actual data
            result = result.Replace("{RecipientName}", recipient.Name);
            result = result.Replace("{Date}", date.ToString("dd MMMM yyyy"));
            result = result.Replace("{Department}", recipient.Department);
            result = result.Replace("{Role}", recipient.Role);

            // Replace data placeholders
            foreach (var kvp in data)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
            }

            return result;
        }

        private Dictionary<string, MailRecipient> InitializeRecipients()
        {
            return new Dictionary<string, MailRecipient>
            {
                { "PE", new MailRecipient { Code = "PE", Name = "John Smith", Email = "john.smith@company.com", Department = "Production", Role = "Employee" } },
                { "PM", new MailRecipient { Code = "PM", Name = "Sarah Johnson", Email = "sarah.johnson@company.com", Department = "Production", Role = "Manager" } },
                { "HR", new MailRecipient { Code = "HR", Name = "Mike Wilson", Email = "mike.wilson@company.com", Department = "Human Resources", Role = "HR Specialist" } },
                { "FN", new MailRecipient { Code = "FN", Name = "Lisa Brown", Email = "lisa.brown@company.com", Department = "Finance", Role = "Finance Manager" } },
                { "IT", new MailRecipient { Code = "IT", Name = "David Lee", Email = "david.lee@company.com", Department = "IT", Role = "IT Administrator" } }
            };
        }

        private Dictionary<string, MailTemplate> InitializeTemplates()
        {
            return new Dictionary<string, MailTemplate>
            {
                {
                    "PE",
                    new MailTemplate
                    {
                        Code = "PE",
                        Subject = "Production Report - {Date}",
                        BodyTemplate = @"Dear {RecipientName},

This is your automated production report for {Date}.

Production Summary:
- Units Produced: {UnitsProduced}
- Quality Score: {QualityScore}%
- Efficiency Rate: {EfficiencyRate}%
- Downtime: {Downtime} hours

Your performance target for this period was {Target} units.
{PerformanceMessage}

Best regards,
Production Management System",
                        Category = "Production"
                    }
                },
                {
                    "PM",
                    new MailTemplate
                    {
                        Code = "PM",
                        Subject = "Daily Production Management Report - {Date}",
                        BodyTemplate = @"Dear {RecipientName},

Your daily production management summary for {Date}:

Overall Production Metrics:
- Total Units: {TotalUnits}
- Team Performance: {TeamPerformance}%
- Issues Reported: {IssuesCount}
- Resolved Issues: {ResolvedIssues}

Department Status: {DepartmentStatus}

Action items requiring your attention:
{ActionItems}

Best regards,
Management Information System",
                        Category = "Management"
                    }
                },
                {
                    "HR",
                    new MailTemplate
                    {
                        Code = "HR",
                        Subject = "HR Daily Summary - {Date}",
                        BodyTemplate = @"Dear {RecipientName},

Human Resources daily summary for {Date}:

Attendance Summary:
- Present: {PresentCount}
- Absent: {AbsentCount}
- Late Arrivals: {LateCount}

New Requests:
- Leave Requests: {LeaveRequests}
- Training Requests: {TrainingRequests}

Pending Actions: {PendingActions}

Best regards,
HR Management System",
                        Category = "HR"
                    }
                },
                {
                    "FN",
                    new MailTemplate
                    {
                        Code = "FN",
                        Subject = "Financial Summary - {Date}",
                        BodyTemplate = @"Dear {RecipientName},

Financial summary for {Date}:

Daily Figures:
- Revenue: ${Revenue}
- Expenses: ${Expenses}
- Net: ${NetAmount}

Budget Status: {BudgetStatus}
Outstanding Items: {OutstandingCount}

Requires Review: {ReviewItems}

Best regards,
Financial Management System",
                        Category = "Finance"
                    }
                },
                {
                    "IT",
                    new MailTemplate
                    {
                        Code = "IT",
                        Subject = "IT System Report - {Date}",
                        BodyTemplate = @"Dear {RecipientName},

IT systems status report for {Date}:

System Health:
- Server Uptime: {ServerUptime}%
- Network Status: {NetworkStatus}
- Backup Status: {BackupStatus}

Incidents:
- New Tickets: {NewTickets}
- Resolved: {ResolvedTickets}
- Pending: {PendingTickets}

Security Updates: {SecurityUpdates}

Best regards,
IT Management System",
                        Category = "IT"
                    }
                }
            };
        }

        private Dictionary<string, object> GetProductionEmployeeData(DateTime date)
        {
            var random = new Random();
            var target = 100;
            var produced = random.Next(80, 120);
            
            return new Dictionary<string, object>
            {
                { "UnitsProduced", produced },
                { "QualityScore", random.Next(85, 98) },
                { "EfficiencyRate", random.Next(80, 95) },
                { "Downtime", random.Next(0, 3) },
                { "Target", target },
                { "PerformanceMessage", produced >= target ? "Excellent work! Target achieved." : "Please focus on meeting production targets." }
            };
        }

        private Dictionary<string, object> GetProductionManagerData(DateTime date)
        {
            var random = new Random();
            return new Dictionary<string, object>
            {
                { "TotalUnits", random.Next(800, 1200) },
                { "TeamPerformance", random.Next(85, 95) },
                { "IssuesCount", random.Next(2, 8) },
                { "ResolvedIssues", random.Next(1, 6) },
                { "DepartmentStatus", "Operational" },
                { "ActionItems", "Review quality metrics, Schedule maintenance for Line 2" }
            };
        }

        private Dictionary<string, object> GetHRData(DateTime date)
        {
            var random = new Random();
            return new Dictionary<string, object>
            {
                { "PresentCount", random.Next(45, 50) },
                { "AbsentCount", random.Next(0, 5) },
                { "LateCount", random.Next(0, 3) },
                { "LeaveRequests", random.Next(1, 5) },
                { "TrainingRequests", random.Next(0, 3) },
                { "PendingActions", "Performance reviews for Q1, Update employee handbook" }
            };
        }

        private Dictionary<string, object> GetFinanceData(DateTime date)
        {
            var random = new Random();
            var revenue = random.Next(50000, 80000);
            var expenses = random.Next(30000, 45000);
            
            return new Dictionary<string, object>
            {
                { "Revenue", revenue.ToString("N0") },
                { "Expenses", expenses.ToString("N0") },
                { "NetAmount", (revenue - expenses).ToString("N0") },
                { "BudgetStatus", "On Track" },
                { "OutstandingCount", random.Next(5, 15) },
                { "ReviewItems", "Monthly reconciliation, Vendor payments approval" }
            };
        }

        private Dictionary<string, object> GetITData(DateTime date)
        {
            var random = new Random();
            return new Dictionary<string, object>
            {
                { "ServerUptime", random.Next(95, 100) },
                { "NetworkStatus", "Stable" },
                { "BackupStatus", "Completed" },
                { "NewTickets", random.Next(3, 10) },
                { "ResolvedTickets", random.Next(5, 12) },
                { "PendingTickets", random.Next(2, 8) },
                { "SecurityUpdates", "All systems updated, No critical vulnerabilities" }
            };
        }
    }
}
