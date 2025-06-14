using System.Text.RegularExpressions;
namespace Red55.Mattermost.OpenId.Proxy.Extensions
{
    public static class Regexp
    {

        public static bool IsValid(string pattern)
        {
            try
            {
                var regex = new Regex (pattern);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
