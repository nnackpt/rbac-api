using RBACapi.Models;
using RBACapi.Models.Dtos;

namespace RBACapi.Services.Interfaces
{
    public interface IUsersAuthorizeService
    {
        Task<IEnumerable<CM_USERS_AUTHORIZE>> GetAllAsync();
        Task<IEnumerable<CM_USERS_AUTHORIZE>> GetByUserIdAsync(string userId);
        Task<CM_USERS_AUTHORIZE?> GetByIdAsync(string authCode); 
        Task<IEnumerable<CM_USERS_AUTHORIZE>> GetByAuthCodeForFacilitiesAsync(string authCode);

        Task<IEnumerable<FacilitySelectionDto>> GetUserFacilitiesByAuthCodeAsync(string authCode);
        Task<IEnumerable<FacilitySelectionDto>> GetUserFacilitiesByUserIdAsync(string userId);
        Task<IEnumerable<FacilitySelectionDto>> GetAllAvailableFacilitiesAsync();
        Task<IEnumerable<CM_USERS_AUTHORIZE>> CreateAsync(UsersAuthorizeCreateRequestDto request, string createdBy);
        Task UpdateAsync(string authCode, UsersAuthorizeUpdateRequestDto request, string updatedBy);
        Task DeleteAsync(string authCode);
    }
}