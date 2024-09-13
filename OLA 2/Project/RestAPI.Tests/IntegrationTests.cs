using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RestAPI.Data;
using RestAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

public class TaskIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private IServiceScope _scope;

    public TaskIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        await ClearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
        _scope?.Dispose();
    }

    private async Task ClearDatabaseAsync()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private StringContent GetPayload(object obj)
    {
        return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(stringContent);
    }

    // Create Task test for success
    [Fact]
    public async Task CreateTask_ShouldAddTaskSuccessfully()
    {
        var newTask = new TaskModel
        {
            TaskID = 1,
            Title = "New Task",
            Description = "Task Description",
            Category = "Work",
            Deadline = DateTime.UtcNow.AddDays(1),
            Completed = false
        };

        var payload = GetPayload(newTask);

        var response = await _client.PostAsync("/api/tasks", payload);

        response.EnsureSuccessStatusCode();
        var responseBody = await DeserializeResponse<TaskResponse>(response);

        Assert.NotNull(responseBody);
        Assert.Equal("Task was successfully created.", responseBody.Message);
        Assert.Equal(newTask.Title, responseBody.Task.Title);
    }

    // Get Task test for success and error
    [Fact]
    public async Task GetTask_ShouldReturnCorrectTask_WhenIdIsValid()
    {
        var newTask = new TaskModel
        {
            TaskID = 1,
            Title = "New Task",
            Description = "Task Description",
            Category = "Work",
            Deadline = DateTime.UtcNow.AddDays(1),
            Completed = false
        };

        var payload = GetPayload(newTask);
        await _client.PostAsync("/api/tasks", payload);

        var taskId = 1;
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await DeserializeResponse<TaskResponse>(response);

        Assert.NotNull(responseBody);
        Assert.Equal($"Task with ID {taskId} was successfully retrieved.", responseBody.Message);
        Assert.Equal(taskId, responseBody.Task.TaskID);
    }

    [Fact]
    public async Task GetTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var taskId = 999;

        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        var responseBody = await DeserializeResponse<Dictionary<string, string>>(response);
        Assert.Equal($"Task with ID {taskId} not found.", responseBody["message"]);
    }

    // Update Task test for success and error
    [Fact]
    public async Task UpdateTask_ShouldModifyTaskSuccessfully()
    {
        var newTask = new TaskModel
        {
            Title = "New Task",
            Description = "Task Description",
            Category = "Work",
            Deadline = DateTime.UtcNow.AddDays(1),
            Completed = false
        };

        var payload = GetPayload(newTask);
        var createResponse = await _client.PostAsync("/api/tasks", payload);

        createResponse.EnsureSuccessStatusCode();
        var createdTaskResponse = await DeserializeResponse<TaskResponse>(createResponse);
        var taskId = createdTaskResponse.Task.TaskID;

        var updatedTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Updated Task Title",
            Description = "Updated Task Description",
            Category = "Personal",
            Deadline = DateTime.UtcNow.AddDays(2),
            Completed = true
        };

        var updatePayload = GetPayload(updatedTask);
        var updateResponse = await _client.PutAsync($"/api/tasks/{taskId}", updatePayload);

        updateResponse.EnsureSuccessStatusCode();
        var updatedTaskResponse = await DeserializeResponse<TaskResponse>(updateResponse);

        Assert.Equal($"Task with ID {taskId} was successfully updated.", updatedTaskResponse.Message);
        Assert.Equal(updatedTask.Title, updatedTaskResponse.Task.Title);
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var taskId = 999;
        var updatedTask = new TaskModel
        {
            TaskID = taskId,
            Title = "Updated Task Title",
            Description = "Updated Task Description",
            Category = "Personal",
            Deadline = DateTime.UtcNow.AddDays(2),
            Completed = true
        };

        var updatePayload = GetPayload(updatedTask);
        var response = await _client.PutAsync($"/api/tasks/{taskId}", updatePayload);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        var responseBody = await DeserializeResponse<Dictionary<string, string>>(response);
        Assert.Equal($"Task with ID {taskId} not found.", responseBody["message"]);
    }

    // Delete Task test for success and error
    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskSuccessfully()
    {
        var newTask = new TaskModel
        {
            TaskID = 1,
            Title = "Task to Delete",
            Description = "Task Description",
            Category = "Work",
            Deadline = DateTime.UtcNow.AddDays(1),
            Completed = false
        };

        var payload = GetPayload(newTask);
        await _client.PostAsync("/api/tasks", payload);

        var taskId = 1;
        var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

        response.EnsureSuccessStatusCode();
        var responseBody = await DeserializeResponse<TaskResponse>(response);

        Assert.Equal($"Task with ID {taskId} was successfully deleted.", responseBody.Message);
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var taskId = 999;

        var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        var responseBody = await DeserializeResponse<Dictionary<string, string>>(response);
        Assert.Equal($"Task with ID {taskId} not found.", responseBody["message"]);
    }


    // Get Task test for success and error
    [Fact]
    public async Task GetTasks_ShouldReturnTasksSuccessfully()
    {
        var newTask = new TaskModel
        {
            Title = "New Task",
            Description = "Task Description",
            Category = "Work",
            Deadline = DateTime.UtcNow.AddDays(1),
            Completed = false
        };

        var payload = GetPayload(newTask);
        await _client.PostAsync("/api/tasks", payload);

        var response = await _client.GetAsync("/api/tasks");

        response.EnsureSuccessStatusCode();
        var responseBody = await DeserializeResponse<TasksResponse>(response);

        Assert.NotNull(responseBody);
        Assert.Equal("Tasks retrieved successfully.", responseBody.Message);
        Assert.NotEmpty(responseBody.Tasks);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnNotFound_WhenNoTasksExist()
    {
        var response = await _client.GetAsync("/api/tasks");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

        var responseBody = await DeserializeResponse<Dictionary<string, string>>(response);
        Assert.Equal("No tasks found.", responseBody["message"]);
    }
}

