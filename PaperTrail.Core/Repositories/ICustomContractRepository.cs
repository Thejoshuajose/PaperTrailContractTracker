using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public interface ICustomContractRepository
{
    Task<List<CustomContract>> GetAllAsync(CancellationToken token = default);
    Task AddAsync(CustomContract contract, CancellationToken token = default);
    Task DeleteAsync(Guid id, CancellationToken token = default);
}
