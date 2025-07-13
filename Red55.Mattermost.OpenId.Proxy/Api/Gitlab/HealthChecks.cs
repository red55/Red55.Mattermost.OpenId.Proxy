using Refit;

namespace Red55.Mattermost.OpenId.Proxy.Api.Gitlab
{
    [Headers ("Authorization: Bearer")]
    public interface IGitlabHealthChecks
    {
        enum Checks
        {
            None,
            All = 1
        }

        [Get ("/-/health")]
        Task<ApiResponse<string>> GetHealthAsync(CancellationToken cancellationToken);

        [Get ("/-/liveness")]
        Task<ApiResponse<string>> GetLivenessAsync(CancellationToken cancellationToken);

        [Get ("/-/readiness")]
        Task<ApiResponse<string>> GetReadinessAsync(CancellationToken cancellationToken, Checks all = Checks.All);
    }
}
