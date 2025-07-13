using Red55.Mattermost.OpenId.Proxy.Models;

namespace Red55.Mattermost.OpenId.Proxy.Extensions
{
    public static class UriExtensions
    {
        public static bool IsNullOrEmpty(this Uri? uri)
        {
            return uri is null || AppConfig.EmptyUri == uri;
        }
    }
}
