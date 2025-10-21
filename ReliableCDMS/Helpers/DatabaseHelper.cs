using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ReliableCDMS.Helpers
{
    public static class DatabaseHelper
    {
        /// <summary>
        /// Test database connectivity
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Verify database accessibility
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                errorMessage = $"Database connection failed: {ex.Message}";
                Debug.WriteLine(errorMessage);

                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Unexpected error: {ex.Message}";
                Debug.WriteLine(errorMessage);

                return false;
            }
        }
    }
}