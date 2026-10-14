using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MiniDesk.Api.Extensions;
using MiniDesk.Api.Middleware;
using MiniDesk.Api.OpenApi;
using MiniDesk.Api.Services;
using MiniDesk.Application;
using MiniDesk.Application.Common.Interfaces;
using MiniDesk.Infrastructure;
using MiniDesk.Infrastructure.Identity.Seed;
using MiniDesk.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

const string CorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<AuthorizeOperationTransformer>();
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();   // first, so it logs the final status code

app.UseExceptionHandling();
app.UseStatusCodePages();         // JSON bodies for bare 401/403/404 (needs builder.Services.AddProblemDetails())


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("MiniDesk API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    // apply pending migrations in development only
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}

await IdentitySeeder.SeedAsync(app.Services);

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);    // before authentication

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });          // app is running
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") }); // DB reachable

app.Run();