using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Contracts.AnyAsync())
            return;

        var party = new Party { Id = Guid.NewGuid(), Name = "Contoso Ltd." };
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Title = "Sample Contract",
            Counterparty = party,
            RenewalDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            NoticePeriodDays = 15
        };

        db.Parties.Add(party);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        var reminders = ReminderFactory.Create(contract);
        await db.Reminders.AddRangeAsync(reminders);
        await db.SaveChangesAsync();
    }
}
