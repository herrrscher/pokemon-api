using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace poke_db.Security
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly Dictionary<string, List<DateTime>> _clientRequests = new();
        private static readonly int _maxRequestsPerMinute = 100;
        private static readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);

            if (IsRateLimited(clientId))
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsRateLimited(string clientId)
        {
            var now = DateTime.UtcNow;

            if (!_clientRequests.ContainsKey(clientId))
            {
                _clientRequests[clientId] = new List<DateTime>();
            }

            var requests = _clientRequests[clientId];

            requests.RemoveAll(time => now - time > _timeWindow);

            if (requests.Count >= _maxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                return true;
            }

            requests.Add(now);
            return false;
        }
    }

    public class ApiKeyAuthenticationFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthenticationFilter> _logger;

        public ApiKeyAuthenticationFilter(IConfiguration configuration, ILogger<ApiKeyAuthenticationFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //? This code below in not used in the current files but it is called by the ASP.NET Core framework
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var path = context.HttpContext.Request.Path.Value?.ToLower();
            if (path == "/" || path?.StartsWith("/health") == true || path?.StartsWith("/swagger") == true)
            {
                return;
            }

            var apiKey = context.HttpContext.Request.Headers["X-API-Key"].FirstOrDefault();
            var validApiKeys = _configuration.GetSection("ApiKeys").Get<string[]>() ?? Array.Empty<string>();

            if (string.IsNullOrEmpty(apiKey) || !validApiKeys.Contains(apiKey))
            {
                _logger.LogWarning("Unauthorized API access attempt from {IP}",
                    context.HttpContext.Connection.RemoteIpAddress);

                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API key" });
            }
        }
    }
}