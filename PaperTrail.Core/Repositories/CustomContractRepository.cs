using MongoDB.Driver;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public class CustomContractRepository : ICustomContractRepository
{
    private readonly MongoContext _context;
    public CustomContractRepository(MongoContext context) => _context = context;

    public Task<List<CustomContract>> GetAllAsync(CancellationToken token = default)
        => _context.CustomContracts.Find(_ => true).SortBy(c => c.Title).ToListAsync(token);

    public Task AddAsync(CustomContract contract, CancellationToken token = default)
        => _context.CustomContracts.InsertOneAsync(contract, cancellationToken: token);

    public Task DeleteAsync(Guid id, CancellationToken token = default)
        => _context.CustomContracts.DeleteOneAsync(c => c.Id == id, token);
}
