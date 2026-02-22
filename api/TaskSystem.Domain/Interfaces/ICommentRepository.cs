using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface ICommentRepository
{
    Task<int> CreateAsync(Comment comment);

    Task<IEnumerable<Comment>> GetByTaskAsync(int taskId);
}