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

        [Get ("/api/v4/users")]
        Task<ApiResponse<IReadOnlyList<User>>> GetUsersAsync(bool active = true,bool exclude_internal= true, 
            bool without_project_bots= true, CancellationToken cancellationToken = default);

        [Get ("/api/v4/users")]
        Task<ApiResponse<IReadOnlyList<User>>> GetUsersAsync(string search, bool active = true, bool exclude_internal = true,
            bool without_project_bots = true, CancellationToken cancellationToken = default);

        [Post ("/api/v4/users")]
        Task<ApiResponse<User>> CreateUserAsync([Body]UserBase u, CancellationToken cancellationToken);

        //[Post("/api/v4/users")] 

    }
}
