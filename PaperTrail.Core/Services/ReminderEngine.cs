using Microsoft.EntityFrameworkCore;
using Quartz;
using PaperTrail.Core.Data;
using Microsoft.Extensions.Logging;

namespace PaperTrail.Core.Services;

public class ReminderEngine : IJob
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly INotificationService _notification;
    private readonly ILogger<ReminderEngine> _logger;

    public ReminderEngine(IDbContextFactory<AppDbContext> dbFactory, INotificationService notification, ILogger<ReminderEngine> logger)
    {
        _dbFactory = dbFactory;
        _notification = notification;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var db = _dbFactory.CreateDbContext();
        var due = await db.Reminders.Include(r => r.Contract)
            .Where(r => r.CompletedUtc == null && r.DueUtc <= DateTime.UtcNow)
            .ToListAsync();

        _logger.LogInformation("Processing {Count} reminders", due.Count);

        foreach (var reminder in due)
        {
            await _notification.ShowAsync("Contract Reminder", $"{reminder.Contract.Title} - {reminder.Type}");
            reminder.CompletedUtc = DateTime.UtcNow;
        }

        if (due.Count > 0)
            await db.SaveChangesAsync();
    }
}
