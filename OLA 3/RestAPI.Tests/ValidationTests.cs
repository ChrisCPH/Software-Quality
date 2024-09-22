using FluentAssertions;
using RestAPI.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class TaskModelTests
{
    [Fact]
    public void TaskModel_Should_Throw_Validation_Error_When_Description_Is_Less_Than_10_Characters()
    {
        var task = new TaskModel
        {
            TaskID = 1,
            Title = "Test Task",
            Description = "Short",
            Category = "General",
            Deadline = DateTime.Now,
            Completed = false
        };

        var validationResults = ValidateModel(task);

        validationResults.Should().ContainSingle(r => r.ErrorMessage == "The description must be at least 10 characters long.");
    }

    [Fact]
    public void TaskModel_Should_Throw_Validation_Error_When_Title_Is_Empty()
    {
        var task = new TaskModel
        {
            TaskID = 1,
            Title = "",
            Description = "This is a valid description.",
            Category = "General",
            Deadline = DateTime.Now,
            Completed = false
        };

        var validationResults = ValidateModel(task);

        validationResults.Should().ContainSingle(r => r.ErrorMessage == "The task title is required.");
    }

    [Fact]
    public void TaskModel_Should_Throw_Validation_Error_When_Category_Is_Empty()
    {
        var task = new TaskModel
        {
            TaskID = 1,
            Title = "Test Task",
            Description = "This is a valid description.",
            Category = "",
            Deadline = DateTime.Now,
            Completed = false
        };

        var validationResults = ValidateModel(task);

        validationResults.Should().ContainSingle(r => r.ErrorMessage == "The task category is required.");
    }

    private IList<ValidationResult> ValidateModel(TaskModel model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
