using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Web.UI.WebControls;

namespace ReliableCDMS
{
    public partial class Documents : System.Web.UI.Page
    {
        private readonly DocumentDAL documentDAL = new DocumentDAL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDocuments();
            }
        }

        /// <summary>
        /// Load all documents
        /// </summary>
        private void LoadDocuments()
        {
            try
            {
                gvDocuments.DataSource = documentDAL.GetAllDocuments();
                gvDocuments.DataBind();
            }
            catch (Exception ex)
            {
                ShowError("Error loading documents: " + ex.Message);
            }
        }

        /// <summary>
        /// Upload document
        /// </summary>
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (!fileUpload.HasFile)
                {
                    ShowError("Please select a file to upload.");
                    return;
                }

                // Sanitize filename to prevent path traversal
                string originalFileName = Path.GetFileName(fileUpload.FileName);
                string fileName = FileHelper.SanitizeFileName(originalFileName);

                string[] allowedTypes = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".jpg", ".png", ".pptx", ".ppt" };

                if (!FileHelper.IsAllowedFileType(fileName, allowedTypes))
                {
                    ShowError("File type not allowed. Allowed types: PDF, Word, Excel, Text, Images, Powerpoint");
                    return;
                }

                // Get file info
                string category = ddlCategory.SelectedValue;
                string comments = txtComments.Text.Trim();
                long fileSize = fileUpload.PostedFile.ContentLength;

                // Validate file size (max 50MB)
                if (fileSize > 52428800)
                {
                    ShowError("File size exceeds 50MB limit.");
                    return;
                }

                string uploadsFolder = Server.MapPath("~/Uploads/");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Get safe file path (prevents directory traversal)
                string filePath = FileHelper.GetSafeUploadPath(fileName, uploadsFolder);
                string uniqueFileName = Path.GetFileName(filePath);

                fileUpload.SaveAs(filePath); // Save file

                int userId = Convert.ToInt32(Session["UserId"]);
                string relativeFilePath = "~/Uploads/" + uniqueFileName;

                int documentId = 0;
                int retryCount = 0;
                const int maxRetries = 3;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        // Try to create as new document first
                        var existingDoc = documentDAL.GetDocumentByFileName(fileName);

                        if (existingDoc == null)
                        {
                            // No document exists, create new
                            documentId = documentDAL.CreateDocument(fileName, category, userId, relativeFilePath, fileSize);

                            AuditHelper.LogAction(userId, "Upload Document",
                                $"Uploaded new document: {fileName}, ID: {documentId}",
                                Request.UserHostAddress);

                            ShowSuccess($"Document '{fileName}' uploaded successfully as version 1!");
                            break;
                        }
                        else
                        {
                            // Document exists, update version
                            if (string.IsNullOrEmpty(comments))
                            {
                                comments = "Updated version";
                            }

                            documentDAL.UpdateDocument(existingDoc.DocumentId, relativeFilePath, userId, comments, fileSize);
                            documentId = existingDoc.DocumentId;

                            AuditHelper.LogAction(userId, "Update Document Version",
                                $"Updated document: {fileName} to version {existingDoc.CurrentVersion + 1}",
                                Request.UserHostAddress);

                            ShowSuccess($"Document '{fileName}' updated to version {existingDoc.CurrentVersion + 1}!");
                            break;
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                    {
                        // Unique constraint violation - another user created the document
                        retryCount++;

                        if (retryCount >= maxRetries)
                        {
                            ShowError("Unable to upload document due to concurrent access. Please try again.");

                            // Clean up orphaned file
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            return;
                        }

                        Thread.Sleep(100 * retryCount); // Wait and retry
                    }
                }

                LoadDocuments();

                // Clear the form
                ddlCategory.SelectedIndex = 0;
                txtComments.Text = "";
            }
            catch (SecurityException ex)
            {
                ShowError("Security validation failed: " + ex.Message);

                // Log security incident
                if (Session["UserId"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserId"]);

                    AuditHelper.LogAction(userId, "Security Alert",
                        $"Attempted path traversal: {fileUpload.FileName}",
                        Request.UserHostAddress);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error uploading document: " + ex.Message);
            }
        }

        /// <summary>
        /// Search documents
        /// </summary>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadDocuments();
                }
                else
                {
                    gvDocuments.DataSource = documentDAL.SearchDocuments(searchTerm);
                    gvDocuments.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error searching documents: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle GridView row commands
        /// </summary>
        protected void gvDocuments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int documentId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Download")
            {
                DownloadDocument(documentId);
            }
            else if (e.CommandName == "DeleteDoc")
            {
                DeleteDocument(documentId);
            }
        }

        /// <summary>
        /// Download document
        /// </summary>
        private void DownloadDocument(int documentId)
        {
            try
            {
                var document = documentDAL.GetDocumentById(documentId);

                if (document == null)
                {
                    ShowError("Document not found or has been deleted.");
                    return;
                }

                if (string.IsNullOrEmpty(document.FilePath))
                {
                    ShowError("Document file path is invalid.");
                    return;
                }

                string filePath = Server.MapPath(document.FilePath); // Get physical file path
                int userId = Convert.ToInt32(Session["UserId"]);

                if (!File.Exists(filePath))
                {
                    ShowError($"File not found on server. The file may have been moved or deleted.");

                    AuditHelper.LogAction(userId, "Download Failed",
                        $"File not found for document ID: {documentId}, Path: {document.FilePath}",
                        Request.UserHostAddress);
                    return;
                }

                // Log action
                AuditHelper.LogAction(userId, "Download Document",
                    $"Downloaded document: {document.FileName}, ID: {documentId}",
                    Request.UserHostAddress);

                // Download file
                Response.Clear();
                Response.ContentType = "application/octet-stream"; // Generic binary type
                Response.AddHeader("Content-Disposition", $"attachment; filename={document.FileName}");
                Response.AddHeader("Content-Length", new FileInfo(filePath).Length.ToString());
                Response.TransmitFile(filePath);
                Response.Flush();
                Response.End();
            }
            catch (Exception ex)
            {
                ShowError("Error downloading document: " + ex.Message);
            }
        }

        /// <summary>
        /// Delete document
        /// </summary>
        private void DeleteDocument(int documentId)
        {
            try
            {
                // Check permissions
                string userRole = Session["UserRole"].ToString();
                var document = documentDAL.GetDocumentById(documentId);
                int currentUserId = Convert.ToInt32(Session["UserId"]);

                // Only allow admin, manager or the uploader to delete
                if (userRole == "Employee" && document.UploadedBy != currentUserId)
                {
                    ShowError("You don't have permission to delete this document.");
                    return;
                }

                bool success = documentDAL.DeleteDocument(documentId);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(currentUserId, "Delete Document",
                        $"Deleted document ID: {documentId}",
                        Request.UserHostAddress);

                    ShowSuccess("Document deleted successfully!");
                    LoadDocuments();
                }
                else
                {
                    ShowError("Failed to delete document.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Error deleting document: " + ex.Message);
            }
        }

        /// <summary>
        /// Format file size for display
        /// </summary>
        public string FormatFileSize(object fileSize)
        {
            if (fileSize == null || fileSize == DBNull.Value)
                return "0 KB";

            long bytes = Convert.ToInt64(fileSize);

            if (bytes < 1024)
                return bytes + " B";
            else if (bytes < 1048576)
                return (bytes / 1024).ToString("F2") + " KB";
            else
                return (bytes / 1048576).ToString("F2") + " MB";
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