using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Services;

/// <summary>
/// Simple JSON based backup and restore facility.  It intentionally only deals
/// with metadata for attachments – the binary files are not copied.
/// </summary>
public class BackupService
{
    private readonly AppDbContext _db;

    public BackupService(AppDbContext db) => _db = db;

    public async Task BackupAsync(string filePath, CancellationToken token = default)
    {
        var data = new BackupModel
        {
            Parties = await _db.Parties.AsNoTracking().ToListAsync(token),
            Contracts = await _db.Contracts.AsNoTracking().ToListAsync(token),
            Attachments = await _db.Attachments.AsNoTracking().ToListAsync(token),
            Reminders = await _db.Reminders.AsNoTracking().ToListAsync(token)
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
            _db.Parties.Update(party);
        foreach (var contract in data.Contracts)
            _db.Contracts.Update(contract);
        foreach (var attachment in data.Attachments)
        {
            if (!File.Exists(attachment.FilePath))
                attachment.MissingFile = true;
            _db.Attachments.Update(attachment);
        }
        foreach (var reminder in data.Reminders)
            _db.Reminders.Update(reminder);

        await _db.SaveChangesAsync(token);
    }

    private class BackupModel
    {
        public List<Party> Parties { get; set; } = new();
        public List<Contract> Contracts { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();
        public List<Reminder> Reminders { get; set; } = new();
    }
}
