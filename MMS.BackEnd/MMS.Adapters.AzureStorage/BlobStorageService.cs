

using DocumentFormat.OpenXml.Bibliography;
using MMS.Application.Common;
using System.Text.RegularExpressions;

namespace MMS.Adapters.AzureStorage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly string _containerName;
    private static readonly HashSet<string> AllowedImageTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp",
        "image/svg+xml",
        "image/x-icon"
    };
    public BlobStorageService(BlobServiceClient client, IOptions<BlobStorageOptions> opts)
    {
        var options = opts.Value;
        _containerName = options.ContainerName;
        _container = client.GetBlobContainerClient(options.ContainerName);
    }

    private static string GenerateBlobName(IFormFile file)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // normalize filename: lowercase, replace spaces, remove invalid chars
        var normalizedFileName = Regex.Replace(fileName.ToLowerInvariant(), @"[^a-z0-9_-]", "_");

        var blobName = $"{timestamp}_{normalizedFileName}{extension}";
        return blobName;
    }

    public async Task<Uri> UploadAsync(IFormFile file)
    {
        var allowedImageTypes = new HashSet<string>
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp",
            "image/svg+xml",
            "image/x-icon"
        };

        if (!allowedImageTypes.Contains(file.ContentType.ToLower()))
        {
            throw new InvalidOperationException("Invalid file type. Allowed types are: jpeg, png, gif, bmp, webp, svg, ico.");
        }

        var blobName = GenerateBlobName(file);
        var blobClient = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = file.ContentType
        });

        return blobClient.Uri;
    }

    public async Task<Uri> UpdateAsync(string blobName, IFormFile file, CancellationToken ct = default)
    {
        var blobClient = _container.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Blob '{blobName}' not found.");

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = file.ContentType
        }, cancellationToken: ct);

        return blobClient.Uri;
    }

    public async Task<bool> DeleteAsync(string blobName, CancellationToken ct = default)
    {
        var blobPath = GetBlobPathFromUrl(blobName, out _);
        var blobClient = _container.GetBlobClient(blobPath);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        return response.Value;
    }

    public async Task<int> DeleteAllAsync(IEnumerable<string> blobUrls, CancellationToken ct = default)
    {
        int deletedCount = 0;

        foreach (var url in blobUrls)
        {
            var blobPath = GetBlobPathFromUrl(url, out _);
            var blobClient = _container.GetBlobClient(blobPath);

            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            if (response.Value)
            {
                deletedCount++;
            }
        }

        return deletedCount; // number of successfully deleted blobs
    }

    public async Task<ApiResponse<List<Uri>>> UploadAllAsync(List<IFormFile> files)
    {
        var result = new List<Uri>();

        foreach (var file in files)
        {
            var blobName = GenerateBlobName(file);
            var blobClient = _container.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            result.Add(blobClient.Uri);
        }

        return new ApiResponse<List<Uri>>(
            StatusCodes.Status201Created,
            "Files uploaded successfully.",
            result
        );
    }


    private static string GetContentTypeFromBase64(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            throw new ArgumentException("Base64 string cannot be empty.");

        if (base64String.StartsWith("data:"))
        {
            var header = base64String.Substring(0, base64String.IndexOf(';'));
            var contentType = header.Replace("data:", "").Trim();
            if (!AllowedImageTypes.Contains(contentType.ToLower()))
                throw new InvalidOperationException($"Invalid image type. Allowed types are: {string.Join(", ", AllowedImageTypes)}.");
            return contentType;
        }

        return "image/jpeg";
    }

    private static string ExtractBase64Data(string base64String)
    {
        if (base64String.StartsWith("data:"))
        {
            var parts = base64String.Split(',');
            if (parts.Length != 2 || !parts[0].Contains("base64"))
                throw new ArgumentException("Invalid base64 data URI format.");
            return parts[1];
        }
        return base64String;
    }

    private string GetBlobPathFromUrl(string url, out string folderName)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Blob URL is required.");

        var decodedUrl = Uri.UnescapeDataString(url);
        Console.WriteLine($"Input URL: {decodedUrl}");

        try
        {
            var uri = new Uri(decodedUrl);
            var path = uri.AbsolutePath;
            var containerPrefix = $"/{_containerName}/";
            if (!path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Blob URL must contain the container name '{_containerName}'.");

            var blobPath = path.Substring(containerPrefix.Length);
            // Extract folder name (e.g., "customer" from "customer/2025_09_11_...")
            var firstSlashIndex = blobPath.IndexOf('/');
            if (firstSlashIndex == -1)
            {
                folderName = null; // No folder in path
                Console.WriteLine($"Parsed blob path: {blobPath}, no folder detected");
                return blobPath;
            }

            folderName = blobPath.Substring(0, firstSlashIndex);
            Console.WriteLine($"Parsed blob path: {blobPath}, folder: {folderName}");
            return blobPath;
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException("Invalid blob URL format.", ex);
        }
    }

    private static string GenerateBlobName(string extension, string folderName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss");
        var guid = Guid.NewGuid().ToString();
        var blobName = $"{timestamp}_{guid}{extension}";
        return string.IsNullOrEmpty(folderName) ? blobName : $"{folderName}/{blobName}";
    }

    public async Task<Uri> UploadBase64Async(string base64String, string folderName, CancellationToken ct = default)
    {
        var contentType = GetContentTypeFromBase64(base64String);
        var cleanBase64 = ExtractBase64Data(base64String);
        var extension = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            "image/x-icon" => ".ico",
            _ => ".jpg"
        };

        // Use folderName directly (extracted from URL or provided)
        var blobName = GenerateBlobName(extension, folderName);
        var blobClient = _container.GetBlobClient(blobName);

        try
        {
            var bytes = Convert.FromBase64String(cleanBase64);
            await using var stream = new MemoryStream(bytes);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = contentType
            }, cancellationToken: ct);
            Console.WriteLine($"Uploaded blob: {blobClient.Uri}");
            return blobClient.Uri;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid base64 string format.", ex);
        }
    }

    public async Task<Uri> UpdateBase64Async(string blobUrl, string base64String, CancellationToken ct = default)
    {
        var blobPath = GetBlobPathFromUrl(blobUrl, out _);
        var blobClient = _container.GetBlobClient(blobPath);

        if (!await blobClient.ExistsAsync(ct))
            throw new FileNotFoundException($"Blob '{blobPath}' not found.");

        var contentType = GetContentTypeFromBase64(base64String);
        var cleanBase64 = ExtractBase64Data(base64String);

        try
        {
            var bytes = Convert.FromBase64String(cleanBase64);
            await using var stream = new MemoryStream(bytes);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
            {
                ContentType = contentType
            }, cancellationToken: ct);
            Console.WriteLine($"Updated blob: {blobClient.Uri}");
            return blobClient.Uri;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid base64 string format.", ex);
        }
    }

    public async Task<bool> DeleteBase64Async(string blobUrl, CancellationToken ct = default)
    {
        var blobPath = GetBlobPathFromUrl(blobUrl, out _);
        var blobClient = _container.GetBlobClient(blobPath);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        Console.WriteLine($"Delete blob '{blobPath}': {response.Value}");
        return response.Value;
    }

    public async Task<Uri> UploadReportAsync(byte[] fileBytes, string fileName, string folderName = null,string contentType = "application/octet-stream", CancellationToken ct = default)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            throw new ArgumentException("File bytes cannot be empty.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.");

        // Ensure container exists
        await _container.CreateIfNotExistsAsync(cancellationToken: ct);

        // Normalize fileName if needed (similar to GenerateBlobName)
        var normalizedFileName = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        normalizedFileName = Regex.Replace(normalizedFileName.ToLowerInvariant(), @"[^a-z0-9_-]", "_");

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var blobName = $"{timestamp}_{normalizedFileName}{extension}";
        if (!string.IsNullOrEmpty(folderName))
        {
            blobName = $"{folderName}/{blobName}";
        }

        var blobClient = _container.GetBlobClient(blobName);

        await using var stream = new MemoryStream(fileBytes);
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: ct);

        return blobClient.Uri;
    }

}
