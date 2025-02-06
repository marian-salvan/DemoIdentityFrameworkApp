using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace DemoBackend.Extensions
{
    public static class SecurityExtensions
    {
        public static void AddSecurity(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN"; // Set the header name for the CSRF token
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://example.com",
                                            "http://www.contoso.com");
                    });
            });

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("fixed", options =>
                {
                    options.PermitLimit = 5; // Allow 10 requests
                    options.Window = TimeSpan.FromSeconds(10); // Within a 10-second window
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0; // No queueing; reject excess requests
                });
            });
        }
    }
}