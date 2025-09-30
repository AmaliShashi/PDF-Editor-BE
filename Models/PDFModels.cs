namespace PDFEditor.Models
{
    public class UploadResult
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public class PDFPreview
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string ImageData { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
    }

    public class PDFEditRequest
    {
        public List<TextOverlay> TextOverlays { get; set; } = new();
        public List<TextReplacement> TextReplacements { get; set; } = new();
        public PDFMetadata? Metadata { get; set; }
    }

    public class TextOverlay
    {
        public int Page { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = 12;
        public ColorInfo Color { get; set; } = new() { R = 0, G = 0, B = 0, A = 255 };
    }

    public class TextReplacement
    {
        public string OldText { get; set; } = string.Empty;
        public string NewText { get; set; } = string.Empty;
    }

    public class PDFMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }

    public class ColorInfo
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; } = 255;
    }

    public class EditResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ExportRequest
    {
        public string Format { get; set; } = string.Empty; // "pdf", "docx", "images"
        public string FileName { get; set; } = string.Empty;
    }

    public class ExportResult
    {
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}