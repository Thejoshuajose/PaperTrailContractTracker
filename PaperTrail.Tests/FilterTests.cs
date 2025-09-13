using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using Xunit;

namespace PaperTrail.Tests;

public class FilterTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Filters_By_Status_Tag_And_Renewal_Range()
    {
        using var ctx = CreateContext();
        var repo = new ContractRepository(ctx);
        var party = new Party { Id = Guid.NewGuid(), Name = "Foo" };
        ctx.Parties.Add(party);
        ctx.Contracts.AddRange(
            new Contract
            {
                Id = Guid.NewGuid(),
                Title = "A",
                Counterparty = party,
                Status = ContractStatus.Active,
                Tags = "finance,legal",
                RenewalDate = new DateOnly(2024, 6, 1)
            },
            new Contract
            {
                Id = Guid.NewGuid(),
                Title = "B",
                Counterparty = party,
                Status = ContractStatus.Draft,
                Tags = "it",
                RenewalDate = new DateOnly(2024, 8, 1)
            },
            new Contract
            {
                Id = Guid.NewGuid(),
                Title = "C",
                Counterparty = party,
                Status = ContractStatus.Archived,
                Tags = "finance",
                RenewalDate = new DateOnly(2025, 1, 1)
            }
        );
        await ctx.SaveChangesAsync();

        var options = new FilterOptions
        {
            Statuses = new[] { ContractStatus.Active, ContractStatus.Draft },
            Tags = new[] { "finance" },
            RenewalFrom = new DateOnly(2024, 1, 1),
            RenewalTo = new DateOnly(2024, 12, 31)
        };

        var results = await repo.GetAllAsync(options);
        results.Should().ContainSingle();
        results[0].Title.Should().Be("A");
    }
}
