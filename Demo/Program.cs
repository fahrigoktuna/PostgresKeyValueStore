using PostgresKeyValueStore.Library;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(a => { });

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.AddConfiguration();

var app = builder.Build();

app.MapControllers();

app.UseRouting();

app.MapGet("/", () => $"PostgresKeyValueStoreApp Configuration Test Value: {builder.Configuration["Test"]}");

app.UseConfigurationUI("admin", "admin");

await app.RunAsync();

namespace PostgresKeyValueStore.Library
{
    public partial class Program { }
}