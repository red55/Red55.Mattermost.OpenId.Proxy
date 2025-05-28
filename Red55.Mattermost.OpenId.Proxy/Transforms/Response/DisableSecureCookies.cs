// Ignore Spelling: Mattermost

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Primitives;

using Red55.Mattermost.OpenId.Proxy.Models;

using Yarp.ReverseProxy.Transforms;

namespace Red55.Mattermost.OpenId.Proxy.Transforms.Response
{
    public class DisableSecureCookies : ResponseTransform
    {
        ILogger<DisableSecureCookies> Logger { get; }

        public DisableSecureCookies(ILogger<DisableSecureCookies> logger)
        {
            
            Logger = EnsureArg.IsNotNull(logger);
        }

        static string RemoveCookieAttr([NotNull] string cookieValue, string attr)
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
            var cookies = TakeHeader (context, WellKnownNames.SetCookieHeader);
            
            if (cookies.Count == 0)
            {
                Logger.LogDebug ("No Set-Cookie headers found in the response.");
                return ValueTask.CompletedTask;
            }

            var newCookies = new string[cookies.Count];

            for (var i = 0; i < cookies.Count; i++)
            {
                if (!string.IsNullOrEmpty(cookies[i]))
                {
                    // Remove the Secure flag from cookies to allow them to be sent over HTTP             
                    newCookies[i] = RemoveCookieAttr (RemoveCookieAttr (cookies[i] ?? "", "Secure"), "SameSite=None");
                }

            }
            var cookieVals = new StringValues (newCookies);
            SetHeader (context, WellKnownNames.SetCookieHeader, cookieVals);

            context.ProxyResponse?.Headers.Remove (WellKnownNames.SetCookieHeader);
            context.ProxyResponse?.Headers.Add (WellKnownNames.SetCookieHeader, cookieVals.AsEnumerable ());

            return ValueTask.CompletedTask;
        }
    }
}
