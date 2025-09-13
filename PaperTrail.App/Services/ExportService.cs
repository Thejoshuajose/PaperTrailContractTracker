using PaperTrail.Core.DTO;
using PaperTrail.Core.Repositories;

namespace PaperTrail.App.Services;

public class ExportService
{
    private readonly IContractRepository _contracts;
    private readonly CsvExporter _exporter;

    public ExportService(IContractRepository contracts, CsvExporter exporter)
    {
        _contracts = contracts;
        _exporter = exporter;
    }

    public async Task<byte[]?> ExportAsync(FilterOptions filter, CancellationToken token = default)
    {
        var contracts = await _contracts.GetAllAsync(filter, token);
        return _exporter.Export(contracts);
    }
}
