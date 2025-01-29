namespace DemoBackend.Responses
{
    public record RefreshTokenResponse
    {
        public required string AccessToken { get; set; }
        public DateTime AccessTokenExpirationTime { get; set; }
    }
}
