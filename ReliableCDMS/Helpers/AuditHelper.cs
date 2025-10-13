using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ReliableCDMS.Helpers
{
    public class AuditHelper
    {
        /// <summary>
        /// Log user action to AuditLog table
        /// </summary>
        public static void LogAction(int userId, string action, string details, string ipAddress = "")
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"INSERT INTO AuditLog (UserId, Action, Details, IPAddress, LogDate) 
                                   VALUES (@UserId, @Action, @Details, @IPAddress, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@Details", details ?? "");
                        cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? "");

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error to file
                Debug.WriteLine("Audit logging failed: " + ex.Message);
            }
        }
    }
}