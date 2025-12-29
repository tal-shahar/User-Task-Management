using TaskManagementAPI.Models;
using ModelsTask = TaskManagementAPI.Models.Task;

namespace TaskManagementAPI.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<ModelsTask>> GetAllAsync(int? userId = null);
    System.Threading.Tasks.Task<ModelsTask?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<ModelsTask> CreateAsync(ModelsTask task);
    System.Threading.Tasks.Task<ModelsTask?> UpdateAsync(int id, ModelsTask task);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<ModelsTask>> GetOverdueTasksAsync(DateTime currentDate);
}

