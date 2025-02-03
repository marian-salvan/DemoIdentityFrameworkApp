namespace DemoBackend.Requests
{
    public record ResetPasswordRequest
    {
        public required string NewPassword { get; set; }
        public required string Token { get; set; }
    }
}
