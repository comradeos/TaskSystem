namespace TaskSystem.Application.Projects;

public interface ICreateProjectUseCase
{
    Task<int> ExecuteAsync(string name, CancellationToken ct = default);
}