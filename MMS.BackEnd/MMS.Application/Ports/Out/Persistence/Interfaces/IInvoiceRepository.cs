namespace MMS.Application.Ports.Out.Persistence.Interfaces;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetListAsync(
            PageParameters pageParameters,
            Expression<Func<Invoice, bool>> pageFilterExpression,
            List<Expression<Func<Invoice, bool>>>? documentFilterExpression = null,
            Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>? order = null
        );

    Task<Either<RepositoryError, Invoice>> GetAsync(Guid invoiceId);
    Task<Either<RepositoryError, Invoice>> DeleteAsync(Guid invoiceId);
    Task<Either<RepositoryError, Invoice>> AddAsync(Invoice invoice);
    Task<Either<RepositoryError, Invoice>> UpdateAsync(Guid invoiceId, Invoice invoice);
}
