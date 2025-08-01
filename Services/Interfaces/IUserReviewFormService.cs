using RBACapi.Models.Dtos;

namespace RBACapi.Services.Interfaces
{
    public interface IUserReviewFormService
    {
        Task<byte[]> GenerateUserReviewFormAsync(string? applicationName = null, string? roleName = null);
        Task<FormOptionsDto> GetFormOptionsAsync();
    }
}