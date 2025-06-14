using System.Text;
using System.Text.RegularExpressions;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Request
{
    public class ReplaceInRequestTransform(string match, string replacement, ILogger logger) : BaseRequestTransform (logger)
    {

        internal Regex Match { get; } = new Regex (EnsureArg.IsNotNullOrEmpty (match, nameof (match)),
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal string Replacement { get; } = EnsureArg.IsNotNullOrEmpty (replacement, nameof (replacement));

        public override async ValueTask ApplyAsync(RequestTransformContext context)
        {
            if (!ShouldApply (context))
            {
                return;
            }

            HeadersHelper.TransformHeaders (context.HttpContext.Request.Headers,
                Match,
                Replacement,
                h => TakeHeader (context, h),
                _ => false,
                (h, vals) => AddHeader (context, h, vals)
            );

            if (context.ProxyRequest.Content is not null && context.HttpContext.Request.Headers.ContentLength > 0)
            {
                var httpContext = context.HttpContext;

                string? requestBody = "";

                using var sr = new StreamReader (httpContext.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                    leaveOpen: true);

                requestBody = await sr.ReadToEndAsync ();

                var replacedContent = Match.Replace (requestBody, Replacement);

                httpContext.Request.Body = new MemoryStream (Encoding.UTF8.GetBytes (replacedContent));
                context.ProxyRequest.Content!.Headers.ContentLength = httpContext.Request.Body.Length;
            }
        }
    }
}
