using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPI.Data;
using RestAPI.Models;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get All
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks()
        {
            try
            {
                var tasks = await _context.Tasks.ToListAsync();

                if (tasks == null || !tasks.Any())
                {
                    return NotFound(new { message = "No tasks found." });
                }

                return Ok(new TasksResponse
                {
                    Message = "Tasks retrieved successfully.",
                    Tasks = tasks
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving tasks.", details = ex.Message });
            }
        }


        // Get
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetTask(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                if (task == null)
                {
                    return NotFound(new { message = $"Task with ID {id} not found." });
                }

                return Ok(new TaskResponse
                {
                    Message = $"Task with ID {id} was successfully retrieved.",
                    Task = task
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving the task.", details = ex.Message });
            }
        }

        // Create
        [HttpPost]
        public async Task<ActionResult<TaskModel>> CreateTask(TaskModel task)
        {
            try
            {
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTask), new { id = task.TaskID }, new TaskResponse
                {
                    Message = "Task was successfully created.",
                    Task = task
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the task.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while creating the task.", details = ex.Message });
            }
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskModel task)
        {
            if (id != task.TaskID)
            {
                return BadRequest(new { message = "Task ID in the URL does not match the Task ID in the body." });
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new TaskResponse
                {
                    Message = $"Task with ID {id} was successfully updated.",
                    Task = task
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.TaskID == id))
                {
                    return NotFound(new { message = $"Task with ID {id} not found." });
                }
                else
                {
                    return StatusCode(500, new { message = "A concurrency error occurred while updating the task." });
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the task.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }


        // Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound(new { message = $"Task with ID {id} not found." });
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return Ok(new TaskResponse
                {
                    Message = $"Task with ID {id} was successfully deleted.",
                    Task = task
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the task.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}