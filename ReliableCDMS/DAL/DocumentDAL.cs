using ReliableCDMS.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ReliableCDMS.DAL
{
    /// <summary>
    /// Data Access Layer for Documents and Document Versions
    /// </summary>
    public class DocumentDAL
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["ReliableCDMSDB"].ConnectionString;

        #region Document Operations - synchronous

        /// <summary>
        /// Get all documents
        /// </summary>
        public DataTable GetAllDocuments()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT d.DocumentId, d.FileName, d.Category, d.UploadDate, 
                                       d.CurrentVersion, d.FileSize, u.Username as UploadedByName
                               FROM Documents d
                               INNER JOIN Users u 
                               ON d.UploadedBy = u.UserId
                               WHERE d.IsDeleted = 0
                               ORDER BY d.UploadDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Search documents
        /// </summary>
        public DataTable SearchDocuments(string searchTerm)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT d.DocumentId, d.FileName, d.Category, d.UploadDate, 
                               d.CurrentVersion, d.FileSize, u.Username as UploadedByName
                               FROM Documents d
                               INNER JOIN Users u 
                               ON d.UploadedBy = u.UserId
                               WHERE d.IsDeleted = 0 
                               AND (d.FileName LIKE @SearchTerm OR d.Category LIKE @SearchTerm)
                               ORDER BY d.UploadDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Get document by ID
        /// </summary>
        public Document GetDocumentById(int documentId)
        {
            Document doc = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT * FROM Documents WHERE DocumentId = @DocumentId AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            doc = new Document
                            {
                                DocumentId = (int)reader["DocumentId"],
                                FileName = reader["FileName"]?.ToString() ?? "Unknown",
                                Category = reader["Category"]?.ToString() ?? "General",
                                UploadedBy = (int)reader["UploadedBy"],
                                UploadDate = (DateTime)reader["UploadDate"],
                                CurrentVersion = (int)reader["CurrentVersion"],
                                FilePath = reader["FilePath"]?.ToString() ?? "",
                                FileSize = reader["FileSize"] != DBNull.Value ? (long)reader["FileSize"] : 0
                            };
                        }
                    }
                }
            }

            return doc;
        }

        /// <summary>
        /// Get document by filename
        /// </summary>
        public Document GetDocumentByFileName(string fileName)
        {
            Document doc = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT * FROM Documents WHERE FileName = @FileName AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FileName", fileName);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            doc = new Document
                            {
                                DocumentId = (int)reader["DocumentId"],
                                FileName = reader["FileName"].ToString(),
                                Category = reader["Category"].ToString(),
                                UploadedBy = (int)reader["UploadedBy"],
                                UploadDate = (DateTime)reader["UploadDate"],
                                CurrentVersion = (int)reader["CurrentVersion"],
                                FilePath = reader["FilePath"].ToString(),
                                FileSize = reader["FileSize"] != DBNull.Value ? (long)reader["FileSize"] : 0
                            };
                        }
                    }
                }
            }

            return doc;
        }

        /// <summary>
        /// Create new document
        /// </summary>
        public int CreateDocument(string fileName, string category, int uploadedBy, string filePath, long fileSize)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO Documents (FileName, Category, UploadedBy, UploadDate, CurrentVersion, FilePath, FileSize, IsDeleted) 
                               VALUES (@FileName, @Category, @UploadedBy, GETDATE(), 1, @FilePath, @FileSize, 0);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@Category", category ?? "General");
                    cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                    cmd.Parameters.AddWithValue("@FilePath", filePath);
                    cmd.Parameters.AddWithValue("@FileSize", fileSize);

                    conn.Open();
                    int documentId = (int)cmd.ExecuteScalar();

                    // Also create first version entry
                    CreateDocumentVersion(documentId, 1, filePath, uploadedBy, "Initial upload");

                    return documentId;
                }
            }
        }

        /// <summary>
        /// Update document (new version)
        /// </summary>
        public bool UpdateDocument(int documentId, string filePath, int uploadedBy, string comments, long fileSize)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // Get current version with lock
                        string getVersionQuery = @"SELECT CurrentVersion 
                                          FROM Documents WITH (UPDLOCK, ROWLOCK) 
                                          WHERE DocumentId = @DocumentId";
                        int currentVersion = 1;

                        using (SqlCommand cmd = new SqlCommand(getVersionQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocumentId", documentId);
                            object result = cmd.ExecuteScalar();

                            if (result == null)
                            {
                                transaction.Rollback();
                                return false; // Document not found
                            }

                            currentVersion = (int)result;
                        }

                        int newVersion = currentVersion + 1;

                        // Update document
                        string updateQuery = @"UPDATE Documents 
                                     SET CurrentVersion = @NewVersion, 
                                         FilePath = @FilePath,
                                         FileSize = @FileSize,
                                         UploadDate = GETDATE()
                                     WHERE DocumentId = @DocumentId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocumentId", documentId);
                            cmd.Parameters.AddWithValue("@NewVersion", newVersion);
                            cmd.Parameters.AddWithValue("@FilePath", filePath);
                            cmd.Parameters.AddWithValue("@FileSize", fileSize);

                            cmd.ExecuteNonQuery();
                        }

                        // Create version entry
                        CreateDocumentVersionInTransaction(conn, transaction, documentId, newVersion, filePath, uploadedBy, comments);

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Delete document (soft delete)
        /// </summary>
        public bool DeleteDocument(int documentId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"UPDATE Documents SET IsDeleted = 1 WHERE DocumentId = @DocumentId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Get total documents count
        /// </summary>
        public string GetTotalDocumentCount()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string totalDocsQuery = "SELECT COUNT(*) FROM Documents WHERE IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(totalDocsQuery, conn))
                {
                    conn.Open();
                    return cmd.ExecuteScalar().ToString();
                }
            }
        }

        /// <summary>
        /// Get my document uploads count
        /// </summary>
        public string GetSelfTotalDocumentCount(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string myUploadsQuery = "SELECT COUNT(*) FROM Documents WHERE UploadedBy = @UserId AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(myUploadsQuery, conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return cmd.ExecuteScalar().ToString();
                }
            }
        }

        /// <summary>
        /// Get recent documents (top 10)
        /// </summary>
        public DataTable GetRecentDocuments()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

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
                        return dt;
                    }
                }
            }
        }

        #endregion

        #region Document Operations - Asynchronous

        /// <summary>
        /// Get document by filename - Async
        /// </summary>
        public async Task<Document> GetDocumentByFileNameAsync(string fileName)
        {
            Document doc = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT * FROM Documents 
                                WHERE FileName = @FileName AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FileName", fileName);

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            doc = new Document
                            {
                                DocumentId = (int)reader["DocumentId"],
                                FileName = reader["FileName"].ToString(),
                                Category = reader["Category"].ToString(),
                                UploadedBy = (int)reader["UploadedBy"],
                                UploadDate = (DateTime)reader["UploadDate"],
                                CurrentVersion = (int)reader["CurrentVersion"],
                                FilePath = reader["FilePath"].ToString(),
                                FileSize = reader["FileSize"] != DBNull.Value ? (long)reader["FileSize"] : 0
                            };
                        }
                    }
                }
            }

            return doc;
        }

        /// <summary>
        /// Create new document - Async
        /// </summary>
        public async Task<int> CreateDocumentAsync(string fileName, string category, int uploadedBy, string filePath, long fileSize)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();

                using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        string query = @"INSERT INTO Documents (FileName, Category, UploadedBy, UploadDate, CurrentVersion, FilePath, FileSize, IsDeleted) 
                               VALUES (@FileName, @Category, @UploadedBy, GETDATE(), 1, @FilePath, @FileSize, 0);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@FileName", fileName);
                            cmd.Parameters.AddWithValue("@Category", category ?? "General");
                            cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                            cmd.Parameters.AddWithValue("@FilePath", filePath);
                            cmd.Parameters.AddWithValue("@FileSize", fileSize);

                            int documentId = (int)await cmd.ExecuteScalarAsync();

                            await CreateDocumentVersionInTransactionAsync(conn, transaction, documentId, 1, filePath, uploadedBy, "Initial upload"); // Create the first version entry

                            transaction.Commit();

                            return documentId;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Update document - Async
        /// </summary>
        public async Task<bool> UpdateDocumentAsync(int documentId, string filePath, string category, int uploadedBy, string comments, long fileSize)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();

                using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // Get current version with lock
                        string getVersionQuery = @"SELECT CurrentVersion 
                                                  FROM Documents WITH (UPDLOCK, ROWLOCK) 
                                                  WHERE DocumentId = @DocumentId";
                        int currentVersion = 1;

                        using (SqlCommand cmd = new SqlCommand(getVersionQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocumentId", documentId);
                            object result = await cmd.ExecuteScalarAsync();

                            if (result == null)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            currentVersion = (int)result;
                        }

                        int newVersion = currentVersion + 1;

                        string updateQuery = @"UPDATE Documents 
                                             SET CurrentVersion = @NewVersion, 
                                                 FilePath = @FilePath,
                                                 FileSize = @FileSize,
                                                 UploadDate = GETDATE(),
                                                 Category = @Category
                                             WHERE DocumentId = @DocumentId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@DocumentId", documentId);
                            cmd.Parameters.AddWithValue("@NewVersion", newVersion);
                            cmd.Parameters.AddWithValue("@FilePath", filePath);
                            cmd.Parameters.AddWithValue("@FileSize", fileSize);
                            cmd.Parameters.AddWithValue("@Category", category);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Create version entry
                        await CreateDocumentVersionInTransactionAsync(conn, transaction, documentId, newVersion, filePath, uploadedBy, comments);

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region Document Version Operations

        /// <summary>
        /// Create document version within a transaction
        /// </summary>
        private void CreateDocumentVersionInTransaction(SqlConnection conn, SqlTransaction transaction,
            int documentId, int versionNumber, string filePath, int uploadedBy, string comments)
        {
            string query = @"INSERT INTO DocumentVersions (DocumentId, VersionNumber, FilePath, UploadedBy, UploadDate, Comments) 
                   VALUES (@DocumentId, @VersionNumber, @FilePath, @UploadedBy, GETDATE(), @Comments)";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@VersionNumber", versionNumber);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                cmd.Parameters.AddWithValue("@Comments", comments ?? "");

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Create document version entry
        /// </summary>
        private void CreateDocumentVersion(int documentId, int versionNumber, string filePath, int uploadedBy, string comments)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO DocumentVersions (DocumentId, VersionNumber, FilePath, UploadedBy, UploadDate, Comments) 
                               VALUES (@DocumentId, @VersionNumber, @FilePath, @UploadedBy, GETDATE(), @Comments)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@VersionNumber", versionNumber);
                    cmd.Parameters.AddWithValue("@FilePath", filePath);
                    cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                    cmd.Parameters.AddWithValue("@Comments", comments ?? "");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Get document versions
        /// </summary>
        public DataTable GetDocumentVersions(int documentId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT dv.VersionId, dv.VersionNumber, dv.UploadDate, 
                                       dv.Comments, u.Username as UploadedByName
                               FROM DocumentVersions dv
                               INNER JOIN Users u ON dv.UploadedBy = u.UserId
                               WHERE dv.DocumentId = @DocumentId
                               ORDER BY dv.VersionNumber DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        #endregion

        #region Document Version Operations - Asynchronous

        /// <summary>
        /// Create document version - Async
        /// </summary>
        private async Task CreateDocumentVersionAsync(int documentId, int versionNumber, string filePath, int uploadedBy, string comments)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"INSERT INTO DocumentVersions (DocumentId, VersionNumber, FilePath, UploadedBy, UploadDate, Comments) 
                               VALUES (@DocumentId, @VersionNumber, @FilePath, @UploadedBy, GETDATE(), @Comments)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@VersionNumber", versionNumber);
                    cmd.Parameters.AddWithValue("@FilePath", filePath);
                    cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                    cmd.Parameters.AddWithValue("@Comments", comments ?? "");

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Create document version within a transaction - Async
        /// </summary>
        private async Task CreateDocumentVersionInTransactionAsync(SqlConnection conn, SqlTransaction transaction,
            int documentId, int versionNumber, string filePath, int uploadedBy, string comments)
        {
            string query = @"INSERT INTO DocumentVersions (DocumentId, VersionNumber, FilePath, UploadedBy, UploadDate, Comments) 
                           VALUES (@DocumentId, @VersionNumber, @FilePath, @UploadedBy, GETDATE(), @Comments)";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@VersionNumber", versionNumber);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                cmd.Parameters.AddWithValue("@Comments", comments ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
        }

        #endregion

    }
}