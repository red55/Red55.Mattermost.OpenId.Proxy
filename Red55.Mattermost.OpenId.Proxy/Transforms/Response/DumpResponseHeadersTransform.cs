using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Response;

public class DumpResponseHeadersTransform(LogLevel logLevel, ILogger logger) : BaseResponseTransform (logger)
{
    LogLevel LogLevel => logLevel;
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        HeadersHelper.DumpHeaders (context.HttpContext.Response.Headers, Log, LogLevel, "RPH");

        return ValueTask.CompletedTask;
    }
}
