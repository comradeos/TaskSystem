using TaskSystem.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Domain.Interfaces;

public interface ICommentRepository
{
    Task<int> CreateAsync(Comment comment);

    Task<IEnumerable<Comment>> GetByTaskAsync(int taskId);
}