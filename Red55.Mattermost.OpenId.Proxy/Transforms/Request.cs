using Red55.Mattermost.OpenId.Proxy.Models;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    public class Request(ILogger<Request> _log) : Yarp.ReverseProxy.Transforms.RequestTransform
    {

        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            // TODO: Transform request headers and Path to reflect keycloak configuration. E.g.: /oauth/token to /realms/{realm}/protocol/openid-connect/token
            if (context.HttpContext.Request.Path.HasValue
                && WellknownNames.InterceptRequestUrls.All (url => !context.HttpContext.Request.Path.Value.EndsWith (url)))
            {
                return ValueTask.CompletedTask;
            }

            _log.LogDebug ("{Method} {Url}", context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            _log.LogDebug ("{Headers}", context.HttpContext.Request.Headers);
            _log.LogDebug ("{Body}", context.HttpContext.Request.Body.ToString ());




            _log.LogInformation ("InterceptRequestUrls processing completed for {Url}", context.HttpContext.Request.Path);
            return ValueTask.CompletedTask;
        }
    }
}
