using System.ComponentModel.DataAnnotations;

using Destructurama.Attributed;

using Generator.Equals;

namespace Red55.Mattermost.OpenId.Proxy.Models
{
    [Equatable]
    public partial record GitlabPat
    {
        [Required]
        [LogMasked]
        public required string BootstrapToken { get; init; } = string.Empty;
        [Required]
        public required string StoreLocation { get; init; } = string.Empty;

        public TimeSpan GracePeriod { get; init; } = TimeSpan.FromDays (1);
    }
    [Equatable]
    public partial record GitLabSettings
    {
        public readonly static GitLabSettings Empty = new ()
        {
            PAT = new GitlabPat () { StoreLocation = "/home/app/", BootstrapToken = string.Empty },
            Url = AppConfig.EmptyUri
        };
        [Required]
        public required Uri Url { get; init; }
        [Required]
        public required GitlabPat PAT { get; init; }
    }
    [Equatable]
    public partial record OpenIdHealthChecks
    {
        public readonly static OpenIdHealthChecks Empty;

        public Uri LivenessUrl { get; init; } = AppConfig.EmptyUri;

        [Required]
        public required Uri ReadinessUrl { get; init; }

        public required bool DangerousAcceptAnyServerCertificate { get; init; } = false;
    }
    [Equatable]
    public partial record OpenIdSettings
    {
        public readonly static OpenIdSettings Empty = new ()
        {
            AppId = string.Empty,
            AppSecret = string.Empty,
            Url = AppConfig.EmptyUri,
            HealthChecks = OpenIdHealthChecks.Empty
        };

        [Required]
        public required Uri Url { get; init; }
        [Required]
        public required string AppId { get; init; }
        [Required]
        [LogMasked]
        public required string AppSecret { get; init; }

        public OpenIdHealthChecks HealthChecks { get; init; } = OpenIdHealthChecks.Empty;
    }
    [Equatable]
    public sealed partial class AppConfig
    {
        public static readonly string SectionName = nameof (AppConfig);
        public static readonly Uri EmptyUri = new ("about:blank");

        [Required]
        public required OpenIdSettings OpenId { get; init; }


        [Required]
        public required GitLabSettings GitLab { get; init; }
    }
}