using TaskSystem.Api.DTOs;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Helpers;

public static class MapperHelper
{
    public static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            CreatedAt = project.CreatedAt
        };
    }

    public static TaskDto ToDto(TaskSystem.Domain.Entities.Task task)
    {
        return new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            NumberInProject = task.NumberInProject,
            Title = task.Title,
            Description = task.Description,
            Status = (int)task.Status,
            Priority = (int)task.Priority,
            AuthorUserId = task.AuthorUserId,
            AuthorUserName = task.AuthorUserName,
            AssignedUserId = task.AssignedUserId,
            AssignedUserName = task.AssignedUserName,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    public static CommentDto ToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name
        };
    }
    
    public static TimelineEventDto ToDto(TimelineEvent entity)
    {
        return new TimelineEventDto
        {
            EventType = entity.EventType,
            EntityId = entity.EntityId,
            UserId = entity.UserId,
            UserName = entity.UserName,
            Data = entity.Data,
            CreatedAt = entity.CreatedAt
        };
    }
}