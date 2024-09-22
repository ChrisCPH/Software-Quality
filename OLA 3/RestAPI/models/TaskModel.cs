using System.ComponentModel.DataAnnotations;

namespace RestAPI.Models
{
    public class TaskModel
    {
        [Required(ErrorMessage = "The task id is required.")]
        public int TaskID { get; set; }

        [Required(ErrorMessage = "The task title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "The task description is required.")]
        [MinLength(10, ErrorMessage = "The description must be at least 10 characters long.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "The task category is required.")]
        public string Category { get; set; } = string.Empty;

        public DateTime Deadline { get; set; }

        public bool Completed { get; set; }
    }

    public class TaskResponse
    {
        public string? Message { get; set; }
        public TaskModel? Task { get; set; }
    }

    public class TasksResponse
    {
        public string? Message { get; set; }
        public IEnumerable<TaskModel>? Tasks { get; set; }
    }
}