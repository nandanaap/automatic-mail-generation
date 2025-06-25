// Controllers/t23c3_testpageController.cs (Enhanced)
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using t23p0.Models;
using t23p0.Services;

namespace t23p0.Controllers
{
    public class t23c3_testpageController : Controller
    {
        private readonly IAutoMailService _autoMailService;
        private readonly ILogger<t23c3_testpageController> _logger;

        public t23c3_testpageController(IAutoMailService autoMailService, ILogger<t23c3_testpageController> logger)
        {
            _autoMailService = autoMailService;
            _logger = logger;
        }

        // Display initial view
        [ActionName("t23v3_index")]
        public IActionResult t23v3_index()
        {
            var model = new MailRequest
            {
                SelectedDate = DateTime.Today
            };
            return View(model);
        }

        // Handle mail generation and sending
        [HttpPost]
        [ActionName("SendMail")]
        public async Task<IActionResult> SendMail(string code, DateTime selectedDate, string additionalMessage = "")
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    TempData["Error"] = "Please provide a valid code";
                    return RedirectToAction("t23v3_index");
                }

                var mailRequest = new MailRequest
                {
                    Code = code.Trim().ToUpper(),
                    SelectedDate = selectedDate,
                    AdditionalMessage = additionalMessage
                };

                // Generate and send mail using AMS
                var result = await _autoMailService.GenerateAndSendMailAsync(mailRequest);

                if (result.Success)
                {
                    TempData["Success"] = $"Mail sent successfully to {result.RecipientEmail} at {result.SentAt:yyyy-MM-dd HH:mm:ss}";
                    _logger.LogInformation($"Mail sent successfully for code {code} to {result.RecipientEmail}");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    _logger.LogError($"Failed to send mail for code {code}: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred while sending the mail.";
                _logger.LogError(ex, $"Exception occurred while sending mail for code {code}");
            }

            return RedirectToAction("t23v3_index");
        }

        // Preview mail content without sending
        [HttpPost]
        [ActionName("PreviewMail")]
        public async Task<IActionResult> PreviewMail(string code, DateTime selectedDate)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Json(new { success = false, message = "Please provide a valid code" });
                }

                var mailContent = await _autoMailService.GenerateMailContentAsync(code.Trim().ToUpper(), selectedDate);

                if (mailContent == null)
                {
                    return Json(new { success = false, message = "Could not generate mail content for the provided code" });
                }

                return Json(new
                {
                    success = true,
                    subject = mailContent.Subject,
                    body = mailContent.Body,
                    recipient = mailContent.RecipientEmail,
                    recipientName = mailContent.RecipientName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while previewing mail for code {code}");
                return Json(new { success = false, message = "An error occurred while generating the mail preview" });
            }
        }

        // Get available codes and their descriptions
        [HttpGet]
        [ActionName("GetAvailableCodes")]
        public IActionResult GetAvailableCodes()
        {
            var codes = new[]
            {
                new { code = "PE", description = "Production Employee", department = "Production" },
                new { code = "PM", description = "Production Manager", department = "Production" },
                new { code = "HR", description = "Human Resource", department = "Human Resources" },
                new { code = "FN", description = "Finance Department", department = "Finance" },
                new { code = "IT", description = "IT Department", department = "Information Technology" }
            };

            return Json(codes);
        }

        // Get data for specific code (for testing/debugging)
        [HttpPost]
        [ActionName("GetDataForCode")]
        public async Task<IActionResult> GetDataForCode(string code, DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Json(new { success = false, message = "Code is required" });
                }

                var dataResponse = await _autoMailService.GetDataForCodeAsync(code.Trim().ToUpper(), date);
                
                return Json(new
                {
                    success = dataResponse.Success,
                    data = dataResponse.Data,
                    error = dataResponse.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while getting data for code {code}");
                return Json(new { success = false, message = "An error occurred while fetching data" });
            }
        }

        // Mail history/logs (basic implementation)
        [ActionName("MailHistory")]
        public IActionResult MailHistory()
        {
            // In a real implementation, this would fetch from database
            // For now, return empty view
            return View();
        }
    }
}
