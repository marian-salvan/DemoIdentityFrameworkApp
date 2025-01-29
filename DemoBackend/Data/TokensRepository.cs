using DemoBackend.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DemoBackend.Data
{
    public interface ITokensRepository
    {
        Task<IdentityUserToken<string>?> GetUserRefreshTokenAsync(string refreshToken);
        Task<bool> UpsertUserRefreshTokenAsync(string userId, string refreshToken);
        Task<bool> RemoveUserRefreshTokenAsync(string userId);
    }

    public class TokensRepository(ApplicationDbContext applicationDbContext) : ITokensRepository
    {
        public async Task<IdentityUserToken<string>?> GetUserRefreshTokenAsync(string refreshToken)
        {
            return await applicationDbContext.UserTokens.SingleOrDefaultAsync(x => x.Value == refreshToken);
        }

        public async Task<bool> UpsertUserRefreshTokenAsync(string userId, string refreshToken)
        {
            var existingUserToken = await applicationDbContext.UserTokens
                .FindAsync(userId, UserTokenConstants.LocalProvider, UserTokenConstants.Refresh);

            if (existingUserToken == null)
            {
                applicationDbContext.UserTokens.Add(new IdentityUserToken<string>
                {
                    UserId = userId,
                    LoginProvider = UserTokenConstants.LocalProvider,
                    Name = UserTokenConstants.Refresh,
                    Value = refreshToken
                });
            }
            else
            {
                existingUserToken.Value = refreshToken;
                applicationDbContext.UserTokens.Update(existingUserToken);
            }

            return (await applicationDbContext.SaveChangesAsync()) == 1;
        }

        public async Task<bool> RemoveUserRefreshTokenAsync(string userId)
        {
            var userToken = await applicationDbContext.UserTokens
                .FindAsync(userId, UserTokenConstants.LocalProvider, UserTokenConstants.Refresh);

            if (userToken == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            applicationDbContext.UserTokens.Remove(userToken);

            return (await applicationDbContext.SaveChangesAsync()) == 1;
        }
    }
}
