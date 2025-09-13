using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public interface IPartyRepository
{
    Task<List<Party>> GetAllAsync(CancellationToken token = default);
    Task<Party?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task AddAsync(Party party, CancellationToken token = default);
    Task UpdateAsync(Party party, CancellationToken token = default);
    Task DeleteAsync(Guid id, CancellationToken token = default);
}
