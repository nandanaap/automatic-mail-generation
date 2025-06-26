private Dictionary<string, MailRecipient> InitializeRecipients()
{
    return new Dictionary<string, MailRecipient>
    {
        // Add this test entry
        { "TEST", new MailRecipient { Code = "TEST", Name = "Nandana Pramod", Email = "nandanapramodak@gmail.com", Department = "Testing", Role = "Developer" } },
        
        // Your existing entries...
        { "PE", new MailRecipient { Code = "PE", Name = "John Smith", Email = "john.smith@company.com", Department = "Production", Role = "Employee" } },
        { "PM", new MailRecipient { Code = "PM", Name = "Sarah Johnson", Email = "sarah.johnson@company.com", Department = "Production", Role = "Manager" } },
        // ... rest of your entries
    };
}
