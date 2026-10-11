namespace Solution.Web.Middleware;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

public class SecurityHeadersMiddleware
{
    public const string CspNonceKey = "CSP_NONCE";
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Generate a cryptographically secure 128-bit (16-byte) CSP nonce for the request
        byte[] nonceBytes = new byte[16];
        RandomNumberGenerator.Fill(nonceBytes);
        string cspNonce = Convert.ToBase64String(nonceBytes);

        // Store nonce in HttpContext.Items for Razor views consumption
        context.Items[CspNonceKey] = cspNonce;

        // Apply OWASP A05 recommended Security Headers
        IHeaderDictionary headers = context.Response.Headers;

        // Content Security Policy (CSP) with nonce-based strict script and style execution
        headers.ContentSecurityPolicy =
            $"default-src 'self'; " +
            $"script-src 'self' 'nonce-{cspNonce}'; " +
            $"style-src 'self' 'nonce-{cspNonce}'; " +
            $"img-src 'self' data:; " +
            $"font-src 'self'; " +
            $"connect-src 'self'; " +
            $"object-src 'none'; " +
            $"base-uri 'self'; " +
            $"form-action 'self'; " +
            $"frame-ancestors 'none'; " +
            $"upgrade-insecure-requests;";

        // Prevent clickjacking
        headers.XFrameOptions = "DENY";

        // Prevent MIME type sniffing
        headers.XContentTypeOptions = "nosniff";

        // Strict referrer policy
        headers[HeaderNames.Referer] = "strict-origin-when-cross-origin";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Restrict sensitive browser permissions
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // HTTP Strict Transport Security (HSTS)
        headers[HeaderNames.StrictTransportSecurity] = "max-age=31536000; includeSubDomains; preload";

        // Remove Server disclosure headers
        headers.Remove(HeaderNames.Server);
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");

        await _next(context).ConfigureAwait(false);
    }
}
