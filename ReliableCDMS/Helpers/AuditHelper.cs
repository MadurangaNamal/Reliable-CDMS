using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ReliableCDMS.Helpers
{
    public static class AuditHelper
    {
        private static Dictionary<string, DateTime> _recentEntries = new Dictionary<string, DateTime>();
        private static readonly object _lock = new object();
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromSeconds(2);
        private static readonly string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

        /// <summary>
        /// Log user action to AuditLog table
        /// </summary>
        public static void LogAction(int userId, string action, string details, string ipAddress = "")
        {
            try
            {
                // Prevent duplicate logs within 2 seconds
                string entryKey = $"{userId}_{action}_{details}";

                lock (_lock)
                {
                    // Check if this exact entry was logged recently
                    if (_recentEntries.ContainsKey(entryKey))
                    {
                        DateTime lastLogged = _recentEntries[entryKey];

                        if (DateTime.Now - lastLogged < DuplicateWindow)
                        {
                            return; // skip this log entry
                        }
                    }

                    // Update recent entries
                    _recentEntries[entryKey] = DateTime.Now;

                    // Clean up old entries
                    if (_recentEntries.Count > 100)
                    {
                        var expiredKeys = new List<string>();
                        foreach (var kvp in _recentEntries)
                        {
                            if (DateTime.Now - kvp.Value > DuplicateWindow)
                            {
                                expiredKeys.Add(kvp.Key);
                            }
                        }
                        foreach (var key in expiredKeys)
                        {
                            _recentEntries.Remove(key);
                        }
                    }
                }

                // Log to database
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"INSERT INTO AuditLog (UserId, Action, Details, IPAddress, LogDate) 
                                   VALUES (@UserId, @Action, @Details, @IPAddress, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Action", action ?? "");
                        cmd.Parameters.AddWithValue("@Details", details ?? "");
                        cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? "");

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Debug output
                        Debug.WriteLine($"Audit Log: {rowsAffected} row(s) inserted - User: {userId}, Action: {action}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error to debug output ( Not throwing exception to avoid breaking the main functionality )
                Debug.WriteLine("Audit logging failed: " + ex.Message);
                Debug.WriteLine("Stack trace: " + ex.StackTrace);
            }
        }
    }
}