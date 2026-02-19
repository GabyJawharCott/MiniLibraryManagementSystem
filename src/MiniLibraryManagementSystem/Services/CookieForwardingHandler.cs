using System.Net.Http.Headers;

namespace MiniLibraryManagementSystem.Services;

/// <summary>Forwards the current request's cookies to the outgoing API request (Blazor Server calling same app).</summary>
public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.Cookie is { } cookieHeader && !string.IsNullOrEmpty(cookieHeader))
            request.Headers.Add("Cookie", cookieHeader.ToString());

        return await base.SendAsync(request, cancellationToken);
    }
}
