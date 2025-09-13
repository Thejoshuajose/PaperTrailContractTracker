using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using Xunit;

namespace PaperTrail.Tests;

public class AttachmentDedupeTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Importing_Same_File_Twice_Dedupes()
    {
        using var ctx = CreateContext();
        var repo = new ContractRepository(ctx);
        var hashSvc = new HashService();
        var contract = new Contract { Id = Guid.NewGuid(), Title = "Test" };
        await repo.AddAsync(contract);

        var bytes = new byte[] { 1, 2, 3, 4 };
        var hash = hashSvc.ComputeHash(bytes);
        var attachment1 = new Attachment
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            FileName = "a.bin",
            FilePath = "a.bin",
            Hash = hash,
            CreatedUtc = DateTime.UtcNow
        };
        await repo.AddAttachmentAsync(contract.Id, attachment1);

        var attachment2 = new Attachment
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            FileName = "b.bin",
            FilePath = "b.bin",
            Hash = hash,
            CreatedUtc = DateTime.UtcNow
        };
        await repo.AddAttachmentAsync(contract.Id, attachment2);

        (await ctx.Attachments.CountAsync()).Should().Be(1);
    }
}
