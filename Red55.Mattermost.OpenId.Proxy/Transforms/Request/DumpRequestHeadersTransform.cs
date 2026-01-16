using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Request;

public class DumpRequestHeadersTransform(LogLevel level, ILogger logger) : BaseRequestTransform (logger)
{
    LogLevel Level => level;
    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        HeadersHelper.DumpHeaders (context.HttpContext.Request.Headers, Log, Level, "RQH");
        return ValueTask.CompletedTask;
    }
}
