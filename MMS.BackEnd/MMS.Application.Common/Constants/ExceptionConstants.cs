namespace MMS.Application.Common.Constants;

public record ExceptionConstants
{
    public const string UserProfileNotFound = "No user profiles found.";
    public const string ContainerNameNullOrEmpty = "Container name cannot be null or empty.";
    public const string FileNameNullOrEmpty = "File name cannot be null or empty.";
    public const string ContentNullOrEmpty = "Content cannot be null or empty.";
    public const string FilePathNullOrEmpty = "File path cannot be null or empty.";
    public const string InvalidFilePath = "File path must contain a valid file name.";
    public const string FailedToSaveFile = "Failed to save file '{0}' to container '{1}'.";
    public const string FailedToRemoveFile = "Failed to remove file from container '{0}'.";
    public const string FailedToGetAccessibleUrl = "Failed to get accessible URL for file '{0}' in container '{1}'.";
    public const string BlobClientNull = "Blob client cannot be null.";
}