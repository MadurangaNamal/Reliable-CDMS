using ReliableCDMS.DAL;
using System;
using System.Web.UI;

namespace ReliableCDMS
{
    public partial class Default : Page
    {
        private readonly DocumentDAL documentDAL = new DocumentDAL();
        private readonly UserDAL userDAL = new UserDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                int userId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 0;
                string userRole = Session["UserRole"] != null ? Session["UserRole"].ToString() : "";

                // Get total documents
                lblTotalDocs.Text = documentDAL.GetTotalDocumentCount();

                // Get total users 
                if (userRole == "Admin")
                    lblTotalUsers.Text = userDAL.GetTotalUsersCount();

                // Get my uploads 
                lblMyUploads.Text = documentDAL.GetSelfTotalDocumentCount(userId);

                // Get recent documents
                gvRecentDocuments.DataSource = documentDAL.GetRecentDocuments();
                gvRecentDocuments.DataBind();

            }
            catch (Exception ex)
            {
                // Handle error
                Response.Write("<script>alert('Error loading dashboard: " + ex.Message + "');</script>");
            }
        }
    }
}