using Microsoft.AspNetCore.Http;
using System.Text;

namespace PostgresKeyValueStore.Library
{
    public class StoreMiddleware
    {
        private readonly RequestDelegate _next;
        private string _userName;
        private string _password;
        private string _uiAddress;

        public StoreMiddleware(RequestDelegate next, string userName, string password, string uiAddress)
        {
            _next = next;
            _userName = userName;
            _password = password;
            _uiAddress = uiAddress;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments($"/{_uiAddress}") || context.Request.Path.StartsWithSegments($"/StoreConfiguration"))
            {

                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    // Authorization header'ını çöz
                    string encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                    string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                    string[] parts = credentials.Split(':', 2);

                    // Kullanıcı adı ve şifre kontrolü
                    string username = parts[0];
                    string password = parts[1];
                    if (username == _userName && password == _password)
                    {
                        if (context.Request.Path == $"/{_uiAddress}" || context.Request.Path == $"/{_uiAddress}/")
                        {
                            context.Response.ContentType = "text/html";
                            await context.Response.WriteAsync((await GetEmbeddedHtml("index.html")).Replace("{uiaddress}", _uiAddress).Replace("{authorizationKey}", encodedCredentials));
                            return;
                        }
                        else if (context.Request.Path.StartsWithSegments("/StoreConfiguration"))
                        {
                            await _next(context);
                            return;
                        }

                        // Static files for JS, CSS, etc.
                        var filePath = context.Request.Path.Value.Replace($"/{_uiAddress}/", "");
                        var resourceStream = GetEmbeddedResource(filePath);
                        if (resourceStream != null)
                        {
                            var fileExtension = Path.GetExtension(filePath);
                            context.Response.ContentType = GetContentType(fileExtension);
                            await resourceStream.CopyToAsync(context.Response.Body);
                            return;
                        }

                        context.Response.StatusCode = 404;
                        return;
                    }
                }

                // Yetkilendirme başarısız ise 401 Unauthorized döndür
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Store\"";
                await context.Response.WriteAsync("Unauthorized");
                return;

            }

            await _next(context);
        }

        private string GetContentType(string fileExtension) =>
            fileExtension switch
            {
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".html" => "text/html",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

        private async Task<string> GetEmbeddedHtml(string fileName)
        {
            var resourceName = $"PostgresKeyValueStore.Library.UI.StaticFiles.{fileName}";
            using var stream = typeof(StoreMiddleware).Assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return string.Empty;

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private Stream GetEmbeddedResource(string fileName)
        {
            var resourceName = $"PostgresKeyValueStore.Library.UI.StaticFiles.{fileName}";
            return typeof(StoreMiddleware).Assembly.GetManifestResourceStream(resourceName);
        }
    }
}
