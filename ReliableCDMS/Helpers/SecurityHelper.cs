using ReliableCDMS.DAL;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ReliableCDMS.Helpers
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Basic authentication check using username/password in header or session
        /// </summary>
        public static bool IsAuthenticated()
        {
            try
            {
                var request = HttpContext.Current.Request;

                // Check for Basic Authentication header
                var authHeader = request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                        string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                        string[] parts = credentials.Split(':');

                        if (parts.Length == 2)
                        {
                            string username = parts[0];
                            string password = parts[1];
                            string passwordHash = HashPassword(password);

                            UserDAL userDAL = new UserDAL();
                            var user = userDAL.AuthenticateUser(username, passwordHash);

                            if (user != null && VerifyPassword(password, user.PasswordHash))
                            {
                                // Store user info in Items for this request only (not session)
                                HttpContext.Current.Items["AuthUserId"] = user.UserId;
                                HttpContext.Current.Items["AuthUsername"] = user.Username;
                                HttpContext.Current.Items["AuthUserRole"] = user.Role;

                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Authentication error: " + ex.Message);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsAuthenticated error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get authenticated user ID
        /// </summary>
        public static int GetAuthenticatedUserId()
        {
            try
            {
                var authUserId = HttpContext.Current.Items["AuthUserId"];

                // Get from Items (set during IsAuthenticated)
                if (authUserId != null)
                {
                    return Convert.ToInt32(authUserId);
                }

                // Fallback: re-authenticate if needed
                if (IsAuthenticated() && authUserId != null)
                {
                    return Convert.ToInt32(authUserId);
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }

        /// <summary>
        /// Check for admin permissions
        /// </summary>
        public static bool IsAdmin()
        {
            return (HttpContext.Current.Items["AuthUserRole"] as string) == "Admin";
        }
    }
}