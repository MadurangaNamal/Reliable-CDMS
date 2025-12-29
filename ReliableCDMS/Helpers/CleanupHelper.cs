using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace ReliableCDMS.Helpers
{
    public static class CleanupHelper
    {
        private static readonly string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

        /// <summary>
        /// Find and remove orphaned files (files on disk not referenced in database)
        /// </summary>
        public static CleanupResult RemoveOrphanedFiles()
        {
            var result = new CleanupResult();

            try
            {
                string uploadsFolder = HostingEnvironment.MapPath("~/Uploads/");

                if (!Directory.Exists(uploadsFolder))
                {
                    result.Message = "Uploads folder does not exist.";
                    return result;
                }

                // Get all files from disk
                var filesOnDisk = Directory.GetFiles(uploadsFolder)
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                // Get all file paths from database (current versions and all version history)
                var filesInDatabase = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // Get current document files
                    string query1 = "SELECT FilePath FROM Documents WHERE IsDeleted = 0";

                    using (SqlCommand cmd = new SqlCommand(query1, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string filePath = reader["FilePath"].ToString();
                                string fileName = Path.GetFileName(filePath.Replace("~/Uploads/", ""));
                                filesInDatabase.Add(fileName);
                            }
                        }
                    }

                    // Get all version history files
                    string query2 = "SELECT FilePath FROM DocumentVersions";

                    using (SqlCommand cmd = new SqlCommand(query2, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string filePath = reader["FilePath"].ToString();
                                string fileName = Path.GetFileName(filePath.Replace("~/Uploads/", ""));
                                filesInDatabase.Add(fileName);
                            }
                        }
                    }
                }

                // Find orphaned files
                var orphanedFiles = filesOnDisk.Except(filesInDatabase, StringComparer.OrdinalIgnoreCase).ToList();

                // Delete orphaned files
                foreach (var orphanedFile in orphanedFiles)
                {
                    try
                    {
                        string fullPath = Path.Combine(uploadsFolder, orphanedFile);
                        File.Delete(fullPath);
                        result.FilesDeleted++;
                        result.DeletedFiles.Add(orphanedFile);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Failed to delete {orphanedFile}: {ex.Message}");
                    }
                }

                result.Success = true;
                result.Message = $"Cleanup completed. Deleted {result.FilesDeleted} orphaned file(s).";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Cleanup failed: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Cleanup result model
        /// </summary>
        public class CleanupResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public int FilesDeleted { get; set; }
            public List<string> DeletedFiles { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
}