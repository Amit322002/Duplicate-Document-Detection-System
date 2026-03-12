using Microsoft.AspNetCore.Mvc;
using DuplicateDocsFinder.Service;
using DuplicateDocsFinder.Dto;

namespace DuplicateDocsFinder.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload_file")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            var result = await _documentService.UploadDocumentAsync(dto);

            return StatusCode(result.StatusCode, result);
        }
    }
}