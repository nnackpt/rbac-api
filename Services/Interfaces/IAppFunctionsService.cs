using RBACapi.Models;

namespace RBACapi.Services.Interfaces
{
    public interface IAppFunctionsService
    {
        Task<List<CM_APPS_FUNCTIONS>> GetAllAsync();
        Task<CM_APPS_FUNCTIONS?> GetByIdAsync(string funcCode);
        Task<CM_APPS_FUNCTIONS> CreateAsync(CM_APPS_FUNCTIONS func);
        Task<CM_APPS_FUNCTIONS?> UpdateAsync(string funcCode, CM_APPS_FUNCTIONS updated);
        Task<bool> DeleteAsync(string funcCode);
        Task<IEnumerable<CM_APPS_FUNCTIONS>> GetByAppCodeAsync(string appCode);
    }
}