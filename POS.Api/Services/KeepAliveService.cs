using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace POS.Api.Services;

public class KeepAliveService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeepAliveService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public KeepAliveService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<KeepAliveService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Keep-Alive Service is starting.");

        // Wait a bit before the first ping to let the app fully start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var appUrl = Environment.GetEnvironmentVariable("APP_URL") 
                             ?? _configuration["APP_URL"];

                if (string.IsNullOrEmpty(appUrl))
                {
                    _logger.LogWarning("APP_URL is not configured. Skipping keep-alive ping.");
                }
                else
                {
                    _logger.LogInformation("🔥 Running Service Warmup (Self-Ping) to {Url}...", appUrl);
                    
                    using var client = _httpClientFactory.CreateClient();
                    // Ensure URL ends with / if needed, but we'll append the path
                    var pingUrl = $"{appUrl.TrimEnd('/')}/api/health/ping";
                    
                    var response = await client.GetAsync(pingUrl, stoppingToken);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("✅ Keep-Alive Ping Successful: {StatusCode}", response.StatusCode);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Keep-Alive Ping Failed: {StatusCode}", response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error occurred during keep-alive ping.");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Keep-Alive Service is stopping.");
    }
}
