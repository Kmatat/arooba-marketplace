using System.Text;
using Arooba.Application;
using Arooba.Infrastructure;
using Arooba.Infrastructure.Persistence;
using Arooba.API.Extensions;
using Arooba.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service Registration
// ---------------------------------------------------------------------------

// Infrastructure layer MUST be registered BEFORE Application layer
// because Application layer services depend on Infrastructure services
// (especially IDateTimeService used by handlers and interceptors)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Application layer: MediatR, FluentValidation, AutoMapper, pipeline behaviors
builder.Services.AddApplicationServices();

// API layer: controllers with global exception filter, problem details
builder.Services.AddApiServices();

// Health checks (includes SQL Server connectivity check)
builder.Services.AddAroobaHealthChecks(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Arooba Marketplace API",
        Version = "v1",
        Description = "Egypt's Most Inclusive Local E-Commerce Marketplace API",
        Contact = new OpenApiContact
        {
            Name = "Arooba",
            Email = "Info@aroobh.com",
            Url = new Uri("https://www.aroobh.com")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments from the API assembly for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AroobaPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Authentication (JWT Bearer)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException(
                "JwtSettings:Secret is not configured. Set it via environment variable or user secrets.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------------------
// Build the application
// ---------------------------------------------------------------------------

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware Pipeline
// ---------------------------------------------------------------------------

// Global exception handling (must be first to catch all downstream errors)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arooba Marketplace API v1");
        c.RoutePrefix = "swagger";
    });

    // Auto-migrate and seed the database in development
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AroobaDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AroobaDbContext>();
    await context.Database.MigrateAsync();
    await AroobaDbContextSeed.SeedAsync(context, logger);
}

app.UseHttpsRedirection();
app.UseCors("AroobaPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();
