using Xunit;
using System.Linq;
using FluentAssertions;
using PaperTrail.Core.Models;
using PaperTrail.Core.Services;

namespace PaperTrail.Tests;

public class ReminderFactoryTests
{
    [Fact]
    public void CreatesExpectedReminders()
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            RenewalDate = new DateOnly(2024, 1, 31),
            NoticePeriodDays = 10,
            TerminationDate = new DateOnly(2024, 2, 29)
        };

        var reminders = ReminderFactory.Create(contract).ToList();

        reminders.Should().HaveCount(3);
        reminders.Any(r => r.Type == ReminderType.Renewal).Should().BeTrue();
        reminders.Any(r => r.Type == ReminderType.Notice).Should().BeTrue();
        reminders.Any(r => r.Type == ReminderType.Termination).Should().BeTrue();
    }
}
