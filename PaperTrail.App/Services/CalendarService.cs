using System.Diagnostics;
using System.IO;
using System.Text;
using PaperTrail.Core.Models;

namespace PaperTrail.App.Services;

public class CalendarService
{
    public void AddToCalendar(Reminder reminder, string title)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"SUMMARY:{title}");
        if (!string.IsNullOrWhiteSpace(reminder.Note))
            sb.AppendLine($"DESCRIPTION:{reminder.Note}");
        sb.AppendLine($"DTSTART:{reminder.DueUtc:yyyyMMdd'T'HHmmss'Z'}");
        sb.AppendLine($"DTEND:{reminder.DueUtc:yyyyMMdd'T'HHmmss'Z'}");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");

        var path = Path.Combine(Path.GetTempPath(), $"papertrail-reminder-{Guid.NewGuid():N}.ics");
        File.WriteAllText(path, sb.ToString());
        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
