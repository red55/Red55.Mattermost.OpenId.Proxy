using Destructurama.Attributed;

namespace Red55.Mattermost.OpenId.Proxy.Models.Gitlab
{
    public record PersonalAccessToken
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Revoked { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = [];
        public int UserId { get; set; }
        public DateTimeOffset LastUsedAt { get; set; }
        public DateOnly ExpiresAt { get; set; }
        public bool Active { get; set; }
        [LogMasked]
        public string Token { get; set; } = string.Empty;
    }
}
