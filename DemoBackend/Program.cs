using DemoBackend.Configuration;
using DemoBackend.Data;
using DemoBackend.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddIdentityDbContext(builder);

//register the authentication settings
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("Client"));

//register the authentication settings
builder.Services.AddJwtAuthentication(builder.Configuration);

//register the authorization policies
builder.Services.AddAuthorizationPolicies();

//register TokensRepository
builder.Services.TryAddScoped<ITokensRepository, TokensRepository>();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
