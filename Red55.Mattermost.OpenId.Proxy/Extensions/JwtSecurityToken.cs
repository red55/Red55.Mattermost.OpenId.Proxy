namespace Red55.Mattermost.OpenId.Proxy.Extensions
{
    public static class JwtSecurityToken
    {
        public static string GetClaimValue(this System.IdentityModel.Tokens.Jwt.JwtSecurityToken token, string claimType)
        {
            EnsureArg.IsNotNull (token, nameof (token));

            var claim = token.Claims.FirstOrDefault (c => c.Type.Equals (claimType, StringComparison.OrdinalIgnoreCase));
            
            return claim?.Value ?? throw new InvalidOperationException ($"Claim '{claimType}' not found in the token.");
        }
    }
}
