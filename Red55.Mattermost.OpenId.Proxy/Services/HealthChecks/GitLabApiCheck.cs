using Microsoft.Extensions.Diagnostics.HealthChecks;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;

namespace Red55.Mattermost.OpenId.Proxy.Services.HealthChecks;
public class GitLabApiReadinessCheck(IGitlabHealthChecks gitlabHealth,
    ILogger<GitLabApiReadinessCheck> log) : IHealthCheck

{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await gitlabHealth.GetReadinessAsync (cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy ("GitLab API is ready.");
            }
            log.LogWarning ("GitLab API readiness check returned status code {StatusCode}.", response.StatusCode);
            return HealthCheckResult.Unhealthy ($"GitLab API returned status code {response.StatusCode}.");
        }
        catch (Exception ex)
        {
            log.LogError (ex, "GitLab API readiness check failed.");
            return HealthCheckResult.Unhealthy ("GitLab API readiness check failed.", ex);
        }
    }
}

public class GitLabApiLivenessCheck(IGitlabHealthChecks gitlabHealth,
    ILogger<GitLabApiLivenessCheck> log) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await gitlabHealth.GetLivenessAsync (cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy ("GitLab API is alive.");
            }
            log.LogWarning ("GitLab API liveness check returned status code {StatusCode}.", response.StatusCode);
            return HealthCheckResult.Unhealthy ($"GitLab API returned status code {response.StatusCode}.");
        }
        catch (Exception ex)
        {
            log.LogError (ex, "GitLab API liveness check failed.");
            return HealthCheckResult.Unhealthy ("GitLab API liveness check failed.", ex);
        }
    }
}