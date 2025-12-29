using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services;

public interface IUserService
{
    System.Threading.Tasks.Task<IEnumerable<UserDto>> GetAllUsersAsync();
    System.Threading.Tasks.Task<UserDto?> GetUserByIdAsync(int id);
    System.Threading.Tasks.Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    System.Threading.Tasks.Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    System.Threading.Tasks.Task<bool> DeleteUserAsync(int id);
}


