using Microsoft.AspNetCore.Mvc;

namespace Red55.Mattermost.OpenId.Proxy.Controllers.OpenId
{
    [Route ("oauth/callback")]
    [ApiController]
    public class Callback : ControllerBase
    {
        [HttpGet ()]
        [Route ("gitlab")]
        public IActionResult GetGitlab()
        {
            return Ok ();
        }
    }
}
