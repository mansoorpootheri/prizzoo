using EnterpriseBase.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace EnterpriseBase.Web.Host.Controllers
{
    [Route("api/File")]
    public class FileController : EnterpriseBaseControllerBase
    {
        [HttpGet("DownloadTempFile")]
        public IActionResult DownloadTempFile(string fileToken, string fileName, string fileType)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), fileToken + ".xlsx");

            if (!System.IO.File.Exists(tempPath))
            {
                return NotFound("File not found or expired.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(tempPath);
            System.IO.File.Delete(tempPath);

            return File(fileBytes, fileType, fileName);
        }

        [HttpGet("DownloadBackup")]
        public IActionResult DownloadBackup(string fileToken, string fileName)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), fileToken + ".backup");
            var markerPath = tempPath + ".done";

            if (!System.IO.File.Exists(tempPath) || !System.IO.File.Exists(markerPath))
            {
                return NotFound("Backup file not ready or not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(tempPath);

            // Cleanup
            System.IO.File.Delete(tempPath);
            System.IO.File.Delete(markerPath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
