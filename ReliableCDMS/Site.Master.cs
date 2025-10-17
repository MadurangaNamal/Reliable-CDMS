using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;
using System.Diagnostics;
using System.Web.Security;
using System.Web.UI;


namespace ReliableCDMS
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // User has valid auth cookie
                if (Session["UserId"] == null)
                {
                    // Session expired but auth cookie still valid
                    RestoreSessionFromAuthCookie();
                }

                if (Session["UserId"] == null) // Double check session was restored successfully
                {
                    // Restoration failed - force re-login
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx?reason=sessionexpired");
                }
            }
            else
            {
                // Not authenticated, redirect to login
                Response.Redirect("~/Login.aspx");
            }
        }

        /// <summary>
        /// Restore session data from authentication cookie
        /// </summary>
        private void RestoreSessionFromAuthCookie()
        {
            try
            {
                string username = Context.User.Identity.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return;
                }

                UserDAL userDAL = new UserDAL();
                var user = userDAL.GetUserByUsername(username);

                if (user != null && user.IsActive)
                {
                    // Restore all session variables
                    Session["UserId"] = user.UserId;
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.Role;
                    Session["Department"] = user.Department;

                    AuditHelper.LogAction(user.UserId, "Session Restored",
                        "Session expired and was restored from auth cookie",
                        Request.UserHostAddress);
                }
                else
                {
                    // User doesn't exist or is inactive
                    FormsAuthentication.SignOut();
                }
            }
            catch (Exception ex)
            {
                // Log error without throw, will redirect to login
                Debug.WriteLine("Session restoration failed: " + ex.Message);
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            // Log the logout action
            if (Session["UserId"] != null)
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                AuditHelper.LogAction(userId, "Logout", "User logged out", Request.UserHostAddress);
            }

            // Clear session
            Session.Clear();
            Session.Abandon();

            // Sign out 
            FormsAuthentication.SignOut();

            // Redirect to login page
            Response.Redirect("~/Login.aspx");
        }
    }
}