using Microsoft.AspNetCore.Http;

namespace FutsalFusion.Application.Interfaces.Services;

public interface IFileUploadService
{
    string UploadDocument(string uploadedFilePath, IFormFile file);
}