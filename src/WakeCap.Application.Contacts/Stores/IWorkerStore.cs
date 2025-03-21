using WakeCap.Domain;

namespace WakeCap.Application.Contacts.Stores;

public interface IWorkersStore
{
    Task<Dictionary<string, Worker>> List(IEnumerable<string> codes, CancellationToken cancellationToken);
}