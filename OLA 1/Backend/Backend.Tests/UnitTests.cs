using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UnitTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskModelController _controller;

    public UnitTests()
    {
        _context = TestHelper.GetInMemoryDbContext();
        _controller = new TaskModelController(_context);
    }

    

    // Unit test 1: CreateTask
    [Fact]
    public async Task CreateTask_AddsTaskAndReturnsCreatedResponse()
    {
        var newTask = new TaskModel { TaskID = 1, Title = "New Task", Deadline = DateTime.UtcNow };

        var result = await _controller.CreateTask(newTask);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var task = Assert.IsType<TaskModel>(createdAtActionResult.Value);
        Assert.Equal("New Task", task.Title);
    }

    // Unit test 2: GetTask
    [Fact]
    public async Task GetTask_ReturnsCorrectTask_WhenIdIsValid()
    {
        _context.TaskModel.Add(new TaskModel { TaskID = 1, Title = "Test Task" });
        await _context.SaveChangesAsync();

        var result = await _controller.GetTask(1);

        var actionResult = Assert.IsType<ActionResult<TaskModel>>(result);
        var task = Assert.IsType<TaskModel>(actionResult.Value);
        Assert.Equal("Test Task", task.Title);
    }

    // Unit test 3: GetTask
    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        var result = await _controller.GetTask(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // Unit test 4: UpdateTask
    [Fact]
    public async Task UpdateTask_UpdatesTaskAndReturnsNoContent()
    {

        var task = new TaskModel { TaskID = 1, Title = "Original Title" };
        _context.TaskModel.Add(task);
        await _context.SaveChangesAsync();

        _context.Entry(task).State = EntityState.Detached;

        var updatedTask = new TaskModel { TaskID = 1, Title = "Updated Title", Deadline = DateTime.UtcNow };

        var result = await _controller.UpdateTask(1, updatedTask);

        Assert.IsType<NoContentResult>(result);

        var updatedEntity = await _context.TaskModel.FindAsync(1);
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Title", updatedEntity?.Title);
    }

    // Unit test 5: DeleteTask
    [Fact]
    public async Task DeleteTask_RemovesTaskAndReturnsNoContent()
    {
        var task = new TaskModel { TaskID = 1, Title = "Task to Delete" };
        _context.TaskModel.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteTask(1);

        Assert.IsType<NoContentResult>(result);

        var deletedTask = await _context.TaskModel.FindAsync(1);
        Assert.Null(deletedTask);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
