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

    public record User
    {
        [JsonConstructor]
        public User() { }
        public int Id { get; set; }
        public required string Username { get; set; }
        public string PublicEmail { get; set; } = string.Empty;
        public required string Name { get; set; }
        public required string State { get; set; }
        public bool Locked { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
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
        public bool Bot { get; set; }
        public string WorkInformation { get; set; } = string.Empty;
        public DateTime? LocalTime { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public DateOnly? LastActivityOn { get; set; }
        public string Email { get; set; } = string.Empty;
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
    }

}
