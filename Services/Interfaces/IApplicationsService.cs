using RBACapi.Models;

namespace RBACapi.Services.Interfaces
{
    public interface IApplicationsService
    {
        Task<List<CM_APPLICATIONS>> GetAllApplicationsAsync();
        Task<CM_APPLICATIONS?> GetApplicationByCodeAsync(string code);
        Task CreateApplicationAsync(CM_APPLICATIONS app);
        Task<bool> UpdateApplicationAsync(CM_APPLICATIONS app);
        Task<bool> DeleteApplicationAsync(string code); 
    }
}