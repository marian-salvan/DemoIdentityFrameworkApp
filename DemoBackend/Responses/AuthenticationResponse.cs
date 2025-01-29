namespace DemoBackend.Responses
{
    public record AuthenticationResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime AccessTokenExpirationTime { get; set; }
    }
}
