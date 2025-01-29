namespace DemoBackend.Requests
{
    public record RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string Role { get; set; }
        public required int Age { get; set; }
    }
}
