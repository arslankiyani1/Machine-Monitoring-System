namespace MMS.Application.Common.Constants;

public record LoggingMessages
{
    public const string InvalidUserId = "Invalid user ID.";
    public const string MemberNotFound = "Member not found for ID {UserId}";
    public const string EmailServiceError = "Error occurred while sending email.";
    public const string EmailServiceStarting = "Service Start";
    public const string EmailServiceStopping = "Service Stop";
}