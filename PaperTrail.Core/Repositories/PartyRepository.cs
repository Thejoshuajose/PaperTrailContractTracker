using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public class PartyRepository : IPartyRepository
{
    private readonly AppDbContext _db;
    public PartyRepository(AppDbContext db) => _db = db;

    public async Task<List<Party>> GetAllAsync(CancellationToken token = default)
        => await _db.Parties.AsNoTracking().ToListAsync(token);

    public Task<Party?> GetByIdAsync(Guid id, CancellationToken token = default)
        => _db.Parties.FirstOrDefaultAsync(p => p.Id == id, token);

    public async Task AddAsync(Party party, CancellationToken token = default)
    {
        _db.Parties.Add(party);
        await _db.SaveChangesAsync(token);
    }

    public async Task UpdateAsync(Party party, CancellationToken token = default)
    {
        _db.Parties.Update(party);
        await _db.SaveChangesAsync(token);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _db.Parties.FindAsync(new object?[] { id }, cancellationToken: token);
        if (entity != null)
        {
            _db.Parties.Remove(entity);
            await _db.SaveChangesAsync(token);
        }
    }
}
