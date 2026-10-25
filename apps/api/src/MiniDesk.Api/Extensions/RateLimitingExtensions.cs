using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MiniDesk.Api.Options;

namespace MiniDesk.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<RateLimitOptions>()
            .Bind(config.GetSection(RateLimitOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var opt = config.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new();

        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // strict: login, register, refresh, by IP
            o.AddPolicy(AuthPolicy, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = opt.AuthPermitLimit,
                        Window = TimeSpan.FromSeconds(opt.AuthWindowSeconds),
                        QueueLimit = 0
                    }));

            // general: per user if logged in, otherwise per IP
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var key = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? ctx.Connection.RemoteIpAddress?.ToString()
                          ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = opt.GlobalPermitLimit,
                    Window = TimeSpan.FromSeconds(opt.GlobalWindowSeconds),
                    QueueLimit = 0
                });
            });

            // JSON body consistent with your other errors
            o.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    status = 429,
                    title = "Too Many Requests",
                    detail = "Too many requests. Please wait and try again.",
                    traceId = context.HttpContext.TraceIdentifier
                }, ct);
            };
        });

        return services;
    }
}