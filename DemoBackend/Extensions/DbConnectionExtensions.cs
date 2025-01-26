using DemoBackend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DemoBackend.Extensions
{
    public static class DbConnectionExtensions
    {
        public static void AddIdentityDbContext(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddDataProtection(); //this service is required by the DataProtectorTokenProvider used in ASP.NET Core Identity

            services.AddIdentityCore<UserEntity>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            })
           .AddRoles<IdentityRole>()
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddTokenProvider<DataProtectorTokenProvider<UserEntity>>(TokenOptions.DefaultProvider);

            services.TryAddScoped<UserManager<UserEntity>>();
        }
    }
}
