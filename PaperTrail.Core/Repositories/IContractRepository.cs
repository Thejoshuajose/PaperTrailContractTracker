using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Repositories;

public interface IContractRepository
{
    Task<List<Contract>> GetAllAsync(FilterOptions options, CancellationToken token = default);
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task AddAsync(Contract contract, CancellationToken token = default);
    Task UpdateAsync(Contract contract, CancellationToken token = default);
    Task DeleteAsync(Guid id, CancellationToken token = default);
    Task AddAttachmentAsync(Guid contractId, Attachment attachment, CancellationToken token = default);
    Task<bool> AttachmentExistsAsync(Guid contractId, string hash, CancellationToken token = default);
    Task AddRemindersAsync(Guid contractId, IEnumerable<Reminder> reminders, CancellationToken token = default);
}
