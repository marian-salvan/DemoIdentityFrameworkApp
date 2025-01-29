namespace DemoBackend.Requests
{
    public record RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }
}
