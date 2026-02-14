using DocumentFormat.OpenXml.Bibliography;

namespace MMS.Application.Interfaces;

public interface IBlobStorageService
{
    Task<Uri> UploadAsync(IFormFile files);
    Task<ApiResponse<List<Uri>>> UploadAllAsync(List<IFormFile> files);
    Task<Uri> UpdateAsync(string blobName, IFormFile file, CancellationToken ct = default);
    Task<bool> DeleteAsync(string blobName, CancellationToken ct = default);
    Task<int> DeleteAllAsync(IEnumerable<string> blobUrls, CancellationToken ct = default);

    Task<Uri> UploadBase64Async(string base64String, string folderName, CancellationToken ct = default);
    Task<Uri> UpdateBase64Async(string blobUrl, string base64String, CancellationToken ct = default);
    Task<bool> DeleteBase64Async(string blobUrl, CancellationToken ct = default);
    Task<Uri> UploadReportAsync(byte[] fileBytes, string fileName, string folderName = null,string contentType = "application/octet-stream",  CancellationToken ct = default);
}