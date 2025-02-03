namespace DemoBackend.Requests
{
    public record ResetEmailNotificationRequest
    {
        public required string NewEmail { get; set; }
    }
}
