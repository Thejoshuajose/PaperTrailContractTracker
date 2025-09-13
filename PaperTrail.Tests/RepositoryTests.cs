using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using Xunit;

namespace PaperTrail.Tests;

public class RepositoryTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ContractCrud()
    {
        using var ctx = CreateContext();
        var repo = new ContractRepository(ctx);
        var contract = new Contract { Id = Guid.NewGuid(), Title = "Test" };
        await repo.AddAsync(contract);
        (await repo.GetAllAsync(new PaperTrail.Core.DTO.FilterOptions())).Should().ContainSingle();
        contract.Title = "Updated";
        await repo.UpdateAsync(contract);
        (await repo.GetByIdAsync(contract.Id))!.Title.Should().Be("Updated");
        await repo.DeleteAsync(contract.Id);
        (await repo.GetAllAsync(new PaperTrail.Core.DTO.FilterOptions())).Should().BeEmpty();
    }
}
