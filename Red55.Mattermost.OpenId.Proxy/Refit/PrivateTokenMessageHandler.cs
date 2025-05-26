using Microsoft.Extensions.Options;

namespace Red55.Mattermost.OpenId.Proxy.Refit
{
    public class PrivateTokenMessageHandler(IOptions<AppConfig> config_) : DelegatingHandler
    {
        protected AppConfig Config { get; } = EnsureArg.IsNotNull (config_.Value, nameof (config_));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authorization = request.Headers.Authorization;
            if (authorization?.Parameter != null && authorization.Scheme == "Bearer")
            {
                // If the request already has an Authorization header, we don't need to add the Private-Token header
                return await base.SendAsync (request, cancellationToken);
            }

            _ = request.Headers.Remove ("Private-Token");
            request.Headers.Add ("Private-Token", Config.Gitlab.AppSecret);
            return await base.SendAsync (request, cancellationToken);
        }
    }
}
