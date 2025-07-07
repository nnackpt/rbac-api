using RBACapi.Models;

namespace RBACapi.Services.Interfaces
{
    public interface IRbacService
    {
        Task<IEnumerable<CM_RBAC>> GetAllAsync();
        Task<CM_RBAC?> GetByCodeAsync(string rbacCode);
        Task<CM_RBAC> CreateAsync(RbacRequest req);
        Task<CM_RBAC?> UpdateAsync(string rbacCode, RbacUpdateRequest req);
        Task<IEnumerable<CM_RBAC?>> DeleteAsync(string rbacCode);
        Task<IEnumerable<string>> GetAssignedFunctionsAsync(string appCode, string roleCode);
    }
}