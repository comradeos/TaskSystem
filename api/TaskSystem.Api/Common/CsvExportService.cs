using System.Text;

namespace TaskSystem.Api.Common;

public interface ICsvExportService
{
    byte[] ExportTasks(IEnumerable<TaskSystem.Domain.Entities.Task> tasks);
}

public class CsvExportService : ICsvExportService
{
    public byte[] ExportTasks(IEnumerable<TaskSystem.Domain.Entities.Task> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Id,Number,Title,Status,Priority,Author,AssignedTo,CreatedAt");

        foreach (var t in tasks)
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
            return "";

        if (value.Contains(",") || value.Contains("\""))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}