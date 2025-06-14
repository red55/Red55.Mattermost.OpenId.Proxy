using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Response
{
    public class ReplaceInResponseTransform(string match, string replacement,
        ILogger<ReplaceInResponseTransform> logger) : BaseResponseTransform (logger)
    {
        private static readonly IReadOnlyCollection<string> _myMimeTypes =
        [
            "text/html", "application/json", "application/xml", "text/plain", "text/json", "text/javascript", "text/css"
        ];

        internal Regex Match { get; } = new Regex (EnsureArg.IsNotNullOrEmpty (match, nameof (match)),
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal string Replacement { get; } = EnsureArg.IsNotNullOrEmpty (replacement, nameof (replacement));

        public override async ValueTask ApplyAsync(ResponseTransformContext context)
        {
            if (!ShouldApply (context, _myMimeTypes))
            {
                return;
            }

            Log.Applying (nameof (ReplaceInResponseTransform), Match.ToString (),
                Replacement, context.HttpContext.Request.Path);

            var contentEncoding = context.HttpContext.Response.Headers.ContentEncoding.ToString ().ToLowerInvariant ();


            HeadersHelper.TransformHeaders (context.HttpContext.Response.Headers,
                Match,
                Replacement,
                h => TakeHeader (context, h),
                _ => false,
                (h, vals) => SetHeader (context, h, vals)
            );

            var bodyStream = await context.ProxyResponse!.Content.ReadAsStreamAsync ();
            using MemoryStream decodedBodyStream = new (), encodedBodyStream = new ();
            Stream readStream, writeStream;


#pragma warning disable CA2000 // Dispose objects before losing scope
            switch (contentEncoding)
            {
                case "gzip":
                    readStream = new GZipStream (bodyStream, CompressionMode.Decompress, leaveOpen: false);
                    writeStream = new GZipStream (encodedBodyStream, CompressionLevel.Optimal, leaveOpen: true);
                    break;
                case "deflate":
                    readStream = new DeflateStream (bodyStream, CompressionMode.Decompress, leaveOpen: false);
                    writeStream = new DeflateStream (encodedBodyStream, CompressionLevel.Optimal, leaveOpen: true);
                    break;
                case "br":
                    readStream = new BrotliStream (bodyStream, CompressionMode.Decompress, leaveOpen: false);
                    writeStream = new BrotliStream (encodedBodyStream, CompressionLevel.Optimal, leaveOpen: true);
                    break;
                default:
                    readStream = bodyStream;
                    writeStream = encodedBodyStream;
                    break;
            }
#pragma warning restore CA2000 // Dispose objects before losing scope
#pragma warning disable S6966 // The stream is disposed in the finally block, so it is safe to use it here.
            try
            {
                context.SuppressResponseBody = true;

                await readStream.CopyToAsync (decodedBodyStream, context.CancellationToken);

                var sResponseBody = Encoding.UTF8.GetString (decodedBodyStream.GetBuffer ()).TrimEnd ((char)0);

                sResponseBody = Match.Replace (sResponseBody, Replacement);

                writeStream.Write (Encoding.UTF8.GetBytes (sResponseBody));
                writeStream.Flush ();

                context.HttpContext.Response.ContentLength = encodedBodyStream.Length;

                encodedBodyStream.Position = 0;
                await encodedBodyStream.CopyToAsync (context.HttpContext.Response.Body);
            }
            catch (Exception e)
            {
                Log.LogError (e, "{Class}.{Method} {Url} {Message}", nameof (ReplaceInResponseTransform), nameof (ApplyAsync),
                    context.HttpContext.Request.Path, e.Message);
                throw;
            }
            finally
            {
                if (readStream != bodyStream)
                {
                    readStream.Dispose ();
                }

                writeStream.Dispose ();
            }
#pragma warning restore S6966 
        }
    }
}
