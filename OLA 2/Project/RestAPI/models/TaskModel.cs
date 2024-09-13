namespace RestAPI.Models 
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
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