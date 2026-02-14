namespace MMS.Adapters.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlobController : ControllerBase
{
    private readonly IBlobStorageService _blobService;

    public BlobController(IBlobStorageService blobService)
    {
        _blobService = blobService;
    }

    /// <summary>
    /// Uploads one or more files to blob storage.
    /// </summary>
    /// <param name="files">The list of files to upload.</param>
    /// <returns>The URIs of the uploaded blobs.</returns>
    [HttpPost("upload-files")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Uploads files to blob storage.",
        Description = "Accepts one or more files and uploads them to the configured blob storage container. Returns the URIs of the uploaded blobs."
    )]
    public async Task<IActionResult> Uploads(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            var errorResponse = new ApiResponse<string>(
                StatusCodes.Status400BadRequest,
                "No files uploaded.",
                null
            );
            return StatusCode(errorResponse.StatusCode, errorResponse);
        }

        var response = await _blobService.UploadAllAsync(files);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Uploads a single file to blob storage.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    [HttpPost("Upload")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Uploads a single file to blob storage.",
        Description = "Accepts one file and uploads it to the configured blob storage container. Returns the URI of the uploaded blob."
    )]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null)
            return BadRequest("No file uploaded.");

        var uri = await _blobService.UploadAsync(file);
        return Created(string.Empty, uri);
    }

    /// <summary>
    /// Updates an existing blob file in storage.
    /// </summary>
    /// <param name="blobName">The name of the blob to update.</param>
    /// <param name="file">The new file to replace the existing blob.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated blob's URI.</returns>
    [HttpPut("{blobName}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Updates a blob file.",
        Description = "Replaces the existing blob file identified by its name with a new uploaded file. Returns the updated blob URI."
    )]
    public async Task<IActionResult> Update(string blobName, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            var existingBlobName = Path.GetFileName(new Uri(blobName).LocalPath);
            var uri = await _blobService.UpdateAsync(existingBlobName, file, ct);
            return Ok(new { uri });
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Blob '{blobName}' does not exist.");
        }
    }

    /// <summary>
    /// Deletes a blob file from storage.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Information about the deletion result.</returns>
    [HttpDelete("delete/{blobName}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Deletes a blob file.",
        Description = "Deletes the blob file from storage identified by the provided name. Returns the status of the deletion."
    )]
    public async Task<IActionResult> Delete(string blobName, CancellationToken ct)
    {
        var deleted = await _blobService.DeleteAsync(blobName, ct);
        return Ok(new
        {
            blobName,
            deleted,
            message = deleted ? "Blob deleted successfully." : "Blob not found."
        });
    }


    /// <summary>
    /// Deletes multiple blob files from storage.
    /// </summary>
    /// <param name="blobNames">List of blob names/URLs to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Information about the deletion results.</returns>
    [HttpDelete("delete-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Deletes multiple blob files.",
        Description = "Deletes all the blob files identified by the provided names or URLs. Returns the number of successfully deleted blobs."
    )]
    public async Task<IActionResult> DeleteAll([FromBody] List<string> blobNames, CancellationToken ct)
    {
        if (blobNames == null || blobNames.Count == 0)
        {
            return BadRequest(new { message = "No blob names provided." });
        }

        var deletedCount = await _blobService.DeleteAllAsync(blobNames, ct);

        return Ok(new
        {
            totalRequested = blobNames.Count,
            totalDeleted = deletedCount,
            message = $"{deletedCount} of {blobNames.Count} blobs deleted."
        });
    }



    /// <summary>
    /// Uploads a single base64-encoded image to blob storage.
    /// </summary>
    /// <param name="request">The base64 string and folder name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    [HttpPost("UploadBase64")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Uploads a base64-encoded image to blob storage.",
        Description = "Accepts a base64-encoded image string and folder name ('user', 'customer', or 'machines'). Uploads to the configured blob storage container and returns the URI of the uploaded blob."
    )]
    public async Task<IActionResult> UploadBase64([FromBody] Base64UploadRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Base64String))
            return BadRequest("No base64 string provided.");

        try
        {
            var uri = await _blobService.UploadBase64Async(request.Base64String, request.FolderName, ct);
            return Created(string.Empty, uri);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }



    /// <summary>
    /// Updates an existing blob with a base64-encoded image.
    /// </summary>
    /// <param name="request">The base64 string and blob name to replace the existing blob.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated blob's URI.</returns>
    [HttpPut("UpdateBase64")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Updates a blob with a base64-encoded image.",
        Description = "Replaces the existing blob identified by the provided blob name in the request body with a new base64-encoded image. Returns the updated blob URI."
    )]
    public async Task<IActionResult> UpdateBase64([FromBody] Base64UpdateRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Base64String) || string.IsNullOrWhiteSpace(request.BlobName))
            return BadRequest("Base64 string and blob name are required.");

        try
        {
            var uri = await _blobService.UpdateBase64Async(request.BlobName, request.Base64String, ct);
            return Ok(new { uri });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Blob '{request.BlobName}' does not exist.");
        }
    }

    /// <summary>
    /// Deletes a blob file from storage using base64 method.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Information about the deletion result.</returns>
    [HttpDelete("DeleteBase64/{blobName}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Deletes a blob file using base64 method.",
        Description = "Deletes the blob file from storage identified by the provided name using the base64 delete method. Returns the status of the deletion."
    )]
    public async Task<IActionResult> DeleteBase64(string blobName, CancellationToken ct)
    {
        var deleted = await _blobService.DeleteBase64Async(blobName, ct);
        return Ok(new
        {
            blobName,
            deleted,
            message = deleted ? "Blob deleted successfully." : "Blob not found."
        });
    }

}