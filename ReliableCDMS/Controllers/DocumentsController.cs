using ReliableCDMS.DAL;
using ReliableCDMS.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace ReliableCDMS.Controllers
{
    /// <summary>
    /// REST API for Document operations
    /// </summary>
    [RoutePrefix("api/documents")]
    public class DocumentsApiController : ApiController
    {
        private DocumentDAL documentDAL = new DocumentDAL();

        /// <summary>
        /// GET: api/documents - Get all documents
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllDocuments()
        {
            try
            {
                // Basic authentication check
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                var documents = documentDAL.GetAllDocuments();

                // Convert DataTable to list
                var docList = new List<object>();
                foreach (System.Data.DataRow row in documents.Rows)
                {
                    docList.Add(new
                    {
                        DocumentId = row["DocumentId"],
                        FileName = row["FileName"],
                        Category = row["Category"],
                        UploadedBy = row["UploadedByName"],
                        UploadDate = row["UploadDate"],
                        CurrentVersion = row["CurrentVersion"],
                        FileSize = row["FileSize"]
                    });
                }

                return Ok(new { success = true, data = docList });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// GET: api/documents/{id} - Get document by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetDocument(int id)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                var document = documentDAL.GetDocumentById(id);

                if (document == null)
                {
                    return NotFound();
                }

                return Ok(new { success = true, data = document });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// POST: api/documents - Upload new document
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult UploadDocument()
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var file = httpRequest.Files[0];
                string fileName = Path.GetFileName(file.FileName);
                string category = httpRequest.Form["category"] ?? "General";

                // Get user ID from session or auth header
                int userId = GetAuthenticatedUserId();

                // Save file
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                string uploadsFolder = HttpContext.Current.Server.MapPath("~/Uploads/");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                file.SaveAs(filePath);

                // Save to database
                string relativeFilePath = "~/Uploads/" + uniqueFileName;
                int documentId = documentDAL.CreateDocument(fileName, category, userId, relativeFilePath, file.ContentLength);

                // Log action
                AuditHelper.LogAction(userId, "API Upload Document",
                    $"Uploaded via API: {fileName}, ID: {documentId}",
                    HttpContext.Current.Request.UserHostAddress);

                return Ok(new
                {
                    success = true,
                    message = "Document uploaded successfully",
                    documentId = documentId
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// PUT: api/documents/{id} - Update document (new version)
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateDocument(int id)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var file = httpRequest.Files[0];
                string comments = httpRequest.Form["comments"] ?? "Updated version";
                int userId = GetAuthenticatedUserId();

                // Save new version
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string uploadsFolder = HttpContext.Current.Server.MapPath("~/Uploads/");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                long fileSize = file.ContentLength;
                file.SaveAs(filePath);

                // Update document
                string relativeFilePath = "~/Uploads/" + uniqueFileName;
                bool success = documentDAL.UpdateDocument(id, relativeFilePath, userId, comments, fileSize);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(userId, "API Update Document",
                        $"Updated via API: Document ID: {id}",
                        HttpContext.Current.Request.UserHostAddress);

                    return Ok(new { success = true, message = "Document updated successfully" });
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// DELETE: api/documents/{id} - Delete document
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteDocument(int id)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                int userId = GetAuthenticatedUserId();
                bool success = documentDAL.DeleteDocument(id);

                if (success)
                {
                    // Log action
                    AuditHelper.LogAction(userId, "API Delete Document",
                        $"Deleted via API: Document ID: {id}",
                        HttpContext.Current.Request.UserHostAddress);

                    return Ok(new { success = true, message = "Document deleted successfully" });
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// GET: api/documents/search?term=xyz - Search documents
        /// </summary>
        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchDocuments([FromUri] string term)
        {
            try
            {
                if (!IsAuthenticated())
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(term))
                {
                    return BadRequest("Search term is required");
                }

                var documents = documentDAL.SearchDocuments(term);

                // Convert DataTable to list
                var docList = new List<object>();
                foreach (System.Data.DataRow row in documents.Rows)
                {
                    docList.Add(new
                    {
                        DocumentId = row["DocumentId"],
                        FileName = row["FileName"],
                        Category = row["Category"],
                        UploadedBy = row["UploadedByName"],
                        UploadDate = row["UploadDate"],
                        CurrentVersion = row["CurrentVersion"]
                    });
                }

                return Ok(new { success = true, data = docList });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Basic authentication check using username/password in header or session
        /// </summary>
        private bool IsAuthenticated()
        {
            // Check session first
            if (HttpContext.Current.Session["UserId"] != null)
            {
                return true;
            }

            // Check for Basic Authentication header
            var authHeader = HttpContext.Current.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
            {
                try
                {
                    string encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                    string credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                    string[] parts = credentials.Split(':');

                    if (parts.Length == 2)
                    {
                        string username = parts[0];
                        string password = parts[1];
                        string passwordHash = SecurityHelper.HashPassword(password);

                        UserDAL userDAL = new UserDAL();
                        var user = userDAL.AuthenticateUser(username, passwordHash);

                        if (user != null)
                        {
                            // Store in session for this request
                            HttpContext.Current.Session["UserId"] = user.UserId;
                            HttpContext.Current.Session["Username"] = user.Username;
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Get authenticated user ID
        /// </summary>
        private int GetAuthenticatedUserId()
        {
            if (HttpContext.Current.Session["UserId"] != null)
            {
                return Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            }
            return 1; // Default to admin for testing
        }

        #endregion
    }
}