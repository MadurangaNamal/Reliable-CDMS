using ReliableCDMS.Helpers;
using System;
using System.Web.Security;
using System.Web.UI;

namespace ReliableCDMS
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx");
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