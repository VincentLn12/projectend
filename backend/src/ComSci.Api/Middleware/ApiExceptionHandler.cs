using ComSci.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ComSci.Api.Middleware;

/// <summary>
/// แปลง exception ทางธุรกิจที่ service โยนมา ให้เป็น HTTP response แบบ ProblemDetails
/// NotFoundException -> 404, BadRequestException -> 400, ConflictException -> 409
/// exception ชนิดอื่นไม่ถูกจัดการที่นี่ จะกลายเป็น 500 ตามปกติ
/// </summary>
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
            // ไม่ใช่ exception ที่รู้จัก ปล่อยให้ระบบจัดการต่อ
            return false;
        }

        ProblemDetails problemDetails = new ProblemDetails();
        problemDetails.Status = statusCode;
        problemDetails.Title = title;
        problemDetails.Detail = exception.Message;

        // ProblemDetailsContext กำหนดค่าได้เฉพาะตอนสร้าง (init-only) จึงต้องใส่ใน { } ตอน new
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
