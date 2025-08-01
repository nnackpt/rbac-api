using RBACapi.Models.Dtos;

namespace RBACapi.Services.Interfaces
{
    public interface IAuthUsersService
    {
        Task<List<AuthUsersResponseDto>> GetAllAuthUsersAsync();
    }
}