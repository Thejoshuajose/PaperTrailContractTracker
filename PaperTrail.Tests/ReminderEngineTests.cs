using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using PaperTrail.Core.Services;
using Quartz;
using Xunit;

namespace PaperTrail.Tests;

public class ReminderEngineTests
{
    private class TestNotification : INotificationService
    {
        public int Count { get; private set; }
        public Task ShowAsync(string title, string message)
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private class Factory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;
        public Factory(DbContextOptions<AppDbContext> options) => _options = options;
        public AppDbContext CreateDbContext() => new AppDbContext(_options);
    }

    [Fact]
    public async Task Due_Reminders_Fire_Once()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var ctx = new AppDbContext(options);
        var contract = new Contract { Id = Guid.NewGuid(), Title = "Test" };
        ctx.Contracts.Add(contract);
        ctx.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            Contract = contract,
            DueUtc = DateTime.UtcNow.AddMinutes(-5)
        });
        await ctx.SaveChangesAsync();

        var factory = new Factory(options);
        var notify = new TestNotification();
        var engine = new ReminderEngine(factory, notify, NullLogger<ReminderEngine>.Instance);

        await engine.Execute(null!);
        notify.Count.Should().Be(1);

        await engine.Execute(null!);
        notify.Count.Should().Be(1); // no additional notifications

        var reminder = await ctx.Reminders.FirstAsync();
        reminder.CompletedUtc.Should().NotBeNull();
    }
}
