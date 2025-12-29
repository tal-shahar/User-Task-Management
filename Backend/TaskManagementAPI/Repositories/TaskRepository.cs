using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<ModelsTask>> GetAllAsync(int? userId = null)
    {
        var query = _context.Tasks.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }
        return await query.ToListAsync();
    }

    public async System.Threading.Tasks.Task<ModelsTask?> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<ModelsTask> CreateAsync(ModelsTask task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<ModelsTask?> UpdateAsync(int id, ModelsTask task)
    {
        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
        {
            return null;
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.DueDate = task.DueDate;
        existingTask.Priority = task.Priority;
        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingTask;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<IEnumerable<ModelsTask>> GetOverdueTasksAsync(DateTime currentDate)
    {
        return await _context.Tasks
            .Where(t => t.DueDate < currentDate)
            .ToListAsync();
    }
}

