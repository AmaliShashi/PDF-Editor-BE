using PDFEditor.Models;

namespace PDFEditor.Services
{
    public interface IPDFService
    {
        Task<UploadResult> UploadPDFAsync(IFormFile file);
        Task<PDFPreview> GetPDFPreviewAsync(string fileId, int page);
        Task<EditResult> EditPDFAsync(string fileId, PDFEditRequest request);
        Task<ExportResult> ExportPDFAsync(string fileId, ExportRequest request);
        Task DeletePDFAsync(string fileId);
    }
}