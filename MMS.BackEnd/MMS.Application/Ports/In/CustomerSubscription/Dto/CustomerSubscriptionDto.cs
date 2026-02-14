namespace MMS.Application.Ports.In.CustomerSubscription.Dto;

public record CustomerSubscriptionDto(
    Guid Id,
    Guid CustomerId,
    Guid SubscriptionId,
    Guid InvoiceId,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    RenewalType RenewalType,
    string Status
);
