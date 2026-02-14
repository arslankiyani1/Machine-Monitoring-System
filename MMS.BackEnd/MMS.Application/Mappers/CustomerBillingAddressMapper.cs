namespace MMS.Application.Mappers;

public class CustomerBillingAddressMapper :Profile
{
    public CustomerBillingAddressMapper()
    {
        CreateMap<AddCustomerBillingAddressDto, CustomerBillingAddress>();
        CreateMap<UpdateCustomerBillingAddressDto, CustomerBillingAddress>();
        CreateMap<CustomerBillingAddress, CustomerBillingAddressDto>();
    }
}
