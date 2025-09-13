using Microsoft.EntityFrameworkCore;
using Quartz;
using PaperTrail.Core.Data;

namespace PaperTrail.Core.Services;

public class ReminderEngine : IJob
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly INotificationService _notification;

    public ReminderEngine(IDbContextFactory<AppDbContext> dbFactory, INotificationService notification)
    {
        _dbFactory = dbFactory;
        _notification = notification;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var db = _dbFactory.CreateDbContext();
        var due = await db.Reminders.Include(r => r.Contract)
            .Where(r => r.CompletedUtc == null && r.DueUtc <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var reminder in due)
        {
            await _notification.ShowAsync("Contract Reminder", $"{reminder.Contract.Title} - {reminder.Type}");
            reminder.CompletedUtc = DateTime.UtcNow;
        }

        if (due.Count > 0)
            await db.SaveChangesAsync();
    }
}
