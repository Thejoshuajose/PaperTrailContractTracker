using MongoDB.Driver;
using MongoDB.Bson;
using PaperTrail.Core.Data;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using System.Linq;

namespace PaperTrail.Core.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly MongoContext _context;

    public ContractRepository(MongoContext context) => _context = context;

    public async Task<List<Contract>> GetAllAsync(FilterOptions options, CancellationToken token = default)
    {
        var filter = Builders<Contract>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var regex = new BsonRegularExpression(options.SearchText, "i");
            filter &= Builders<Contract>.Filter.Or(
                Builders<Contract>.Filter.Regex(c => c.Title, regex),
                Builders<Contract>.Filter.Regex(c => c.Tags, regex)
            );
        }

        if (options.Statuses != null && options.Statuses.Length > 0)
            filter &= Builders<Contract>.Filter.In(c => c.Status, options.Statuses);

        if (options.RenewalFrom.HasValue)
            filter &= Builders<Contract>.Filter.Gte(c => c.RenewalDate, options.RenewalFrom);
        if (options.RenewalTo.HasValue)
            filter &= Builders<Contract>.Filter.Lte(c => c.RenewalDate, options.RenewalTo);

        var list = await _context.ImportedContracts.Find(filter).ToListAsync(token);

        var partyIds = list
            .Where(c => c.CounterpartyId.HasValue)
            .Select(c => c.CounterpartyId!.Value)
            .Distinct()
            .ToList();

        if (partyIds.Count > 0)
        {
            var parties = await _context.Clients
                .Find(p => partyIds.Contains(p.Id))
                .ToListAsync(token);
            var dict = parties.ToDictionary(p => p.Id);
            foreach (var c in list)
                if (c.CounterpartyId.HasValue && dict.TryGetValue(c.CounterpartyId.Value, out var party))
                    c.Counterparty = party;
        }

        return list;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var contract = await _context.ImportedContracts.Find(c => c.Id == id).FirstOrDefaultAsync(token);
        if (contract != null)
        {
            contract.Attachments = await _context.Attachments.Find(a => a.ContractId == id).ToListAsync(token);
            contract.Reminders = await _context.Reminders.Find(r => r.ContractId == id).ToListAsync(token);

            if (contract.CounterpartyId.HasValue)
                contract.Counterparty = await _context.Clients
                    .Find(p => p.Id == contract.CounterpartyId.Value)
                    .FirstOrDefaultAsync(token);
        }
        return contract;
    }

    public Task AddAsync(Contract contract, CancellationToken token = default)
        => _context.ImportedContracts.InsertOneAsync(contract, cancellationToken: token);

    public Task UpdateAsync(Contract contract, CancellationToken token = default)
        => _context.ImportedContracts.ReplaceOneAsync(c => c.Id == contract.Id, contract, cancellationToken: token);

    public async Task DeleteAsync(Guid id, CancellationToken token = default)
    {
        await _context.ImportedContracts.DeleteOneAsync(c => c.Id == id, token);
        await _context.PreviousContracts.DeleteOneAsync(c => c.Id == id, token);
    }

    public async Task AddAttachmentAsync(Guid contractId, Attachment attachment, CancellationToken token = default)
    {
        if (await AttachmentExistsAsync(contractId, attachment.Hash!, token))
            return;
        attachment.ContractId = contractId;
        await _context.Attachments.InsertOneAsync(attachment, cancellationToken: token);
    }

    public Task<bool> AttachmentExistsAsync(Guid contractId, string hash, CancellationToken token = default)
        => _context.Attachments.Find(a => a.ContractId == contractId && a.Hash == hash).AnyAsync(token);

    public async Task AddRemindersAsync(Guid contractId, IEnumerable<Reminder> reminders, CancellationToken token = default)
    {
        // Replace existing reminders for the contract and ensure no duplicate IDs are inserted.
        var reminderList = reminders
            .Select(r =>
            {
                if (r.Id == Guid.Empty)
                    r.Id = Guid.NewGuid();
                r.ContractId = contractId;
                return r;
            })
            .GroupBy(r => r.Id)
            .Select(g => g.First())
            .ToList();

        await _context.Reminders.DeleteManyAsync(r => r.ContractId == contractId, token);

        if (reminderList.Count > 0)
            await _context.Reminders.InsertManyAsync(reminderList, cancellationToken: token);
    }
}
