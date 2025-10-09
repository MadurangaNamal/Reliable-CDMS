using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;
using System.Web.UI.WebControls;

namespace ReliableCDMS
{
    public partial class Users : System.Web.UI.Page
    {
        private UserDAL userDAL = new UserDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is admin
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();
            }
        }

        /// <summary>
        /// Load all users
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                gvUsers.DataSource = userDAL.GetAllUsers();
                gvUsers.DataBind();
            }
            catch (Exception ex)
            {
                ShowError("Error loading users: " + ex.Message);
            }
        }

        /// <summary>
        /// Add new user
        /// </summary>
        protected void btnAddUser_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;
                string role = ddlRole.SelectedValue;
                string department = txtDepartment.Text.Trim();

                // Hash password
                string passwordHash = SecurityHelper.HashPassword(password);

                // Create user
                int userId = userDAL.CreateUser(username, passwordHash, role, department);

                if (userId > 0)
                {
                    // Log action
                    int currentUserId = Convert.ToInt32(Session["UserId"]);
                    AuditHelper.LogAction(currentUserId, "Create User",
                        $"Created new user: {username}, Role: {role}",
                        Request.UserHostAddress);

                    ShowSuccess($"User '{username}' created successfully!");
                    LoadUsers();

                    // Clear form
                    txtUsername.Text = "";
                    txtPassword.Text = "";
                    txtDepartment.Text = "";
                    ddlRole.SelectedIndex = 0;
                }
                else
                {
                    ShowError("Failed to create user. Username may already exist.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error creating user: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle GridView row commands
        /// </summary>
        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                DeleteUser(userId);
            }
        }

        /// <summary>
        /// Delete (deactivate) user
        /// </summary>
        private void DeleteUser(int userId)
        {
            try
            {
                // Prevent deleting self
                int currentUserId = Convert.ToInt32(Session["UserId"]);
                if (userId == currentUserId)
                {
                    ShowError("You cannot deactivate your own account.");
                    return;
                }

                bool success = userDAL.DeleteUser(userId);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(currentUserId, "Deactivate User",
                        $"Deactivated user ID: {userId}",
                        Request.UserHostAddress);

                    ShowSuccess("User deactivated successfully!");
                    LoadUsers();
                }
                else
                {
                    ShowError("Failed to deactivate user.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error deactivating user: " + ex.Message);
            }
        }

        /// <summary>
        /// Show success message
        /// </summary>
        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
            lblSuccess.Text = message;
        }

        /// <summary>
        /// Show error message
        /// </summary>
        private void ShowError(string message)
        {
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
            lblError.Text = message;
        }
    }
}