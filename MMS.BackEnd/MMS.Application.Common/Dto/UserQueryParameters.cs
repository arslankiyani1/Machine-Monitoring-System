namespace MMS.Application.Common.Dto;

public class UserQueryParameters : PageParameters
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("exact")]
    public bool? Exact { get; set; }

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }
}