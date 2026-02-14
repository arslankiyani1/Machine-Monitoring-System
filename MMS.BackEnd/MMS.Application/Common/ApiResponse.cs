namespace MMS.Application.Common;

public class ApiResponse<T>(int statusCode, string message, T? data = default)
{
    public int StatusCode { get; } = statusCode;
    public string Message { get; } = message;
    public T? Data { get; } = data;
}