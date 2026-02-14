using Alert = MMS.Application.Models.NoSQL.Alert;

namespace MMS.Application.Ports.Out.Persistence.Interfaces.NoSql;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken cancellationToken = default);
}
