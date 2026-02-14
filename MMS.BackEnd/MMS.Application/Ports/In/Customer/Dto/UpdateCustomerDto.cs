namespace MMS.Application.Ports.In.Customer.Dto;

public record UpdateCustomerDto(
Guid Id,
string Name,
string? Email,
string PhoneCountryCode,
string PhoneNumber,
string TimeZone,
string Country,
string City,
string Street,
string PostalCode,
string Region,
string? State,
CustomerStatus Status, 
List<Shift> Shifts,
string? ImageBase64
);