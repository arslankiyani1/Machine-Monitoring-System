namespace MMS.Adapters.Rest.Filters;

[ExcludeFromCodeCoverage]
public class ExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (!(context.Exception is not RepositoryError)) return;

        string message;
        int statusCode;
        string code;
        switch (context.Exception)
        {
            case ValidationError validationError:
                message = validationError.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "ValidationError";
                break;

            case EntityNotRemovable entityNotRemoveable:
                message = entityNotRemoveable.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "EntityNotRemoveable";
                break;

            case ForbiddenAccess forbiddenAccess:
                message = forbiddenAccess.Message;
                statusCode = StatusCodes.Status403Forbidden;
                code = "Forbidden";
                break;

            case EntityNotFound entityNotFound:
                message = entityNotFound.Message;
                statusCode = StatusCodes.Status404NotFound;
                code = "NotFound";
                break;

            case EntityConflict conflictError:
                message = conflictError.Message;
                statusCode = StatusCodes.Status409Conflict;
                code = "Conflict";
                break;

            case EntitySoftDeleted entitySoftDeleted:
                message = entitySoftDeleted.Message;
                statusCode = StatusCodes.Status410Gone;
                code = "EntitySoftDeleted";
                break;

            case LimitExceeded limitExceeded:
                message = limitExceeded.Message;
                statusCode = StatusCodes.Status429TooManyRequests;
                code = "LimitExceeded";
                break;
            case InvalidContainerNameException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidContainerName";
                break;

            case InvalidFileNameException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidFileName";
                break;

            case InvalidContentException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidContent";
                break;

            case BlobContainerCreationException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "BlobContainerCreationError";
                break;

            case BlobUploadException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "BlobUploadError";
                break;

            case InvalidFilePathException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidFilePath";
                break;

            case BlobDeletionException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "BlobDeletionError";
                break;

            case BlobNotFoundException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status404NotFound;
                code = "BlobNotFound";
                break;

            case FailedToGenerateSasUrlException:
                message = context.Exception.Message;
                statusCode = StatusCodes.Status500InternalServerError;
                code = "FailedToGenerateSasUrl";
                break;

            case InvalidBlobClientException invalidBlobClientException:
                message = invalidBlobClientException.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidBlobClient";
                break;

            case InvalidExpiryDurationException invalidExpiryDurationException:
                message = invalidExpiryDurationException.Message;
                statusCode = StatusCodes.Status400BadRequest;
                code = "InvalidExpiryDuration";
                break;

            case SasUriGenerationException sasUriGenerationException:
                message = sasUriGenerationException.Message;
                statusCode = StatusCodes.Status500InternalServerError;
                code = "SasUriGenerationError";
                break;

            default:
                message = "An unexpected error occurred.";
                statusCode = StatusCodes.Status500InternalServerError;
                code = "InternalServerError";
                break;
        }

        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.StatusCode = statusCode;
        context.Result = new JsonResult(new { message, code, statusCode });
    }
}