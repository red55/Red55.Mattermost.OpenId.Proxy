using System.ComponentModel.DataAnnotations;

namespace Red55.Mattermost.OpenId.Proxy.Models
{
    public record GitLabSettings
    {
        public readonly static GitLabSettings Empty = new ()
        {
            PAT = string.Empty,
            Url = new Uri (string.Empty)
        };
        [Required]
        public required Uri Url { get; init; }
        [Required]
        public required string PAT { get; init; }
    }
    public record KeyCloakSettings
    {
        public readonly static KeyCloakSettings Empty = new ()
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

    public class AppConfig
    {
        public static readonly string SectionName = nameof (AppConfig);

        [Required]
        public required KeyCloakSettings KeyCloak { get; init; }


        [Required]
        public required GitLabSettings GitLab { get; init; }
    }
}