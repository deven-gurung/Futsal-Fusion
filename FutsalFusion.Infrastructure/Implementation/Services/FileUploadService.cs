using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Domain.Utilities;

namespace FutsalFusion.Infrastructure.Implementation.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    public FileUploadService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public string UploadDocument(string uploadedFilePath, IFormFile file)
    {
        if (!Directory.Exists(Path.Combine(_webHostEnvironment.WebRootPath, uploadedFilePath)))
        {
            Directory.CreateDirectory(Path.Combine(_webHostEnvironment.WebRootPath, uploadedFilePath));
        }
        
        var uploadedDocumentPath = Path.Combine(_webHostEnvironment.WebRootPath, uploadedFilePath);
        
        var fileName = UploadFile(uploadedDocumentPath, file);
        
        return fileName;
    }
    
    private static string UploadFile(string uploadedFilePath, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);

        var fileName = extension.SetUniqueFileName();

        using var stream = new FileStream(Path.Combine(uploadedFilePath, fileName), FileMode.Create);
            
        file.CopyTo(stream);
            
        return fileName;
    }
}