namespace MMS.Application.Ports.In.Invoice.Dto;

public record AddInvoiceDto(
    string Invoicenumber,
    DateTime Payment,
    decimal Amout,
    decimal Tax,
    string Status,
    string Paymentmethod,
    string PaymentGatewayTrxId,
    Guid CustomerSubscriptionId,
    Guid CustomerId,
    Guid BillingAdressId
);
