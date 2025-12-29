using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;
using TaskManagementAPI.Repositories;
using BCrypt.Net;

namespace TaskManagementAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async System.Threading.Tasks.Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async System.Threading.Tasks.Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async System.Threading.Tasks.Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(createUserDto.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = (Role)createUserDto.Role,
            FullName = createUserDto.FullName,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToDto(createdUser);
    }

    public async System.Threading.Tasks.Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            return null;
        }

        existingUser.Email = updateUserDto.Email;
        existingUser.Role = (Role)updateUserDto.Role;
        existingUser.FullName = updateUserDto.FullName;
        existingUser.IsActive = updateUserDto.IsActive;

        var updatedUser = await _userRepository.UpdateAsync(id, existingUser);
        return updatedUser == null ? null : MapToDto(updatedUser);
    }

    public async System.Threading.Tasks.Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = (RoleDto)user.Role,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}


