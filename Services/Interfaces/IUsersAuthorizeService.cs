using RBACapi.Models;
using RBACapi.Models.Dtos;

namespace RBACapi.Services.Interfaces
{
    public interface IUsersAuthorizeService
    {
        Task<List<CM_USERS_AUTHORIZE>> GetAllAsync();
        Task<CM_USERS_AUTHORIZE?> GetByIdAsync(string authCode);
        Task<List<CM_USERS_AUTHORIZE>> GetByUserIdAsync(string userId);
        Task<IEnumerable<CM_USERS_AUTHORIZE>> CreateAsync(UsersAuthorizeCreateRequestDto request, string createdBy);
        Task UpdateAsync(CM_USERS_AUTHORIZE userAuthorize);
        Task DeleteAsync(string authCode);
    }
}