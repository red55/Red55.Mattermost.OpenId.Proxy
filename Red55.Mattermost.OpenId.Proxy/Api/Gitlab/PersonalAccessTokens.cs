using Red55.Mattermost.OpenId.Proxy.Models.Gitlab;

using Refit;

namespace Red55.Mattermost.OpenId.Proxy.Api.Gitlab
{

    [Headers ("Authorization: Bearer")]
    public interface IPersonalAccessTokens
    {

        [Get ("/api/v4/personal_access_tokens/self")]
        Task<ApiResponse<PersonalAccessToken>> GetSelfAsync(CancellationToken cancellationToken);
        [Post ("/api/v4/personal_access_tokens/self/rotate")]
        Task<ApiResponse<PersonalAccessToken>> RotateSelfAsync(CancellationToken cancellationToken);

    }
}
