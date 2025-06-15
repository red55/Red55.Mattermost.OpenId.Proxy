using System.ComponentModel.DataAnnotations;

using Destructurama.Attributed;

namespace Red55.Mattermost.OpenId.Proxy.Models
{
    public record GitlabPat
    {
        [Required]
        [LogMasked]
        public required string BootstrapToken { get; init; } = string.Empty;
        [Required]
        public required string StoreLocation { get; init; } = string.Empty;

        public TimeSpan GracePeriod { get; init; } = TimeSpan.FromDays (1);
    }

    public record GitLabSettings
    {
        public readonly static GitLabSettings Empty = new ()
        {
            PAT = new GitlabPat () { StoreLocation = "/home/app/", BootstrapToken = string.Empty },
            Url = new Uri (string.Empty)
        };
        [Required]
        public required Uri Url { get; init; }
        [Required]
        public required GitlabPat PAT { get; init; }
    }
    public record OpenIdSettings
    {
        public readonly static OpenIdSettings Empty = new ()
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
        [LogMasked]
        public required string AppSecret { get; init; }
    }

    public class AppConfig
    {
        public static readonly string SectionName = nameof (AppConfig);

        [Required]
        public required OpenIdSettings OpenId { get; init; }


        [Required]
        public required GitLabSettings GitLab { get; init; }
    }
}