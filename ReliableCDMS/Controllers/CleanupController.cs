using ReliableCDMS.Helpers;
using System;
using System.Web.Http;

namespace ReliableCDMS.Controllers
{
    [RoutePrefix("api/cleanup")]
    public class CleanupController : ApiController
    {
        [HttpGet]
        public IHttpActionResult CleanupOrphanedFiles()
        {
            if (!SecurityHelper.IsAuthenticated() || !SecurityHelper.IsAdmin())
            {
                return Unauthorized();
            }

            var cleanupResult = CleanupHelper.RemoveOrphanedFiles();

            if (cleanupResult.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = cleanupResult.Message,
                    filesDeleted = cleanupResult.FilesDeleted,
                    deletedFiles = cleanupResult.DeletedFiles
                });
            }
            else
            {
                return InternalServerError(new Exception(cleanupResult.Message));
            }
        }
    }
}