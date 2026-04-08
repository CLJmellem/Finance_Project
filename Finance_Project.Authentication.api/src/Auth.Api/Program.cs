using Auth.Api.Configuration;
using Auth.Api.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Bootstrapper.RegisterServices(builder.Services, builder.Configuration);
Bootstrapper.RegisterRepositories(builder.Services);
Bootstrapper.RegisterValidators(builder.Services);

var app = builder.Build();

// Handle Middleware exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
