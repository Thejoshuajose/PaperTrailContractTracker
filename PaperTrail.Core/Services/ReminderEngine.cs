using MongoDB.Driver;
using Quartz;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using Microsoft.Extensions.Logging;

namespace PaperTrail.Core.Services;

public class ReminderEngine : IJob
{
    private readonly MongoContext _context;
    private readonly INotificationService _notification;
    private readonly ILogger<ReminderEngine> _logger;

    public ReminderEngine(MongoContext context, INotificationService notification, ILogger<ReminderEngine> logger)
    {
        _context = context;
        _notification = notification;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var due = await _context.Reminders.Find(r => r.CompletedUtc == null && r.DueUtc <= DateTime.UtcNow).ToListAsync();

        _logger.LogInformation("Processing {Count} reminders", due.Count);

        foreach (var reminder in due)
        {
            var contract = await _context.ImportedContracts.Find(c => c.Id == reminder.ContractId).FirstOrDefaultAsync();
            if (contract != null)
                await _notification.ShowAsync("Contract Reminder", $"{contract.Title} - {reminder.Type}");
            var update = Builders<Reminder>.Update.Set(r => r.CompletedUtc, DateTime.UtcNow);
            await _context.Reminders.UpdateOneAsync(r => r.Id == reminder.Id, update);
        }
    }
}
