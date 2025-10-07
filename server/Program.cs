using Server.Data;
using Server.Services;
using Server.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Server.Helper;
using Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoService, TodoService>();

// Configure JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-very-secure-secret-key-that-is-at-least-256-bits-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TodoApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TodoApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.

// IMPORTANT: Exception handling middleware should be first
app.UseGlobalExceptionHandling();

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Production security headers
    app.UseHsts();
}

// Security middleware
app.UseHttpsRedirection();

// CORS - should be before authentication
app.UseCors(policy => 
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:57419") // React/Vite dev servers
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials());

// Request/Response logging middleware
app.UseRequestLogging();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks endpoint
app.MapHealthChecks("/health");

// Default endpoint for API info
app.MapGet("/", () => new
{
    Name = "TodoApp API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
});

app.Run();
