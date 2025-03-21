using WakeCap.Domain;

namespace WakeCap.Application.Contacts.Stores;

public interface IZonesStore
{
    Task<Dictionary<string, Zone>> List(IEnumerable<string> codes, CancellationToken cancellationToken = default);
}