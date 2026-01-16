using Red55.Mattermost.OpenId.Proxy.Transforms.Request;
using Red55.Mattermost.OpenId.Proxy.Transforms.Response;

using Yarp.ReverseProxy.Transforms.Builder;

namespace Red55.Mattermost.OpenId.Proxy.Transforms;

public class DumpHeadersTransformFactory(ILoggerFactory loggerFactory) : ITransformFactory
{

    internal readonly string DumpRequestHeaders = nameof (DumpRequestHeaders);
    internal readonly string DumpResponseHeaders = nameof (DumpResponseHeaders);
    internal readonly string InvalidDumpHeadersValue =
                        "Unexpected value for {0}: {1}. Expected one of LogLevel";

    ILoggerFactory LoggerFactory { get; } = EnsureArg.IsNotNull (loggerFactory, nameof (loggerFactory));

    private bool BuildTransform(string name, IReadOnlyDictionary<string, string> transformValues, Action<LogLevel> add)
    {
        if (transformValues.TryGetValue (name, out var logLevelString))
        {

            if (!Enum.TryParse<LogLevel> (logLevelString, out var logLevel))
            {
                throw new NotSupportedException (string.Format (InvalidDumpHeadersValue,
                    name, logLevelString));
            }

            add (logLevel);
            return true;
        }

        return false;
    }
    public bool Build(TransformBuilderContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        var b = BuildTransform (DumpRequestHeaders, transformValues, logLevel =>
        {
            context.RequestTransforms.Add (new DumpRequestHeadersTransform (logLevel,
                LoggerFactory.CreateLogger<DumpRequestHeadersTransform> ()));
        });
        var b1 = BuildTransform (DumpResponseHeaders, transformValues, logLevel =>
        {
            context.ResponseTransforms.Add (new DumpResponseHeadersTransform (logLevel,
                LoggerFactory.CreateLogger<DumpResponseHeadersTransform> ()));
        });


        return b || b1;
    }

    public bool Validate(TransformRouteValidationContext context, IReadOnlyDictionary<string, string> transformValues)
    {
        var dumpHeaders = string.Empty;

        if (transformValues.TryGetValue (DumpRequestHeaders, out dumpHeaders)
            || transformValues.TryGetValue (DumpResponseHeaders, out dumpHeaders))
        {
            TransformHelpers.TryCheckTooManyParameters (context, transformValues, 1);

            if (!Enum.TryParse<LogLevel> (dumpHeaders, out var _))
            {
                context.Errors.Add (
                    new ArgumentException (
                        $"Unexpected value for {DumpRequestHeaders}: {dumpHeaders}. Expected one of LogLevel")
                );
            }

            return true;
        }

        return false;
    }
}