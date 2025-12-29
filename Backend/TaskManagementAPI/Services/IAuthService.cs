using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services;

public interface IAuthService
{
    System.Threading.Tasks.Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    string GenerateJwtToken(UserDto user);
}


