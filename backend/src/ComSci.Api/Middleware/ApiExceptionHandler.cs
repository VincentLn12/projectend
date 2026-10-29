using ComSci.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Middleware;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string title;

        if (exception is NotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            title = "Not found";
        }
        else if (exception is BadRequestException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Bad request";
        }
        else if (exception is ConflictException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Conflict";
        }
        else
        {
            return false;
        }

        ProblemDetails problemDetails = new ProblemDetails();
        problemDetails.Status = statusCode;
        problemDetails.Title = title;
        problemDetails.Detail = exception.Message;

        // ProblemDetailsContext กำหนดค่าได้ตอน new เท่านั้น (init-only)
        ProblemDetailsContext context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        };

        httpContext.Response.StatusCode = statusCode;
        return await _problemDetailsService.TryWriteAsync(context);
    }
}
