using Microsoft.AspNetCore.Mvc;

using Red55.Mattermost.OpenId.Proxy.Api.Gitlab;

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
        public async Task<IActionResult> Get([FromServices] IUser gitlabUserService,
            [FromHeader] string authorization,
            [FromServices] ILogger<UserController> log,
            CancellationToken cancellation)
        {
            EnsureArg.IsNotNull (gitlabUserService, nameof (gitlabUserService));

            try
            {
                var user = await gitlabUserService.GetUserAsync (2, cancellation);
                await user.EnsureSuccessStatusCodeAsync ();

                if (user.Error != null)
                {
                    return BadRequest (user.Error);
                }

                return Ok (user.Content);
            }
            catch (Exception e)
            {
                log.LogError (e, "Call to User details failed");
                return this.Problem (statusCode: 500, detail: e.Message);
            }
        }
    }
}
