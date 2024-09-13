using System.Net;
using System.Threading.Tasks;
using Xunit;
using Backend;
using Backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Collections.Generic;

public class TaskModelIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public TaskModelIntegrationTests(WebApplicationFactory<Program> factory)
	{
		_factory = factory;
	}

	[Fact]
	public async Task GetAllTasks_ReturnsSuccessStatusCode_AndTaskList()
	{
		var client = _factory.CreateClient();

		var response = await client.GetAsync("/api/taskmodel");

		response.EnsureSuccessStatusCode();
		var responseString = await response.Content.ReadAsStringAsync();
		var tasks = JsonConvert.DeserializeObject<List<TaskModel>>(responseString);

		Assert.NotNull(tasks);
		Assert.IsType<List<TaskModel>>(tasks);
	}

	[Fact]
	public async Task CreateTask_ReturnsCreatedStatusCode_AndTaskObject()
	{
		var client = _factory.CreateClient();
		var newTask = new TaskModel
		{
			TaskID = 10,
			Title = "Test Title",
			Description = "TestDescription",
			Deadline = DateTime.Parse("2024-04-13T08:30:00Z"),
			Completed = false,
			ListID = 1
		};

		var response = await client.PostAsJsonAsync("/api/taskmodel/", newTask);

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		var createdTask = await response.Content.ReadFromJsonAsync<TaskModel>();
		Assert.NotNull(createdTask);
		Assert.Equal("Test Title", createdTask.Title);
	}
}
