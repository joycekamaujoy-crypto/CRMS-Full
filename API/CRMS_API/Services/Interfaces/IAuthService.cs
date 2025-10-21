using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> ConfirmEmailAsync(string email, string token);
    }
}
