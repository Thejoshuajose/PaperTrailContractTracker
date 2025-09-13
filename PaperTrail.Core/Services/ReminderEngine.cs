using Microsoft.EntityFrameworkCore;
using Quartz;
using PaperTrail.Core.Data;

namespace PaperTrail.Core.Services;

public class ReminderEngine : IJob
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notification;

    public ReminderEngine(AppDbContext db, INotificationService notification)
    {
        _db = db;
        _notification = notification;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var due = await _db.Reminders.Include(r => r.Contract)
            .Where(r => r.CompletedUtc == null && r.DueUtc <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var reminder in due)
        {
            await _notification.ShowAsync("Contract Reminder", $"{reminder.Contract.Title} - {reminder.Type}");
            reminder.CompletedUtc = DateTime.UtcNow;
        }

        if (due.Count > 0)
            await _db.SaveChangesAsync();
    }
}
