using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MiniDesk.Application.Common.Exceptions;

namespace MiniDesk.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // client closed the connection, nobody is listening for a response
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(ex, "Exception after the response started.");
            throw ex is null ? new InvalidOperationException() : ex;   // can't write a body anymore
        }

        var problem = ex switch
        {
            ValidationException ve => BuildValidation(ve),
            NotFoundException => Build(StatusCodes.Status404NotFound, "Not Found", ex.Message),
            ForbiddenException => Build(StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            ConflictException => Build(StatusCodes.Status409Conflict, "Conflict", ex.Message),
            UnauthorizedException => Build(StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
            _ => Build(StatusCodes.Status500InternalServerError, "Server Error", env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.")
        };

        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (problem.Status >= 500)
            logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
        else
            logger.LogWarning("{Type}: {Message}", ex.GetType().Name, ex.Message);

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }

    private static ProblemDetails Build(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail
    };

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