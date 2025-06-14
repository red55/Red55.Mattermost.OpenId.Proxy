// Ignore Spelling: Mattermost Api

using System.IdentityModel.Tokens.Jwt;
using System.Net;

using Microsoft.AspNetCore.Mvc;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;
using Red55.Mattermost.OpenId.Proxy.Extensions;
using Red55.Mattermost.OpenId.Proxy.Extensions.GitLab;
using Red55.Mattermost.OpenId.Proxy.Models.Gitlab;

namespace Red55.Mattermost.OpenId.Proxy.Controllers.api.v4
{
    [Route ("/api/v4/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Returns the current user.
        /// </summary>
        /// <returns>The current user.</returns>
        [HttpGet ()]
        [ProducesResponseType (StatusCodes.Status200OK)]
        [ProducesResponseType (StatusCodes.Status404NotFound)]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserBase>> Get([FromHeader] string authorization,
            [FromServices] ILogger<UserController> log,
            [FromServices] IUser userApi,
            CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty (authorization) || !authorization.StartsWith ("Bearer "))
            {
                return Problem (statusCode: StatusCodes.Status400BadRequest, detail: "Authorization header is required");
            }

            try
            {
                var token = new JwtSecurityTokenHandler ().ReadJwtToken (authorization.Split ("Bearer ")[1]);
                var email = token.GetClaimValue (JwtRegisteredClaimNames.Email);
                var ur = await userApi.GetUsersAsync (email,
                    cancellationToken: cancellation);

                await ur.EnsureSuccessfulAsync ();

                UserBase r;
                if (ur.StatusCode == HttpStatusCode.NotFound || 0 == (ur.Content?.Count ?? 0))
                {
                    r = new UserBase ()
                    {
                        Name = token.GetClaimValue (JwtRegisteredClaimNames.Name),
                        Username = token.GetClaimValue (JwtRegisteredClaimNames.PreferredUsername),
                        Email = email,
                    };

                    var u = await userApi.CreateUserAsync (r, cancellation);
                    await u.EnsureSuccessfulAsync ();

                    if (u.Content is null)
                    {
                        return this.Problem (statusCode: 500, detail: "Failed to create user");
                    }

                    r.Id = u.Content.Id;
                    r.AvatarUrl = u.Content?.AvatarUrl ?? string.Format (UserBase.DEFAULT_PICTURE_URL,
                        u?.Content?.EmailHash () ?? UserBase.DEFAULT_PICTURE_S256);
                }
                else
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    r = ur.Content[0];
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                }

                return r;
            }
            catch (Exception e)
            {
                log.LogError (e, "Call to User details failed");
                return this.Problem (statusCode: 500, detail: e.Message);
            }
        }
    }
}
