namespace ExamApp.Api.Infrastructure.Middleware;

public static class SecurityHeadersMiddleware
{
    private const string PageCsp =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; " +
        "font-src 'self'; connect-src 'self'; base-uri 'self'; form-action 'self'; " +
        "frame-ancestors 'none'";

    private const string ApiCsp = "default-src 'none'; frame-ancestors 'none'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            var path = context.Request.Path;
            if (path.StartsWithSegments("/api"))
            {
                headers["Content-Security-Policy"] = ApiCsp;
            }
            else if (!path.StartsWithSegments("/swagger"))
            {
                headers["Content-Security-Policy"] = PageCsp;
            }

            await next();
        });
    }
}
