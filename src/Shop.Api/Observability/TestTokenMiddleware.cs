using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Context;

namespace Shop.Api.Observability;

public sealed class TestTokenMiddleware
{
    public const string HeaderName = "X-Test-Token";

    private readonly RequestDelegate _next;

    public TestTokenMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var token = TryGetToken(context.Request.Headers);
        if (!string.IsNullOrWhiteSpace(token))
        {
            context.Response.Headers[HeaderName] = token!;
        }

        using (LogContext.PushProperty("test_token", token ?? string.Empty))
        {
            Log.Information("HTTP {Method} {Path}", context.Request.Method, context.Request.Path.Value);
            await _next(context);
        }
    }

    private static string? TryGetToken(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderName, out StringValues values))
            return null;
        var token = values.FirstOrDefault();
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }
}

