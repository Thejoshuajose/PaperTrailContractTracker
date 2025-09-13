using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using System.IO;

namespace PaperTrail.App.Services;

public class ImportService
{
    private readonly IContractRepository _contracts;
    private readonly HashService _hash;

    public ImportService(IContractRepository contracts, HashService hash)
    {
        _contracts = contracts;
        _hash = hash;
    }

    public async Task<Attachment?> ImportAsync(Guid contractId, string filePath, CancellationToken token = default)
    {
        if (!File.Exists(filePath))
            return null;

        await using var src = File.OpenRead(filePath);
        var hash = _hash.ComputeHash(src);

        if (await _contracts.AttachmentExistsAsync(contractId, hash, token))
            return null;

        src.Position = 0;

        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PaperTrailContractTracker", "files");
        Directory.CreateDirectory(baseDir);
        var fileName = Path.GetFileName(filePath);
        var destName = $"{Guid.NewGuid()}_{fileName}";
        var destPath = Path.Combine(baseDir, destName);
        await using (var dest = File.Create(destPath))
        {
            await src.CopyToAsync(dest, token);
        }

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            FileName = fileName,
            FilePath = destPath,
            Hash = hash,
            CreatedUtc = DateTime.UtcNow
        };
        await _contracts.AddAttachmentAsync(contractId, attachment, token);
        return attachment;
    }
}
