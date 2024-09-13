using Xunit;
using Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class TaskModelSpecificationTests
{
    [Fact]
    public void ValidateTaskModel_ShouldHaveRequiredTitleError()
    {
        var task = new TaskModel
        {
            Title = null,
            Description = "Some description",
            Deadline = DateTime.Now,
            Completed = false,
            ListID = 1
        };

        var context = new ValidationContext(task);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(task, context, validationResults, true);

        Assert.NotNull(validationResults);

        var errorMessage = "The Title field is required.";
        Assert.Contains(validationResults, v => 
            v.ErrorMessage != null && v.ErrorMessage.Contains(errorMessage, StringComparison.OrdinalIgnoreCase)
        );
    }
}