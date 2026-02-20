using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface ICommentRepository
{
    Task AddAsync(Comment comment);
    Task<IReadOnlyList<Comment>> GetByTaskIdAsync(Guid taskId);
}