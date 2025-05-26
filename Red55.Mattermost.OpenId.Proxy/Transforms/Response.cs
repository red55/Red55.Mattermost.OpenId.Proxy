using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    public class Response : ResponseTransform
    {
        static string RemoveCoookieAttr([NotNull] string cookieValue, string attr)
        {
            if (!string.IsNullOrEmpty (cookieValue) && cookieValue.Contains (attr, StringComparison.OrdinalIgnoreCase))
            {
                return cookieValue.Replace (attr, "", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return cookieValue;
            }
        }
        public override ValueTask ApplyAsync(ResponseTransformContext context)
        {
            var cookies = TakeHeader (context, "Set-Cookie");

            if (cookies.Count == 0)
            {
                return ValueTask.CompletedTask;
            }

            string[] newCookies = new string[cookies.Count];

            for (var i = 0; i < cookies.Count; i++)
            {
                // Remove the Secure flag from cookies to allow them to be sent over HTTP             
                newCookies[i] = RemoveCoookieAttr (RemoveCoookieAttr (cookies[i], "Secure"), "SameSite=None");

            }
            var cookieVals = new StringValues (newCookies);
            SetHeader (context, "Set-Cookie", cookieVals);

            context.ProxyResponse?.Headers.Remove ("Set-Cookie");
            context.ProxyResponse?.Headers.Add ("Set-Cookie", cookieVals.AsEnumerable ());

            return ValueTask.CompletedTask;
        }
    }
}
