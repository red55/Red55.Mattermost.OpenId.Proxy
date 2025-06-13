using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Response
{
    public abstract class BaseResponseTransform(ILogger logger) : ResponseTransform
    {
        protected ILogger Log { get; } = EnsureArg.IsNotNull (logger, nameof (logger));

    }
}
