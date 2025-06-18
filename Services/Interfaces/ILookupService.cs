using RBACapi.Models;

namespace RBACapi.Services.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<CM_APPS_ROLES>> GetRolesByAppAsync(string appCode);
        Task<IEnumerable<CM_APPS_FUNCTIONS>> GetFunctionsByAppAsync(string appCode);
    }
}