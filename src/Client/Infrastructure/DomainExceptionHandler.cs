using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Client.Infrastructure;

// The ONE place the exception→status-code contract lives. Controllers raise domain exceptions and
// never wrap actions in per-action try/catch, so the error contract stays identical across every
// endpoint.
internal sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);
        if (status is null)
            return false; // not a known domain exception — let the default handler 500 it.

        httpContext.Response.StatusCode = status.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status.Value,
                Title = title,
                Detail = exception.Message,
            },
        });
    }

    private static (int? status, string? title) Map(Exception exception) => exception switch
    {
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
        KeyNotFoundException        => (StatusCodes.Status404NotFound, "Not Found"),
        // Aggregate invariant / business-rule violations are bad requests. Bill-sourced
        // calendar entries being read-only also surfaces as InvalidOperationException → 400.
        InvalidOperationException   => (StatusCodes.Status400BadRequest, "Bad Request"),
        ArgumentException           => (StatusCodes.Status400BadRequest, "Bad Request"),
        _                           => (null, null),
    };
}
