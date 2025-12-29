using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services;

public interface ITaskService
{
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync(int? userId = null);
    System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id);
    System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId);
    System.Threading.Tasks.Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
}

