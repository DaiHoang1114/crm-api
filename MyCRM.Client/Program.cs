using MyCRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MyCRM.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Keycloak Service
builder.Services.AddHttpClient<KeycloakService>();

// Load Keycloak settings from config.
var keycloakAuthority = builder.Configuration["Keycloak:Authority"];    // e.g. "http://localhost:8080/realms/your-realm"
var keycloakAudience = builder.Configuration["Keycloak:Audience"];      // e.g. "crm-api"

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.Audience = keycloakAudience;
    options.RequireHttpsMetadata = false; // only for dev
    // Optional: map claims
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Checks if token is expired
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };

    // Add debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"   Token: {context.Request.Headers["Authorization"]}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ Token validated successfully");
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine($"   Claims: {string.Join(", ", claims ?? Array.Empty<string>())}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"⚠️ Challenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// Add controllers
builder.Services.AddControllers();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors(policy => policy
  .WithOrigins("http://localhost:4200")
  .AllowAnyMethod()
  .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Test endpoints
app.MapGet("/public", () => "Public OK");

app.MapGet("/secure", (ClaimsPrincipal user) =>
{
    return $"Hello {user.Identity!.Name}";
}).RequireAuthorization();

app.Run();