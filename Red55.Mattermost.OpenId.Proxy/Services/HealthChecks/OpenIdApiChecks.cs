
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Red55.Mattermost.OpenId.Proxy.Models;

namespace Red55.Mattermost.OpenId.Proxy.Services.HealthChecks;

public class OpenIdApiReadinessCheck(IOptions<AppConfig> config,
    ILogger<OpenIdApiReadinessCheck> log,
    IHttpClientFactory httpClientFactory) : IHealthCheck
{
    protected AppConfig Config { get; } = config.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var http = _httpClientFactory.CreateClient ("OpenIdApiChecks");

        try
        {
            var r = await http.GetAsync (Config.OpenId.HealthChecks.ReadinessUrl, cancellationToken);
            if (r.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy ("OpenId API is ready.");
            }
            log.LogWarning ("OpenId API readiness check returned status code {StatusCode}.", r.StatusCode);
            return HealthCheckResult.Unhealthy ($"OpenId API returned status code {r.StatusCode}.");
        }
        catch (Exception ex)
        {
            log.LogError (ex, "OpenId API readiness check failed.");
            return HealthCheckResult.Unhealthy ("OpenId API readiness check failed.", ex);
        }
    }
}


public class OpenIdApiLivenessCheck(IOptions<AppConfig> config,
    ILogger<OpenIdApiLivenessCheck> log,
    IHttpClientFactory httpClientFactory) : IHealthCheck
{
    protected AppConfig Config { get; } = config.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var http = _httpClientFactory.CreateClient ("OpenIdApiChecks");
        try
        {
            var r = await http.GetAsync (Config.OpenId.HealthChecks.LivenessUrl, cancellationToken);
            if (r.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy ("OpenId API is alive.");
            }
            log.LogWarning ("OpenId API liveness check returned status code {StatusCode}.", r.StatusCode);
            return HealthCheckResult.Unhealthy ($"OpenId API returned status code {r.StatusCode}.");
        }
        catch (Exception ex)
        {
            log.LogError (ex, "OpenId API liveness check failed.");
            return HealthCheckResult.Unhealthy ("OpenId API Liveness check failed.", ex);
        }
    }
}