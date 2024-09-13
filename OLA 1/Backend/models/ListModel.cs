namespace Backend.Models 
{
    public class List
    {
        public int ListID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}