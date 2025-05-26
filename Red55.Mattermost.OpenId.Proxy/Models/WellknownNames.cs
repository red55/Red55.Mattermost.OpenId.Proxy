namespace Red55.Mattermost.OpenId.Proxy.Models
{
    public class WellknownNames
    {
        private static readonly IReadOnlyList<string> _InterceptRequestUrls = ["/api/v4/user", "/oauth/authorize", "/oauth/token"];

        public static IReadOnlyList<string> InterceptRequestUrls { get => _InterceptRequestUrls; }
    }
}
