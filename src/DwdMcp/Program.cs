using DwdMcp;
using DwdMcp.Services;
using DwdMcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<DwdApiOptions>(
    builder.Configuration.GetSection(DwdApiOptions.SectionName));

// HTTP clients with resilience
builder.Services.AddHttpClient(DwdApiClient.StationHttpClientName, (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DwdApiOptions>>().Value;
    client.BaseAddress = new Uri(options.StationBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DwdMcp", "1.0"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
})
.AddStandardResilienceHandler();

builder.Services.AddHttpClient(DwdApiClient.WarningHttpClientName, (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DwdApiOptions>>().Value;
    client.BaseAddress = new Uri(options.WarningBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DwdMcp", "1.0"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
})
.AddStandardResilienceHandler();

// Services
builder.Services.AddSingleton<IDwdApiClient, DwdApiClient>();

// MCP server with stdio transport
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new()
    {
        Name = "DwdMcp",
        Version = "1.0.0"
    };
})
.WithStdioServerTransport()
.WithTools<StationTools>()
.WithTools<WarningTools>()
.WithTools<ForecastTools>();

// Logging — must go to stderr, not stdout (reserved for MCP stdio transport)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

await builder.Build().RunAsync();
