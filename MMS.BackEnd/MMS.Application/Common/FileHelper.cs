namespace MMS.Application.Common;

[ExcludeFromCodeCoverage]
public class FileHelper
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".svg"];

    public static Stream ConvertBase64ToStream(string base64String)
    {
        var imageBytes = Convert.FromBase64String(base64String);
        return new MemoryStream(imageBytes);
    }

    public static void ValidateFileExtension(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new Exception("Invalid file");
        }
    }
}