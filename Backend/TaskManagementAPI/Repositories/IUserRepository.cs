using TaskManagementAPI.Models;
using ModelsUser = TaskManagementAPI.Models.User;

namespace TaskManagementAPI.Repositories;

public interface IUserRepository
{
    System.Threading.Tasks.Task<IEnumerable<ModelsUser>> GetAllAsync();
    System.Threading.Tasks.Task<ModelsUser?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<ModelsUser?> GetByUsernameAsync(string username);
    System.Threading.Tasks.Task<ModelsUser> CreateAsync(ModelsUser user);
    System.Threading.Tasks.Task<ModelsUser?> UpdateAsync(int id, ModelsUser user);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
}


