using System.Security.Cryptography;
using System.Text;

namespace Red55.Mattermost.OpenId.Proxy.Extensions.GitLab
{
    public static class User
    {
        public static string EmailHash(this Models.Gitlab.UserBase user)
        {
            static string ComputeSha256Hash(string rawData)
            {
                // Create a SHA256
                // ComputeHash - returns byte array
                byte[] bytes = SHA256.HashData (Encoding.UTF8.GetBytes (rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder ();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append (bytes[i].ToString ("x2"));
                }
                return builder.ToString ();
            }

            if (string.IsNullOrEmpty (user.Email))
            {
                return Models.Gitlab.UserBase.DEFAULT_PICTURE_S256;
            }

        
            return ComputeSha256Hash(user.Email.ToLowerInvariant ());
        }
    }
}
