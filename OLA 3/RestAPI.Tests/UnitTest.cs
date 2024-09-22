using RestAPI.Controllers;
using RestAPI.Data;
using RestAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;

public class UnitTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TaskController _controller;

    public UnitTests()
    {
        _context = GetInMemoryDbContext();
        _controller = new TaskController(_context);
    }

    public static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // Unit test 1: CreateTask
    [Fact]
    public async Task CreateTask_AddsTaskAndReturnsCreatedResponse()
    {
        var newTask = new TaskModel { TaskID = 1, Title = "New Task", Deadline = DateTime.UtcNow };

        var result = await _controller.CreateTask(newTask);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<TaskResponse>(actionResult.Value);
        Assert.Equal("Task was successfully created.", returnValue.Message);
        Assert.Equal(newTask, returnValue.Task);
    }

    // Unit test 2: GetTask
    [Fact]
    public async Task GetTask_ReturnsTaskAndSuccessMessage()
    {
        var taskId = 1;
        var expectedTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Test Task"
        };
        _context.Tasks.Add(expectedTask);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTask(taskId);

        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<TaskResponse>(actionResult.Value);

        Assert.Equal($"Task with ID {taskId} was successfully retrieved.", returnValue.Message);

        var taskFromDb = await _context.Tasks.FindAsync(taskId);
        Assert.Equal(taskFromDb, returnValue.Task);
    }

    // Unit test 3: GetTasks
    [Fact]
    public async Task GetTasks_ReturnsTasksAndSuccessMessage()
    {
        var tasks = new List<TaskModel>
    {
        new TaskModel { TaskID = 1, Title = "Task 1", Deadline = DateTime.UtcNow.AddDays(1) },
        new TaskModel { TaskID = 2, Title = "Task 2", Deadline = DateTime.UtcNow.AddDays(2) }
    };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTasks();

        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<TasksResponse>(actionResult.Value);

        Assert.Equal("Tasks retrieved successfully.", returnValue.Message);
        Assert.Equal(tasks.Count, returnValue.Tasks?.Count());

        foreach (var task in tasks)
        {
            var returnedTask = returnValue.Tasks?.FirstOrDefault(t => t.TaskID == task.TaskID);
            Assert.NotNull(returnedTask);
            Assert.Equal(task.Title, returnedTask.Title);
            Assert.Equal(task.Deadline, returnedTask.Deadline);
        }
    }


    // Unit test 4: UpdateTask
    [Fact]
    public async Task UpdateTask_UpdatesTaskAndReturnsSuccessMessage()
    {
        var taskId = 1;
        var originalTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Test Title",
            Description = "Test Description",
            Category = "Test Category",
            Deadline = DateTime.UtcNow,
            Completed = false
        };
        _context.Tasks.Add(originalTask);
        await _context.SaveChangesAsync();

        _context.Entry(originalTask).State = EntityState.Detached;

        var updatedTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Updated Title",
            Description = "Test Description",
            Category = "Test Category",
            Deadline = DateTime.UtcNow,
            Completed = false
        };

        var result = await _controller.UpdateTask(taskId, updatedTask);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<TaskResponse>(actionResult.Value);

        Assert.Equal($"Task with ID {taskId} was successfully updated.", returnValue.Message);
        Assert.Equal(updatedTask.Title, returnValue.Task?.Title);
        Assert.Equal(updatedTask.Description, returnValue.Task?.Description);
        Assert.Equal(updatedTask.Category, returnValue.Task?.Category);
        Assert.Equal(updatedTask.Deadline, returnValue.Task?.Deadline);
        Assert.Equal(updatedTask.Completed, returnValue.Task?.Completed);

        var updatedEntity = await _context.Tasks.FindAsync(taskId);
        Assert.NotNull(updatedEntity);
        Assert.Equal(updatedTask.Title, updatedEntity.Title);
        Assert.Equal(updatedTask.Description, updatedEntity.Description);
        Assert.Equal(updatedTask.Category, updatedEntity.Category);
        Assert.Equal(updatedTask.Deadline, updatedEntity.Deadline);
        Assert.Equal(updatedTask.Completed, updatedEntity.Completed);
    }

    // Unit test 5: DeleteTask
    [Fact]
    public async Task DeleteTask_RemovesTaskAndReturnsSuccessMessage()
    {
        var taskId = 1;
        var taskToDelete = new TaskModel
        {
            TaskID = taskId,
            Title = "Task to Delete"
        };
        _context.Tasks.Add(taskToDelete);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteTask(taskId);

        var actionResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<TaskResponse>(actionResult.Value);

        Assert.Equal($"Task with ID {taskId} was successfully deleted.", returnValue.Message);
        Assert.Equal(taskToDelete, returnValue.Task);

        var deletedTask = await _context.Tasks.FindAsync(taskId);
        Assert.Null(deletedTask);
    }

    //Error testing
    [Fact]
    public async Task UpdateTask_TaskNotFound_ReturnsNotFound()
    {
        var taskId = 999;
        var updatedTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Updated Task"
        };

        var result = await _controller.UpdateTask(taskId, updatedTask);

        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = actionResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Contains("not found", response?.ToString().ToLower());
    }

    [Fact]
    public async Task DeleteTask_TaskNotFound_ReturnsNotFound()
    {
        var taskId = 999;

        var result = await _controller.DeleteTask(taskId);

        var actionResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = actionResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Contains("not found", response?.ToString().ToLower());
    }

    [Fact]
    public async Task GetTasks_ReturnsNotFound_WhenNoTasksExist()
    {
        var result = await _controller.GetTasks();

        var actionResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = actionResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Contains("no tasks found", response?.ToString().ToLower());
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        var result = await _controller.GetTask(999);

        var actionResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = actionResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Contains("not found", response?.ToString().ToLower());
    }

    [Fact]
    public async Task UpdateTask_IdMismatch_ReturnsBadRequest()
    {
        var taskId = 1;
        var taskToUpdate = new TaskModel { TaskID = 2, Title = "New Title" };

        var result = await _controller.UpdateTask(taskId, taskToUpdate);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTask_DbUpdateException_ReturnsStatusCode500()
    {
        var taskId = 1;
        var taskToDelete = new TaskModel { TaskID = taskId, Title = "Task to Delete" };
        _context.Tasks.Add(taskToDelete);
        await _context.SaveChangesAsync();

        _context.Database.EnsureDeleted();

        var result = await _controller.DeleteTask(taskId);

        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(500, objectResult?.StatusCode);
    }
}