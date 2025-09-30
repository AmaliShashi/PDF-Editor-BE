# PDF Editor Backend - C# ASP.NET Core MVC

This is the backend API for the PDF Editor application built with ASP.NET Core MVC.

## Features

- Upload PDF files
- Preview PDF pages as images
- Edit PDFs (text overlay, text replacement, metadata updates)
- Export to multiple formats:
  - PDF (updated version)
  - Word (.docx)
  - Images (PNG files in ZIP)

## Setup Instructions

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Aspose licenses (trial versions work for evaluation)

   The API will be available at:
   - Swagger UI: `https://localhost:44322/swagger`

  ## API Endpoints

### Upload PDF
```
POST /api/pdf/upload
Content-Type: multipart/form-data
Body: PDF file
```

### Get PDF Preview
```
GET /api/pdf/preview/{fileId}?page=1
Returns: Base64 image and text content
```

### Edit PDF
```
POST /api/pdf/edit/{fileId}
Content-Type: application/json
Body: PDFEditRequest
```

### Export PDF
```
POST /api/pdf/export/{fileId}
Content-Type: application/json
Body: ExportRequest
Returns: File download
```

### Delete PDF
```
DELETE /api/pdf/{fileId}
```

## Libraries Used

- **Aspose.PDF**: PDF manipulation, text extraction, page rendering
- **Aspose.Words**: PDF to Word conversion
- **System.Drawing.Common**: Image processing
- **ASP.NET Core**: Web API framework

## Assumptions Made

1. **Temporary Storage**: Files are stored locally in the `uploads` folder
4. **Basic Security**: No authentication for simplicity
5. **File Cleanup**: Manual cleanup of uploaded files
