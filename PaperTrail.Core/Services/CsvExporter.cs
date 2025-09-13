using System.Linq;
using CsvHelper;
using System.Globalization;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Services;

public class CsvExporter
{
    public byte[] Export(IEnumerable<Contract> contracts)
    {
        using var memory = new MemoryStream();
        using var writer = new StreamWriter(memory);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(contracts.Select(c => new
        {
            c.Title,
            Counterparty = c.Counterparty?.Name,
            c.Status,
            c.RenewalDate,
            c.Tags
        }));
        writer.Flush();
        return memory.ToArray();
    }
}
