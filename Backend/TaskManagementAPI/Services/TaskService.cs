using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;
using TaskManagementAPI.Repositories;

namespace TaskManagementAPI.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly AppDbContext _context;

    public TaskService(ITaskRepository taskRepository, AppDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync(int? userId = null)
    {
        var tasks = await _taskRepository.GetAllAsync(userId);
        return tasks.Select(MapToDto);
    }

    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task == null ? null : MapToDto(task);
    }

    public async System.Threading.Tasks.Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var task = new Models.Task
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            DueDate = createTaskDto.DueDate,
            Priority = (Priority)createTaskDto.Priority,
            UserId = userId,
            UserFullName = string.IsNullOrEmpty(user.FullName) ? user.Username : user.FullName,
            UserEmail = user.Email
        };

        var createdTask = await _taskRepository.CreateAsync(task);
        return MapToDto(createdTask);
    }

    public async System.Threading.Tasks.Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return null;
        }

        existingTask.Title = updateTaskDto.Title;
        existingTask.Description = updateTaskDto.Description;
        existingTask.DueDate = updateTaskDto.DueDate;
        existingTask.Priority = (Priority)updateTaskDto.Priority;

        var updatedTask = await _taskRepository.UpdateAsync(id, existingTask);
        return updatedTask == null ? null : MapToDto(updatedTask);
    }

    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    private static TaskDto MapToDto(Models.Task task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = (PriorityDto)task.Priority,
            UserFullName = task.UserFullName,
            UserEmail = task.UserEmail,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}

