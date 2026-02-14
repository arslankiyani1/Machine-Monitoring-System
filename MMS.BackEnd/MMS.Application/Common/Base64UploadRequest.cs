namespace MMS.Application.Common;

public class Base64UploadRequest
{
    [Required]
    public string Base64String { get; set; }

    [Required]
    [RegularExpression("^(user|customer|machines)$", ErrorMessage = "FolderName must be 'user', 'customer', or 'machines'.")]
    public string FolderName { get; set; }
}

public class Base64MultiUploadRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one base64 string is required.")]
    public List<string> Base64Strings { get; set; }

    [Required]
    [RegularExpression("^(user|customer|machines)$", ErrorMessage = "FolderName must be 'user', 'customer', or 'machines'.")]
    public string FolderName { get; set; }
}


public class Base64UpdateRequest
{
    [Required]
    public string Base64String { get; set; }

    [Required]
    public string BlobName { get; set; }
}