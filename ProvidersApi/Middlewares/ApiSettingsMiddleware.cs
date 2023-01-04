using Microsoft.Extensions.Options;

namespace ProvidersApi.Middlewares
{
    public class ApiSettingsMiddleware
    {
        private readonly ApiSettings _apiSettings;
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-Api-Key";

        public ApiSettingsMiddleware(RequestDelegate requestDelegate, IOptions<ApiSettings> options)
        {
            _next = requestDelegate;
            _apiSettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;

                await context.Response.WriteAsync("Key was not provided.");

                return;
            }

            var apiKey = _apiSettings?.ApiKey;

            if (apiKey != extractedApiKey)
            {
                context.Response.StatusCode = 401;

                await context.Response.WriteAsync("Unauthorized Client.");

                return;
            }

            await _next(context);
        }

    }
}
