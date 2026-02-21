namespace TaskSystem.Application.Projects;

public interface ICreateProject
{
    Task<int> ExecuteAsync(string name, CancellationToken ct = default);
}