using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Words;
using PDFEditor.Models;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Imaging;

namespace PDFEditor.Services
{
    public class PDFService : IPDFService
    {
        private readonly string _uploadPath;
        private readonly ILogger<PDFService> _logger;

        public PDFService(IConfiguration configuration, ILogger<PDFService> logger)
        {
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads";
            _logger = logger;
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<UploadResult> UploadPDFAsync(IFormFile file)
        {
            var fileId = Guid.NewGuid().ToString();
            var fileName = $"{fileId}.pdf";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Get PDF info
            using var document = new Aspose.Pdf.Document(filePath);
            var pageCount = document.Pages.Count;

            return new UploadResult
            {
                FileId = fileId,
                FileName = file.FileName,
                PageCount = pageCount,
                FileSizeBytes = file.Length
            };
        }

        public async Task<PDFPreview> GetPDFPreviewAsync(string fileId, int page)
        {
            var filePath = Path.Combine(_uploadPath, $"{fileId}.pdf");
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found");

            using var document = new Aspose.Pdf.Document(filePath);
            
            if (page < 1 || page > document.Pages.Count)
                throw new ArgumentOutOfRangeException(nameof(page), "Invalid page number");

            // Convert page to image for preview
            var resolution = new Aspose.Pdf.Devices.Resolution(150);
            var pngDevice = new Aspose.Pdf.Devices.PngDevice(resolution);
            
            using var imageStream = new MemoryStream();
            pngDevice.Process(document.Pages[page], imageStream);
            
            var imageBase64 = Convert.ToBase64String(imageStream.ToArray());

            // Extract text from page
            var textAbsorber = new TextAbsorber();
            document.Pages[page].Accept(textAbsorber);

            return new PDFPreview
            {
                PageNumber = page,
                TotalPages = document.Pages.Count,
                ImageData = $"data:image/png;base64,{imageBase64}",
                TextContent = textAbsorber.Text
            };
        }

        public async Task<EditResult> EditPDFAsync(string fileId, PDFEditRequest request)
        {
            var filePath = Path.Combine(_uploadPath, $"{fileId}.pdf");
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found");

            using var document = new Aspose.Pdf.Document(filePath);

            // Apply text overlays
            foreach (var overlay in request.TextOverlays)
            {
                if (overlay.Page > 0 && overlay.Page <= document.Pages.Count)
                {
                    var page = document.Pages[overlay.Page];
                    var textFragment = new TextFragment(overlay.Text)
                    {
                        Position = new Position(overlay.X, overlay.Y)
                    };
                    
                    textFragment.TextState.FontSize = (float)overlay.FontSize;
                    textFragment.TextState.ForegroundColor = Aspose.Pdf.Color.FromArgb(
                        overlay.Color.A, overlay.Color.R, overlay.Color.G, overlay.Color.B);
                    
                    page.Paragraphs.Add(textFragment);
                }
            }

            // Apply text replacements
            foreach (var replacement in request.TextReplacements)
            {
                var textFragmentAbsorber = new TextFragmentAbsorber(replacement.OldText);
                document.Pages.Accept(textFragmentAbsorber);

                foreach (TextFragment textFragment in textFragmentAbsorber.TextFragments)
                {
                    textFragment.Text = replacement.NewText;
                }
            }

            // Update metadata
            if (request.Metadata != null)
            {
                var info = document.Info;
                if (!string.IsNullOrEmpty(request.Metadata.Title))
                    info.Title = request.Metadata.Title;
                if (!string.IsNullOrEmpty(request.Metadata.Author))
                    info.Author = request.Metadata.Author;
                if (!string.IsNullOrEmpty(request.Metadata.Subject))
                    info.Subject = request.Metadata.Subject;
            }

            // Save edited document
            var editedFilePath = Path.Combine(_uploadPath, $"{fileId}_edited.pdf");
            document.Save(editedFilePath);

            return new EditResult
            {
                Success = true,
                Message = "PDF edited successfully"
            };
        }

        public async Task<ExportResult> ExportPDFAsync(string fileId, ExportRequest request)
        {
            var filePath = Path.Combine(_uploadPath, $"{fileId}_edited.pdf");
            
            // Fallback to original if edited doesn't exist
            if (!File.Exists(filePath))
                filePath = Path.Combine(_uploadPath, $"{fileId}.pdf");
            
            if (!File.Exists(filePath))
                throw new FileNotFoundException("PDF file not found");

            return request.Format.ToLower() switch
            {
                "pdf" => await ExportToPDFAsync(filePath, request.FileName),
                "docx" => await ExportToWordAsync(filePath, request.FileName),
                "images" => await ExportToImagesAsync(filePath, request.FileName),
                _ => throw new ArgumentException("Unsupported export format")
            };
        }

        private async Task<ExportResult> ExportToPDFAsync(string filePath, string fileName)
        {
            var fileData = await File.ReadAllBytesAsync(filePath);
            var exportFileName = !string.IsNullOrEmpty(fileName) ? $"{fileName}.pdf" : "exported.pdf";
            
            return new ExportResult
            {
                FileData = fileData,
                FileName = exportFileName,
                ContentType = "application/pdf"
            };
        }

        private async Task<ExportResult> ExportToWordAsync(string filePath, string fileName)
        {
            using var document = new Aspose.Pdf.Document(filePath);
            using var docStream = new MemoryStream();
            
            // Convert PDF to Word
            document.Save(docStream, Aspose.Pdf.SaveFormat.DocX);
            
            var exportFileName = !string.IsNullOrEmpty(fileName) ? $"{fileName}.docx" : "exported.docx";
            
            return new ExportResult
            {
                FileData = docStream.ToArray(),
                FileName = exportFileName,
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };
        }

        private async Task<ExportResult> ExportToImagesAsync(string filePath, string fileName)
        {
            using var document = new Aspose.Pdf.Document(filePath);
            using var zipStream = new MemoryStream();
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
            
            var resolution = new Aspose.Pdf.Devices.Resolution(300);
            var pngDevice = new Aspose.Pdf.Devices.PngDevice(resolution);
            
            for (int i = 1; i <= document.Pages.Count; i++)
            {
                using var imageStream = new MemoryStream();
                pngDevice.Process(document.Pages[i], imageStream);
                
                var entry = archive.CreateEntry($"page_{i}.png");
                using var entryStream = entry.Open();
                imageStream.Seek(0, SeekOrigin.Begin);
                await imageStream.CopyToAsync(entryStream);
            }
            
            var exportFileName = !string.IsNullOrEmpty(fileName) ? $"{fileName}_images.zip" : "exported_images.zip";
            
            return new ExportResult
            {
                FileData = zipStream.ToArray(),
                FileName = exportFileName,
                ContentType = "application/zip"
            };
        }

        public async Task DeletePDFAsync(string fileId)
        {
            var files = new[]
            {
                Path.Combine(_uploadPath, $"{fileId}.pdf"),
                Path.Combine(_uploadPath, $"{fileId}_edited.pdf")
            };

            foreach (var file in files)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
    }
}