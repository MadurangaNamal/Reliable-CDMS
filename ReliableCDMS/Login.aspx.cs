using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using ReliableCDMS.Models;
using System;
using System.Web.Security;

namespace ReliableCDMS
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // If already logged in, redirect to dashboard
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                // Hash the password
                string passwordHash = SecurityHelper.HashPassword(password);

                // Authenticate user
                UserDAL userDAL = new UserDAL();
                User user = userDAL.AuthenticateUser(username, passwordHash);

                if (user != null && SecurityHelper.VerifyPassword(password, user.PasswordHash))
                {
                    // Store user info in session
                    Session["UserId"] = user.UserId;
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.Role;
                    Session["Department"] = user.Department;

                    // Create authentication ticket
                    FormsAuthentication.SetAuthCookie(username, false);

                    // Log the login action
                    string ipAddress = Request.UserHostAddress;
                    AuditHelper.LogAction(user.UserId, "Login", "User logged in successfully", ipAddress);

                    // Redirect to dashboard
                    Response.Redirect("~/Default.aspx");
                }
                else
                {
                    // Show error
                    pnlError.Visible = true;
                    lblError.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred: " + ex.Message;
            }
        }
    }
}