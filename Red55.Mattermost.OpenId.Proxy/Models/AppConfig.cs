using System.ComponentModel.DataAnnotations;

using Red55.Mattermost.OpenId.Proxy.Models;

namespace Red55.Mattermost.OpenId.Proxy.Models
{
    public record GitlabSettings
    {
        public readonly static GitlabSettings Empty = new ()
        {
            AppId = string.Empty,
            AppSecret = string.Empty,
            Url = new Uri (string.Empty)
        };

        [Required]
        public required Uri Url { get; init; }
        [Required]
        public required string AppId { get; init; }
        [Required]
        public required string AppSecret { get; init; }
    }
}
public class AppConfig
{
    public static readonly string SectionName = nameof (AppConfig);

    [Required]
    public required GitlabSettings Gitlab { get; init; }
}
