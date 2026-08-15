using Abp.Authorization;
using Abp.Domain.Repositories;
using EnterpriseBase.Controllers;
using EnterpriseBase.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseBase.Web.Host.Controllers
{
    /// <summary>
    /// Upload/serve for product and store photos - DB-backed via the existing
    /// BinaryObject/IBinaryObjectManager scaffolding (Storage/BinaryObject.cs),
    /// which was never wired up to any endpoint before this. Callers upload
    /// first to get an ImageId, then pass that ImageId to the existing
    /// Retailer/Store/Product DTOs that already had an ImageId field.
    /// </summary>
    [Route("api/Image")]
    public class ImageController : EnterpriseBaseControllerBase
    {
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<BinaryObject, Guid> _binaryObjectRepository;

        public ImageController(
            IBinaryObjectManager binaryObjectManager,
            IRepository<BinaryObject, Guid> binaryObjectRepository)
        {
            _binaryObjectManager = binaryObjectManager;
            _binaryObjectRepository = binaryObjectRepository;
        }

        [AbpAuthorize]
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Deliberately BadRequest(message), not a thrown UserFriendlyException:
            // ABP's exception-to-JSON-envelope conversion only applies to
            // dynamic-API app-service methods, not plain MVC controllers like
            // this one - throwing here reaches Kestrel unhandled and returns
            // an empty response (confirmed empirically). BadRequest(string)
            // still gets wrapped into the usual {result, success, ...}
            // envelope, just with the message under `result` rather than
            // `error.message` - the frontend client accounts for both shapes.
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest("Only JPEG, PNG, and WebP images are allowed.");
            }

            if (file.Length > BinaryObjectConsts.BytesMaxSize)
            {
                return BadRequest($"Image must be smaller than {BinaryObjectConsts.BytesMaxSize / 1024} KB.");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                bytes = stream.ToArray();
            }

            // Product/store photos are part of the public catalog - same
            // reasoning as Store/Product themselves not being tenant-scoped
            // (see Store.cs's doc comment). TenantId is forced null here,
            // regardless of the uploader's own session tenant, so an
            // anonymous shopper's GET below can always find it.
            var binaryObject = new BinaryObject(null, bytes, file.ContentType);
            await _binaryObjectManager.SaveAsync(binaryObject);

            return Ok(new { imageId = binaryObject.Id });
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var binaryObject = await _binaryObjectRepository.GetAll()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (binaryObject == null)
            {
                return NotFound();
            }

            var contentType = string.IsNullOrEmpty(binaryObject.Description)
                ? "application/octet-stream"
                : binaryObject.Description;

            return File(binaryObject.Bytes, contentType);
        }
    }
}
