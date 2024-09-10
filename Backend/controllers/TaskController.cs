using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskModelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskModelController(AppDbContext context)
        {
            _context = context;
        }

        // Get All Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks()
        {
            return await _context.TaskModel.ToListAsync();
        }

        // Get Task by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetTask(int id)
        {
            var task = await _context.TaskModel.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        // Create a new Task
        [HttpPost]
        public async Task<ActionResult<TaskModel>> CreateTask(TaskModel task)
        {
            _context.TaskModel.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.TaskID }, task);
        }

        // Update an existing Task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskModel task)
        {
            if (id != task.TaskID)
            {
                return BadRequest();
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TaskModel.Any(e => e.TaskID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Delete a Task
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.TaskModel.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.TaskModel.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
