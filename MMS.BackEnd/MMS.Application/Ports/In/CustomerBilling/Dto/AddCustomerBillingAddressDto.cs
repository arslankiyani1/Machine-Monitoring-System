namespace MMS.Application.Ports.In.CustomerBilling.Dto;

public record AddCustomerBillingAddressDto(
    string Country,
    string Region,
    string ZipCode,
    string City,
    string State,
    string Street,
    Guid CustomerId
);