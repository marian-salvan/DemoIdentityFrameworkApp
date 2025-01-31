using DemoBackend.Constants;
using DemoBackend.Policies;
using Microsoft.AspNetCore.Authorization;

namespace DemoBackend.Extensions
{
    public static class AuthorizationExtentions
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                //claims based authorization
                options.AddPolicy(PoliciesConstants.Premium,
                    policy => policy.RequireClaim(ApplicationClaims.AccessToPremium, ApplicationClaims.AccessToPremium));

                //policy based authorization
                options.AddPolicy(PoliciesConstants.AgeRestriction,
                    policy => policy.Requirements.Add(new MinimumAgeRequirement(18)));
            });

           services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
        }
    }
}
