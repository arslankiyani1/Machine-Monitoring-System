namespace MMS.Application.Ports.In.Subscription.Dto;

public record SubscriptionAddDto(
    string Name,
    string Currency,
    decimal Price,
    BillingCycle BillingCycle,
    SubscriptionStatus Status,
    Dictionary<string, object> Features
);