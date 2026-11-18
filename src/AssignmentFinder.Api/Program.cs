using AssignmentFinder.Infrastructure;
using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Scraping;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssignmentFinderInfrastructure(builder.Configuration);
builder.Services.AddAssignmentConnectors();

builder.Services
    .AddAuthenticationJwtBearer(options =>
    {
        options.SigningKey = builder.Configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
    })
    .AddAuthorization();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
