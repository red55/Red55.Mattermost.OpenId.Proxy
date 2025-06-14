using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Request
{
    public abstract class BaseRequestTransform(ILogger logger) : RequestTransform
    {
        protected ILogger Log { get; } = EnsureArg.IsNotNull (logger, nameof (logger));

        protected virtual bool ShouldApply(RequestTransformContext context)
        {
            _ = EnsureArg.IsNotNull (context, nameof (context));
            return true;
        }
    }
}
