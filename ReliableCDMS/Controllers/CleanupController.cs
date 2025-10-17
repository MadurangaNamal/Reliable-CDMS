using ReliableCDMS.Helpers;
using System;
using System.Web.Http;

namespace ReliableCDMS.Controllers
{
    [RoutePrefix("api/admin")]
    public class CleanupController : ApiController
    {
        [HttpGet]
        [Route("cleanup")]
        public IHttpActionResult CleanupOrphanedFiles()
        {
            if (!User.Identity.IsAuthenticated)
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