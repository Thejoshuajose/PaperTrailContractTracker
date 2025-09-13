using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using System.Linq;

namespace PaperTrail.Core.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly AppDbContext _db;

    public ContractRepository(AppDbContext db) => _db = db;

    public async Task<List<Contract>> GetAllAsync(FilterOptions options, CancellationToken token = default)
    {
        var query = _db.Contracts
            .AsNoTracking()
            .Include(c => c.Counterparty)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var pattern = $"%{options.SearchText}%";
            query = query.Where(c =>
                EF.Functions.Like(c.Title, pattern) ||
                (c.Counterparty != null && EF.Functions.Like(c.Counterparty.Name, pattern)) ||
                (c.Tags != null && EF.Functions.Like(c.Tags, pattern)));
        }

        if (options.Statuses != null && options.Statuses.Length > 0)
            query = query.Where(c => options.Statuses.Contains(c.Status));

        var tags = options.NormalizeTags().ToArray();
        if (tags.Length > 0)
            query = query.Where(c => c.Tags != null && tags.All(t => c.Tags.ToLower().Split(',', ';', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLowerInvariant()).Contains(t)));

        if (options.RenewalFrom.HasValue)
            query = query.Where(c => c.RenewalDate >= options.RenewalFrom);
        if (options.RenewalTo.HasValue)
            query = query.Where(c => c.RenewalDate <= options.RenewalTo);

        return await query.ToListAsync(token);
    }

    public Task<Contract?> GetByIdAsync(Guid id, CancellationToken token = default)
        => _db.Contracts.Include(c => c.Attachments).Include(c => c.Reminders).FirstOrDefaultAsync(c => c.Id == id, token);

    public async Task AddAsync(Contract contract, CancellationToken token = default)
    {
        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(token);
    }

    public async Task UpdateAsync(Contract contract, CancellationToken token = default)
    {
        _db.Contracts.Update(contract);
        await _db.SaveChangesAsync(token);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _db.Contracts.FindAsync(new object?[] { id }, cancellationToken: token);
        if (entity != null)
        {
            _db.Contracts.Remove(entity);
            await _db.SaveChangesAsync(token);
        }
    }

    public async Task AddAttachmentAsync(Guid contractId, Attachment attachment, CancellationToken token = default)
    {
        if (await AttachmentExistsAsync(contractId, attachment.Hash!, token))
            return;
        var contract = await _db.Contracts.Include(c => c.Attachments).FirstAsync(c => c.Id == contractId, token);
        contract.Attachments.Add(attachment);
        await _db.SaveChangesAsync(token);
    }

    public Task<bool> AttachmentExistsAsync(Guid contractId, string hash, CancellationToken token = default)
        => _db.Attachments.AnyAsync(a => a.ContractId == contractId && a.Hash == hash, token);

    public async Task AddRemindersAsync(Guid contractId, IEnumerable<Reminder> reminders, CancellationToken token = default)
    {
        var contract = await _db.Contracts.Include(c => c.Reminders).FirstAsync(c => c.Id == contractId, token);
        foreach (var r in reminders)
            contract.Reminders.Add(r);
        await _db.SaveChangesAsync(token);
    }
}
