using MMS.Application.Ports.In.Invoice.Dto;

namespace MMS.Application.Mappers;

public class InvoiceMapper : Profile
{
    public InvoiceMapper()
    {
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<AddInvoiceDto, Invoice>();
        CreateMap<UpdateInvoiceDto, Invoice>();
    }
}
