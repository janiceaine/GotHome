using Microsoft.AspNetCore.Http;

namespace GotHome.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile imageFile);
}
