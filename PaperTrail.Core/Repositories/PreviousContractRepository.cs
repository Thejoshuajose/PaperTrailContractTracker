using MongoDB.Bson;
using MongoDB.Driver;
using PaperTrail.Core.Data;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using System;
using System.Linq;

namespace PaperTrail.Core.Repositories;

/// <summary>
/// Repository for working with previously created contracts stored in the
/// <c>PreviousContracts</c> collection.
/// </summary>
public class PreviousContractRepository : IContractRepository
{
    private readonly MongoContext _context;

    public PreviousContractRepository(MongoContext context) => _context = context;

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

        var list = await _context.PreviousContracts
            .Find(filter)
            .SortByDescending(c => c.UpdatedUtc)
            .Limit(30)
            .ToListAsync(token);

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
        var contract = await _context.PreviousContracts.Find(c => c.Id == id).FirstOrDefaultAsync(token);
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
        => _context.PreviousContracts.InsertOneAsync(contract, cancellationToken: token);

    public Task UpdateAsync(Contract contract, CancellationToken token = default)
        => _context.PreviousContracts.ReplaceOneAsync(c => c.Id == contract.Id, contract, cancellationToken: token);

    public Task DeleteAsync(Guid id, CancellationToken token = default)
        => _context.PreviousContracts.DeleteOneAsync(c => c.Id == id, token);

    public async Task AddOrUpdateAsync(Contract contract, CancellationToken token = default)
    {
        contract.UpdatedUtc = DateTime.UtcNow;
        await _context.PreviousContracts.ReplaceOneAsync(c => c.Id == contract.Id, contract,
            new ReplaceOptions { IsUpsert = true }, token);

        var excess = await _context.PreviousContracts.Find(_ => true)
            .SortByDescending(c => c.UpdatedUtc)
            .Skip(30)
            .Project(c => c.Id)
            .ToListAsync(token);

        if (excess.Any())
        {
            var filter = Builders<Contract>.Filter.In(c => c.Id, excess);
            await _context.PreviousContracts.DeleteManyAsync(filter, token);
        }
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
        // Replace any existing reminders for the contract with the provided ones.
        var reminderList = reminders.ToList();

        await _context.Reminders.DeleteManyAsync(r => r.ContractId == contractId, token);

        foreach (var r in reminderList)
            r.ContractId = contractId;

        if (reminderList.Any())
            await _context.Reminders.InsertManyAsync(reminderList, cancellationToken: token);
    }
}

