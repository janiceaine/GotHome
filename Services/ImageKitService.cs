using GotHome.Services;
using Imagekit.Sdk;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public class ImageKitService : IImageService
{
    private readonly ImagekitClient _imagekit;
    private readonly IConfiguration _configuration;
    private readonly string _publicKey;
    private readonly string _privateKey;
    private readonly string _urlEndpoint;

    public ImageKitService(IConfiguration config)
    {
        _configuration = config;
        _publicKey = config["ImageKit:PublicKey"] ?? "";
        _privateKey = config["ImageKit:PrivateKey"] ?? "";
        _urlEndpoint = config["ImageKit:UrlEndpoint"] ?? "";
        _imagekit = new ImagekitClient(_publicKey, _privateKey, _urlEndpoint);
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            throw new ArgumentException("Invalid image file.");
        }

        var base64Image = await ConvertToBase64Async(imageFile);
        var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
        var request = new FileCreateRequest()
        {
            file = base64Image,
            fileName = fileName,
            // folder = "gothome", // optional Imagekit folder name (if you have one)
        };

        var response = await _imagekit.UploadAsync(request);

        if (response.HttpStatusCode != 200)
        {
            throw new Exception("Image upload failed.");
        }

        return response.url;
    }

    private static async Task<string> ConvertToBase64Async(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Invalid file.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        return Convert.ToBase64String(fileBytes);
    }
}
