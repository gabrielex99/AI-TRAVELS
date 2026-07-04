using System.Text.Json;
using AiTravels.Api.Data;
using AiTravels.Api.Middleware;
using AiTravels.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// JSON serialization: snake_case globally
// ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ──────────────────────────────────────────────
// Swagger / OpenAPI
// ──────────────────────────────────────────────
builder.Services.AddOpenApi();

// ──────────────────────────────────────────────
// Database: PostgreSQL via Npgsql
// ──────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
    ?? "Host=postgres;Port=5432;Database=aitravels;Username=aitravels;Password=aitravels";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ──────────────────────────────────────────────
// HttpClient for AI Engine
// ──────────────────────────────────────────────
var aiEngineUrl = builder.Configuration["AI_ENGINE_URL"] ?? "http://ai-engine:8000";

builder.Services.AddHttpClient<IAiEngineService, AiEngineService>(client =>
{
    client.BaseAddress = new Uri(aiEngineUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ──────────────────────────────────────────────
// Application services
// ──────────────────────────────────────────────
builder.Services.AddScoped<AffiliateService>();
builder.Services.AddScoped<CacheService>();

// ──────────────────────────────────────────────
// CORS: allow frontend on localhost:3000
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ──────────────────────────────────────────────
// Middleware pipeline
// ──────────────────────────────────────────────
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// ──────────────────────────────────────────────
// Auto-migrate database on startup (dev only)
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database is created. Is PostgreSQL running?");
    }
}

app.Run();
