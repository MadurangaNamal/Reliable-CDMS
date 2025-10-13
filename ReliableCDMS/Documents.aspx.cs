using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;
using System.IO;
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

                // Get file info
                string fileName = Path.GetFileName(fileUpload.FileName);
                string category = ddlCategory.SelectedValue;
                string comments = txtComments.Text.Trim();
                long fileSize = fileUpload.PostedFile.ContentLength;

                // Validate file size (max 50MB)
                if (fileSize > 52428800)
                {
                    ShowError("File size exceeds 50MB limit.");
                    return;
                }

                // Check if document with same filename already exists
                var existingDocument = documentDAL.GetDocumentByFileName(fileName);

                // Create unique filename for storage
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                string uploadsFolder = Server.MapPath("~/Uploads/");

                // Create uploads folder if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                fileUpload.SaveAs(filePath);

                int userId = Convert.ToInt32(Session["UserId"]);
                string relativeFilePath = "~/Uploads/" + uniqueFileName;

                if (existingDocument != null)
                {
                    // UPDATE: Document exists, create new version
                    if (string.IsNullOrEmpty(comments))
                    {
                        comments = "Updated version";
                    }

                    bool success = documentDAL.UpdateDocument(
                        existingDocument.DocumentId,
                        relativeFilePath,
                        userId,
                        comments,
                        fileSize
                    );

                    if (success)
                    {
                        // Log action
                        AuditHelper.LogAction(userId, "Update Document Version",
                            $"Updated document: {fileName} to version {existingDocument.CurrentVersion + 1}",
                            Request.UserHostAddress);

                        ShowSuccess($"Document '{fileName}' updated to version {existingDocument.CurrentVersion + 1}!");
                    }
                    else
                    {
                        ShowError("Failed to update document version.");
                    }
                }
                else
                {
                    // CREATE: New document, version 1
                    if (string.IsNullOrEmpty(comments))
                    {
                        comments = "Initial upload";
                    }

                    int documentId = documentDAL.CreateDocument(fileName, category, userId, relativeFilePath, fileSize);

                    if (documentId > 0)
                    {
                        // Log action
                        AuditHelper.LogAction(userId, "Upload Document",
                            $"Uploaded new document: {fileName}, ID: {documentId}",
                            Request.UserHostAddress);

                        ShowSuccess($"Document '{fileName}' uploaded successfully as version 1!");
                    }
                    else
                    {
                        ShowError("Failed to upload document.");
                    }
                }

                LoadDocuments();

                // Clear form
                ddlCategory.SelectedIndex = 0;
                txtComments.Text = "";
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

                if (document != null)
                {
                    string filePath = Server.MapPath(document.FilePath); // Get physical file path

                    if (File.Exists(filePath))
                    {
                        // Log action
                        int userId = Convert.ToInt32(Session["UserId"]);
                        AuditHelper.LogAction(userId, "Download Document",
                            $"Downloaded document: {document.FileName}, ID: {documentId}",
                            Request.UserHostAddress);

                        // Download file
                        Response.Clear();
                        Response.ContentType = "application/octet-stream";
                        Response.AddHeader("Content-Disposition", $"attachment; filename={document.FileName}");
                        Response.TransmitFile(filePath);
                        Response.End();
                    }
                    else
                    {
                        ShowError("File not found on server.");
                    }
                }
                else
                {
                    ShowError("Document not found.");
                }
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