using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace ReliableCDMS
{
    public partial class Default : Page
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

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

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Get total documents count
                    string totalDocsQuery = "SELECT COUNT(*) FROM Documents WHERE IsDeleted = 0";
                    using (SqlCommand cmd = new SqlCommand(totalDocsQuery, conn))
                    {
                        lblTotalDocs.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Get total users count (Admin only)
                    if (userRole == "Admin")
                    {
                        string totalUsersQuery = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
                        using (SqlCommand cmd = new SqlCommand(totalUsersQuery, conn))
                        {
                            lblTotalUsers.Text = cmd.ExecuteScalar().ToString();
                        }
                    }

                    // Get my uploads count
                    string myUploadsQuery = "SELECT COUNT(*) FROM Documents WHERE UploadedBy = @UserId AND IsDeleted = 0";
                    using (SqlCommand cmd = new SqlCommand(myUploadsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        lblMyUploads.Text = cmd.ExecuteScalar().ToString();
                    }

                    // Get recent documents (top 10)
                    string recentDocsQuery = @"SELECT TOP 10 d.DocumentId, d.FileName, d.Category, d.UploadDate, 
                                                     d.CurrentVersion, u.Username as UploadedByName
                                              FROM Documents d
                                              INNER JOIN Users u ON d.UploadedBy = u.UserId
                                              WHERE d.IsDeleted = 0
                                              ORDER BY d.UploadDate DESC";

                    using (SqlCommand cmd = new SqlCommand(recentDocsQuery, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            gvRecentDocuments.DataSource = dt;
                            gvRecentDocuments.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Response.Write("<script>alert('Error loading dashboard: " + ex.Message + "');</script>");
            }
        }
    }
}