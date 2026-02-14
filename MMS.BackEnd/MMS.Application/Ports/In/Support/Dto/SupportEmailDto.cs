namespace MMS.Application.Ports.In.Support.Dto;

public record SupportEmailDto
(
    string? Name,
    string? Email,
    string? Subject,
    string? Message
);
