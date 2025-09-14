using MongoDB.Driver;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public class PartyRepository : IPartyRepository
{
    private readonly MongoContext _context;
    public PartyRepository(MongoContext context) => _context = context;

    public Task<List<Party>> GetAllAsync(CancellationToken token = default)
        => _context.Clients.Find(_ => true).ToListAsync(token);

    public Task<Party?> GetByIdAsync(Guid id, CancellationToken token = default)
        => _context.Clients.Find(p => p.Id == id).FirstOrDefaultAsync(token);

    public Task AddAsync(Party party, CancellationToken token = default)
        => _context.Clients.InsertOneAsync(party, cancellationToken: token);

    public Task UpdateAsync(Party party, CancellationToken token = default)
        => _context.Clients.ReplaceOneAsync(p => p.Id == party.Id, party, cancellationToken: token);

    public Task DeleteAsync(Guid id, CancellationToken token = default)
        => _context.Clients.DeleteOneAsync(p => p.Id == id, token);
}
