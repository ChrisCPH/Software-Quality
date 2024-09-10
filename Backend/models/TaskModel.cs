using System.ComponentModel.DataAnnotations;

namespace Backend.Models 
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        [Required(ErrorMessage = "The Title field is required.")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public bool Completed { get; set; }
        public int ListID {get; set; }
    }
}