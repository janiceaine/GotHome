namespace GotHome.Attributes;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class ImageFileAttribute : ValidationAttribute
{
    private readonly long _maxFileSize = 2 * 1024 * 1024; // 2 MB
    private readonly string[] _permittedExtensions = new[]
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            // Optional upload: no file is OK. Use [Required] if you want to force it.
            return ValidationResult.Success;
        }

        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult($"Max file size is {_maxFileSize / (1024 * 1024)} MB.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                return new ValidationResult(
                    "Invalid file type. Only JPG, JPEG, PNG, GIF, and WEBP are allowed."
                );
            }
        }

        return ValidationResult.Success;
    }
}
