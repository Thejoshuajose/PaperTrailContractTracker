using System.Text.Json;
using MongoDB.Driver;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Services;

/// <summary>
/// Simple JSON based backup and restore facility.  It intentionally only deals
/// with metadata for attachments – the binary files are not copied.
/// </summary>
public class BackupService
{
    private readonly MongoContext _context;

    public BackupService(MongoContext context) => _context = context;

    public async Task BackupAsync(string filePath, CancellationToken token = default)
    {
        var data = new BackupModel
        {
            Parties = await _context.Parties.Find(_ => true).ToListAsync(token),
            ImportedContracts = await _context.ImportedContracts.Find(_ => true).ToListAsync(token),
            PreviousContracts = await _context.PreviousContracts.Find(_ => true).ToListAsync(token),
            Attachments = await _context.Attachments.Find(_ => true).ToListAsync(token),
            Reminders = await _context.Reminders.Find(_ => true).ToListAsync(token)
        };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json, token);
    }

    public async Task RestoreAsync(string filePath, CancellationToken token = default)
    {
        if (!File.Exists(filePath))
            return;
        var json = await File.ReadAllTextAsync(filePath, token);
        var data = JsonSerializer.Deserialize<BackupModel>(json);
        if (data == null)
            return;

        foreach (var party in data.Parties)
            await _context.Parties.ReplaceOneAsync(p => p.Id == party.Id, party, new ReplaceOptions { IsUpsert = true }, token);
        foreach (var contract in data.ImportedContracts)
            await _context.ImportedContracts.ReplaceOneAsync(c => c.Id == contract.Id, contract, new ReplaceOptions { IsUpsert = true }, token);
        foreach (var contract in data.PreviousContracts)
            await _context.PreviousContracts.ReplaceOneAsync(c => c.Id == contract.Id, contract, new ReplaceOptions { IsUpsert = true }, token);
        foreach (var attachment in data.Attachments)
        {
            if (!File.Exists(attachment.FilePath))
                attachment.MissingFile = true;
            await _context.Attachments.ReplaceOneAsync(a => a.Id == attachment.Id, attachment, new ReplaceOptions { IsUpsert = true }, token);
        }
        foreach (var reminder in data.Reminders)
            await _context.Reminders.ReplaceOneAsync(r => r.Id == reminder.Id, reminder, new ReplaceOptions { IsUpsert = true }, token);
    }

    private class BackupModel
    {
        public List<Party> Parties { get; set; } = new();
        public List<Contract> ImportedContracts { get; set; } = new();
        public List<Contract> PreviousContracts { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();
        public List<Reminder> Reminders { get; set; } = new();
    }
}
