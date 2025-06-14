using System.Text.RegularExpressions;

using Microsoft.Extensions.Primitives;

namespace Red55.Mattermost.OpenId.Proxy.Transforms
{
    public static class HeadersHelper
    {
        public static void TransformHeaders(IHeaderDictionary headers,
            Regex match,
            string replacement,
            Func<string, StringValues> take,
            Func<string, bool> shouldRemove,
            Action<string, StringValues> put)
        {
            var hdrs = headers.Select (h => h.Key).ToArray ();
            foreach (var h in hdrs)
            {
                var vals = take (h);
                if (shouldRemove != null && shouldRemove (h))
                {
                    continue;
                }
                if (vals.Count > 0)
                {
                    var r = new string[vals.Count];
                    for (var i = 0; i < vals.Count; ++i)
                    {
                        r[i] = match.Replace (vals[i]!, replacement);
                    }
                    put (h, r);
                }
                else
                {
                    put (h, StringValues.Empty);
                }
            }
        }
    }
}
