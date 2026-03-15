using System.Text;
using CardsService.API.Configuration;
using CardsService.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DDD layers registration
Bootstrapper.RegisterServices(builder.Services, builder.Configuration);
Bootstrapper.RegisterRepositories(builder.Services);
Bootstrapper.RegisterValidators(builder.Services);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// === CORS for local development ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// === Pipeline HTTP ===

// Global Middleware exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Cards Service API")
               .WithPreferredScheme("Bearer")
               .WithHttpBearerAuthentication(bearer => bearer.Token = string.Empty);
    });
    app.UseCors("Development");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health checks for container orchestrators
// GET /health → liveness | GET /health/ready → readiness (includes MongoDB)
app.MapHealthChecks("/health/ready");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
