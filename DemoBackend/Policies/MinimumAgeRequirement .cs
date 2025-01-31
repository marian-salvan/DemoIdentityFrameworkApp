using DemoBackend.Constants;
using Microsoft.AspNetCore.Authorization;

namespace DemoBackend.Policies
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; }

        public MinimumAgeRequirement(int minimumAge) => MinimumAge = minimumAge;
    }

    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MinimumAgeRequirement requirement)
        {
            var age = context.User.FindFirst(ApplicationClaims.Age);

            if (age is null)
            {
                return Task.CompletedTask;
            }

            if (int.TryParse(age.Value, out var userAge))
            {
                if (userAge >= requirement.MinimumAge)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
