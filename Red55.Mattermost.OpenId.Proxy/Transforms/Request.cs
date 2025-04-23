using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    public class Request(ILogger<Request> _log) : Yarp.ReverseProxy.Transforms.RequestTransform
    {
        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            _log.LogInformation ("{method} {url}", context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            _log.LogInformation ("{headers}", context.HttpContext.Request.Headers);
            _log.LogInformation ("{body}", context.HttpContext.Request.Body.ToString ());

            return base.ApplyAsync (context);
        }
    }
}
