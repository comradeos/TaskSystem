namespace TaskSystem.Domain.Entities;

public class Project
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Project(
        int id,
        string name,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }
}