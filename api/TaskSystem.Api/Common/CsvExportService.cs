using System.Text;
using Task = TaskSystem.Domain.Entities.Task;

namespace TaskSystem.Api.Common;

public interface ICsvExportService
{
    byte[] ExportTasks(IEnumerable<Task> tasks);
}

public class CsvExportService : ICsvExportService
{
    public byte[] ExportTasks(IEnumerable<Task> tasks)
    {
        StringBuilder sb = new();

        sb.AppendLine("Id,Number,Title,Status,Priority,Author,AssignedTo,CreatedAt");

        foreach (Task t in tasks)
        {
            sb.AppendLine(string.Join(",",
                t.Id,
                t.NumberInProject,
                Escape(t.Title),
                t.Status,
                t.Priority,
                Escape(t.AuthorUserName),
                Escape(t.AssignedUserName),
                t.CreatedAt));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}