using task_manager_server.Services;
using task_manager_server.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TasksFileService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AngularClient");

app.MapGet("/tasks", async (TasksFileService tasksFileService) =>
{
    var tasks = await tasksFileService.GetTasksAsync();
    return Results.Ok(tasks);
});

app.MapPost("/tasks", async (SaveTaskRequest request, TasksFileService tasksFileService) =>
{
    var validationError = ValidateTaskRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError }); 
    }
    var tasks = await tasksFileService.GetTasksAsync();
    var newTask = new TaskItem
    {
        Id = tasks.Count == 0 ? 1 : tasks.Max(task => task.Id) + 1,
        Title = request.Title.Trim(),
        Description = request.Description.Trim()
        Priority = request.Priority,
        DueDate = request.DueDate,
        Status = request.Status
    };
    tasks.Add(newTask);
    await tasksFileService.SaveTasksAsync(tasks);
    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

app.MapPut("/tasks/{id:int}", async (int id, SaveTaskRequest request, TasksFileService tasksFileService) =>
{
    var validationError = ValidateTaskRequest(request); 
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError }); 
    }
    var tasks = await tasksFileService.GetTasksAsync();
    var existingTask = tasks.FirstOrDefault(task => task.Id == id);
    if (existingTask is null)
    {
        return Results.NotFound();
    }
    existingTask.Title = request.Title.Trim();
    existingTask.Description = request.Description.Trim();
    existingTask.Priority = request.Priority;
    existingTask.DueDate = request.DueDate;
    existingTask.Status = request.Status;
    await tasksFileService.SaveTasksAsync(tasks);
    return Results.Ok(existingTask);
});

app.MapDelete("/tasks/{id:int}", async (int id, TasksFileService tasksFileService) =>
{
    var tasks = await tasksFileService.GetTasksAsync();
    var taskToDelete = tasks.FirstOrDefault(task => task.Id == id);
    if (taskToDelete is null)
    {
        return Results.NotFound();
    }
    tasks.Remove(taskToDelete);
    await tasksFileService.SaveTasksAsync(tasks);
    return Results.NoContent();
});

app.Run();

static string? ValidateTaskRequest(SaveTaskRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return "Title is required.";
    }
    if (string.IsNullOrWhiteSpace(request.DueDate))
    {
        return "Due date is required.";
    }
    var allowedPriorities = new[] { "low", "medium", "high" };
    if (!allowedPriorities.Contains(request.Priority))
    {
        return "Priority must be low, medium, or high.";
    }
    var allowedStatuses = new[] { "pending", "in-progress", "completed" };
    if (!allowedStatuses.Contains(request.Status))
    {
        return "Status must be pending, in-progress, or completed.";
    }
    return null;
}