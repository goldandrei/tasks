using System.Text.Json;
using task_manager_server.Models;

namespace task_manager_server.Services;

public class TasksFileService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public TasksFileService(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "tasks.json");
    }

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<TaskItem>();
        }

        var json = await File.ReadAllTextAsync(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<TaskItem>();
        }

        return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
    }

    public async Task SaveTasksAsync(List<TaskItem> tasks)
    {
        var json = JsonSerializer.Serialize(tasks, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}