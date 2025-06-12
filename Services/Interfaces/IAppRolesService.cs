using RBACapi.Models;

namespace RBACapi.Services.Interfaces
{
    public interface IAppRolesService
    {
        Task<IEnumerable<CM_APPS_ROLES>> GetAllRolesAsync();
        Task<CM_APPS_ROLES?> GetRoleByCodeAsync(string code);
        Task<CM_APPS_ROLES> CreateRoleAsync(CM_APPS_ROLES role);
        Task<CM_APPS_ROLES?> UpdateRoleAsync(string code, CM_APPS_ROLES updatedRole);
        Task<bool> DeleteRoleAsync(string code);
    }
}