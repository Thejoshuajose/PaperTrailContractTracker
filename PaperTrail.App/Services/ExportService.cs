using PaperTrail.Core.DTO;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;

namespace PaperTrail.App.Services;

public class ExportService
{
    private readonly IContractRepository _contracts;
    private readonly CsvExporter _exporter;
    private readonly ILicenseService _license;

    public ExportService(IContractRepository contracts, CsvExporter exporter, ILicenseService license)
    {
        _contracts = contracts;
        _exporter = exporter;
        _license = license;
    }

    public async Task<byte[]?> ExportAsync(FilterOptions filter, CancellationToken token = default)
    {
        if (!_license.IsPro)
            return null;
        var contracts = await _contracts.GetAllAsync(filter, token);
        return _exporter.Export(contracts);
    }
}
