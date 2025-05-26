using Red55.Mattermost.OpenId.Proxy.Models.Gitlab;

using Refit;

namespace Red55.Mattermost.OpenId.Proxy.Api.Gitlab
{
    [Headers ("Authorization: Bearer")]
    public interface IUser
    {
        [Get ("/api/v4/user")]
        Task<ApiResponse<User>> GetUserAsync(CancellationToken cancellationToken);

        [Get ("/api/v4/users/{id}")]
        Task<ApiResponse<User>> GetUserAsync(uint id, CancellationToken cancellationToken);
    }
}
