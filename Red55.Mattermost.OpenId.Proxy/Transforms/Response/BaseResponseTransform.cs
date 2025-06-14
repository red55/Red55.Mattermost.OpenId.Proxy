using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Response
{
    internal static partial class ResponseLogging
    {
        [LoggerMessage (Level = LogLevel.Debug,
            Message = "ProxyResponse is null, skipping transform")]
        internal static partial void ProxyResponseIsNull(this ILogger logger);

        [LoggerMessage (Level = LogLevel.Debug,
            Message = "Response status code is not in the successful: {StatusCode}")]
        internal static partial void ResponseStatusCodeIsBad(this ILogger logger, System.Net.HttpStatusCode statusCode);

        [LoggerMessage (Level = LogLevel.Debug,
            Message = "No matching Content-Type header found to transform")]
        internal static partial void NoMatchingContentType(this ILogger logger);

        [LoggerMessage (Level = LogLevel.Debug,
            Message = "Transform not applied for {Path} with Content-Type: {ContentType}")]
        internal static partial void TransformNotApplied(this ILogger logger, string path, string contentType);

        [LoggerMessage (Level = LogLevel.Information,
            Message = "Applying {Transform}: {Match}, {Replacement}, {Path}")]
        internal static partial void Applying(this ILogger logger, string transform, string match, string replacement, string path);

    }

    public abstract class BaseResponseTransform(ILogger logger) : ResponseTransform
    {

        protected ILogger Log { get; } = EnsureArg.IsNotNull (logger, nameof (logger));
        protected virtual bool ShouldApply(ResponseTransformContext context, IReadOnlyCollection<string> contentTypes)
        {
            _ = EnsureArg.IsNotNull (context, nameof (context));
            var contentType = context.ProxyResponse?.Content.Headers?.ContentType?.MediaType ?? "none";
            var bShouldApply = false;

            if (context.ProxyResponse is null)
            {
                Log.ProxyResponseIsNull ();
            }
            else if (context.ProxyResponse.Content is null)
            {
                Log.LogDebug ("ProxyResponse.Content is null, skipping transform for {Path}",
                    context.HttpContext.Request.Path);
            }
            else if (!context.ProxyResponse.IsSuccessStatusCode)
            {
                Log.ResponseStatusCodeIsBad (context.ProxyResponse.StatusCode);
            }
            else if (contentType == "none" || (contentTypes.Count > 0 && !contentTypes.Contains (contentType)))
            {
                Log.NoMatchingContentType ();
            }
            else
            {
                bShouldApply = true;
            }

            if (!bShouldApply)
            {
                Log.TransformNotApplied (context.HttpContext.Request.Path, contentType);
            }

            return bShouldApply;
        }
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private static bool TryGetValues(HttpHeaders headers, string headerName, out StringValues values)
        {
            if (headers.NonValidated.TryGetValues (headerName, out var values2))
            {
                if (values2.Count <= 1)
                {
                    values = values2.ToString ();
                }
                else
                {
                    values = ToArray (in values2);
                }

                return true;
            }

            values = default;
            return false;
            static StringValues ToArray(in HeaderStringValues values)
            {
                string[] array = new string[values.Count];
                int num = 0;
                foreach (string value in values)
                {
                    array[num++] = value;
                }

                return array;
            }
        }

        public static StringValues GetHeader(ResponseTransformContext context, string headerName)
        {
            _ = EnsureArg.IsNotNull (context, nameof (context));
            _ = EnsureArg.IsNotNullOrEmpty (headerName, nameof (headerName));

            if (context.HttpContext.Response.Headers.TryGetValue (headerName, out var value))
            {
                return value;
            }
            else
            {
                HttpResponseMessage proxyResponse = EnsureArg.IsNotNull (context.ProxyResponse);
                if (!context.HeadersCopied && !TryGetValues (proxyResponse.Headers, headerName, out value))
                {
                    TryGetValues (proxyResponse.Content.Headers, headerName, out value);
                }
            }

            return value;
        }
    }
}
