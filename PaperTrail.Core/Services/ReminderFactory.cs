using PaperTrail.Core.Models;

namespace PaperTrail.Core.Services;

public static class ReminderFactory
{
    public static IEnumerable<Reminder> Create(Contract contract)
    {
        var list = new List<Reminder>();
        if (contract.RenewalDate.HasValue)
        {
            var renewalLocal = contract.RenewalDate.Value.ToDateTime(new TimeOnly(9,0));
            list.Add(new Reminder
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                Type = ReminderType.Renewal,
                DueUtc = renewalLocal.ToUniversalTime(),
                CreatedUtc = DateTime.UtcNow
            });
            if (contract.NoticePeriodDays.HasValue)
            {
                var noticeLocal = renewalLocal.AddDays(-contract.NoticePeriodDays.Value);
                list.Add(new Reminder
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    Type = ReminderType.Notice,
                    DueUtc = noticeLocal.ToUniversalTime(),
                    CreatedUtc = DateTime.UtcNow
                });
            }
        }
        if (contract.TerminationDate.HasValue)
        {
            var termLocal = contract.TerminationDate.Value.ToDateTime(new TimeOnly(9,0));
            list.Add(new Reminder
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                Type = ReminderType.Termination,
                DueUtc = termLocal.ToUniversalTime(),
                CreatedUtc = DateTime.UtcNow
            });
        }
        return list;
    }
}
