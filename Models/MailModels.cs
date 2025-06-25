// Models/MailModels.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace t23p0.Models
{
    public class MailRequest
    {
        [Required]
        public string Code { get; set; }
        
        [Required]
        public DateTime SelectedDate { get; set; }
        
        public string AdditionalMessage { get; set; }
    }

    public class MailRecipient
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
    }

    public class MailTemplate
    {
        public string Code { get; set; }
        public string Subject { get; set; }
        public string BodyTemplate { get; set; }
        public string Category { get; set; }
        public List<string> RequiredData { get; set; } = new List<string>();
    }

    public class MailContent
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }

    public class MailSendResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public string RecipientEmail { get; set; }
    }

    public class DataRequest
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public List<string> RequiredFields { get; set; } = new List<string>();
    }

    public class DataResponse
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
