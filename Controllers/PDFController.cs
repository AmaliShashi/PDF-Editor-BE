using Microsoft.AspNetCore.Mvc;
using PDFEditor.Services;
using PDFEditor.Models;

namespace PDFEditor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFController : ControllerBase
    {
        private readonly IPDFService _pdfService;
        private readonly ILogger<PDFController> _logger;

        public PDFController(IPDFService pdfService, ILogger<PDFController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPDF(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only PDF files are allowed");

                var result = await _pdfService.UploadPDFAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDF");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("preview/{fileId}")]
        public async Task<IActionResult> GetPDFPreview(string fileId, [FromQuery] int page = 1)
        {
            try
            {
                var preview = await _pdfService.GetPDFPreviewAsync(fileId, page);
                return Ok(preview);
            }
            catch (FileNotFoundException)
            {
                return NotFound("PDF file not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PDF preview");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("edit/{fileId}")]
        public async Task<IActionResult> EditPDF(string fileId, [FromBody] PDFEditRequest request)
        {
            try
            {
                var result = await _pdfService.EditPDFAsync(fileId, request);
                return Ok(result);
            }
            catch (FileNotFoundException)
            {
                return NotFound("PDF file not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing PDF");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("export/{fileId}")]
        public async Task<IActionResult> ExportPDF(string fileId, [FromBody] ExportRequest request)
        {
            try
            {
                var result = await _pdfService.ExportPDFAsync(fileId, request);
                
                return request.Format.ToLower() switch
                {
                    "pdf" => File(result.FileData, "application/pdf", result.FileName),
                    "docx" => File(result.FileData, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.FileName),
                    "images" => File(result.FileData, "application/zip", result.FileName),
                    _ => BadRequest("Unsupported export format")
                };
            }
            catch (FileNotFoundException)
            {
                return NotFound("PDF file not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PDF");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeletePDF(string fileId)
        {
            try
            {
                await _pdfService.DeletePDFAsync(fileId);
                return Ok();
            }
            catch (FileNotFoundException)
            {
                return NotFound("PDF file not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PDF");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}