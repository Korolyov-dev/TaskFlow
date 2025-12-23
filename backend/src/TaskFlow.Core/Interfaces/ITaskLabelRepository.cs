using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces;

public interface ITaskLabelRepository : IRepository<TaskLabel>
{
    // Специфичные методы для связи многие-ко-многим
    Task<TaskLabel?> GetByTaskAndLabelAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskLabel>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskLabel>> GetByLabelAsync(Guid labelId, CancellationToken cancellationToken = default);
    Task RemoveByTaskAndLabelAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default);
    Task RemoveAllForTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task RemoveAllForLabelAsync(Guid labelId, CancellationToken cancellationToken = default);

    // Проверки
    Task<bool> ExistsAsync(Guid taskId, Guid labelId, CancellationToken cancellationToken = default);
    Task<int> CountLabelsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<int> CountTasksForLabelAsync(Guid labelId, CancellationToken cancellationToken = default);

    // Пакетные операции
    Task AddMultipleAsync(Guid taskId, IEnumerable<Guid> labelIds, CancellationToken cancellationToken = default);
    Task RemoveMultipleAsync(Guid taskId, IEnumerable<Guid> labelIds, CancellationToken cancellationToken = default);
    Task ReplaceTaskLabelsAsync(Guid taskId, IEnumerable<Guid> newLabelIds, CancellationToken cancellationToken = default);
}
