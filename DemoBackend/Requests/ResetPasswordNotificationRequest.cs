namespace DemoBackend.Requests
{
    public record ResetPasswordNotificationRequest
    {
        public required string Email { get; set; }
    }
}
