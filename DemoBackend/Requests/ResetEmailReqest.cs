namespace DemoBackend.Requests
{
    public record ResetEmailReqest
    {
        public required string NewEmail { get; set; }
        public required string Token { get; set; }
    }
}
