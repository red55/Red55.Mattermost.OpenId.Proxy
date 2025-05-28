using System.Text.Json.Serialization;

namespace Red55.Mattermost.OpenId.Proxy.Models.Gitlab
{
    public record UserIdentity
    {
        [JsonConstructor]
        public UserIdentity() { }
        public string Provider { get; set; } = string.Empty;
        public string ExternUid { get; set; } = string.Empty;
    }

    public record UserBase
    {
        internal const string DEFAULT_PICTURE_S256 = "750881342f8815121846d79d9e3d6aa5f8603adf578c3d9ac8e0b922bc75292d";
        internal static readonly string DEFAULT_PICTURE_URL = "https://www.gravatar.com/avatar/{0}?s=80";
        public int Id { get; set; }
        public required string Name { get; set; }

        public required string Username { get; set; }
        public string Email { get; set; } = string.Empty;

        public bool ForceRandomPassword { get; set; } = true;
        public string PublicEmail { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Format(DEFAULT_PICTURE_URL, DEFAULT_PICTURE_S256);
        public string WebUrl { get; set; } = string.Empty;

    }
    public record User : UserBase
    {
        [JsonConstructor]
        public User() { }
        public required string State { get; set; }
        public bool Locked { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Skype { get; set; } = string.Empty;
        public string Linkedin { get; set; } = string.Empty;
        public string Twitter { get; set; } = string.Empty;
        public string Discord { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Pronouns { get; set; } = string.Empty;
        public bool Bot { get; set; } = false;
        public string WorkInformation { get; set; } = string.Empty;
        public DateTime? LocalTime { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateOnly? LastActivityOn { get; set; }
        public int ThemeId { get; set; }
        public int ColorSchemeId { get; set; }
        public int ProjectsLimit { get; set; }
        public DateTime? CurrentSignInAt { get; set; }
        public List<UserIdentity> Identities { get; set; } = [];
        public bool CanCreateGroup { get; set; }
        public bool CanCreateProject { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool External { get; set; }
        public bool PrivateProfile { get; set; }
        public string CommitEmail { get; set; } = string.Empty;

        public UserBase? CreatedBy { get; init; }
    }

}
