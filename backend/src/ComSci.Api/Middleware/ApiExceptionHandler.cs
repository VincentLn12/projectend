using ComSci.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Middleware;

/// <summary>Turns known business exceptions into ProblemDetails; anything else falls through to a 500.</summary>
public class ApiExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (0, null),
        };
        if (title is null) return false;

        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
