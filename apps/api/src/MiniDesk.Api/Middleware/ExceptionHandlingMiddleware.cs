using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniDesk.Application.Common.Exceptions;
using Serilog.Context;

namespace MiniDesk.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // (2) every log line written while this request runs gets the TraceId property
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                context.Response.StatusCode = 499;   // client left, nobody to answer
            }
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        // (1) if part of the response was already sent, we can't write a JSON error body anymore
        if (context.Response.HasStarted)
        {
            logger.LogError(ex, "Exception after the response started.");
            return;
        }

        var problem = ex switch
        {
            ValidationException ve => BuildValidation(ve),
            NotFoundException => Build(404, "Not Found", ex.Message),
            ForbiddenException => Build(403, "Forbidden", ex.Message),
            UnauthorizedException => Build(401, "Unauthorized", ex.Message),
            ConflictException => Build(409, "Conflict", ex.Message),
            DbUpdateConcurrencyException => Build(409, "Conflict", "The record was changed by someone else. Reload and try again."),
            DbUpdateException => Build(409, "Conflict", "The change conflicts with existing data (duplicate value or a record still in use)."),
            _ => Build(500, "Server Error",
                                                env.IsDevelopment()
                                                    ? $"{ex.GetType().Name}: {ex.Message}"
                                                    : "An unexpected error occurred. Quote the traceId when reporting it.")
        };

        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (problem.Status >= 500)
            logger.LogError(ex, "Unhandled exception");
        else
            logger.LogWarning("{Type}: {Message}", ex.GetType().Name, ex.Message);

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }

    private static ProblemDetails Build(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ValidationProblemDetails BuildValidation(ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };
    }
}