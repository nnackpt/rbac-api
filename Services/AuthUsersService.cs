using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;

namespace RBACapi.Services
{
    public class AuthUsersService : IAuthUsersService
    {
        private readonly ApplicationDbContext _context;

        public AuthUsersService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuthUsersResponseDto>> GetAllAuthUsersAsync()
        {
            var authUsers = await _context.UsersAuthorize
                .Include(ua => ua.Application)
                .Include(ua => ua.Role)
                .Where(ua => ua.ACTIVE == true) 
                .Select(ua => new AuthUsersResponseDto
                {
                    // AuthCode = ua.AUTH_CODE,
                    ApplicationTitle = ua.Application != null ? ua.Application.TITLE ?? "" : "",
                    RoleName = ua.Role != null ? ua.Role.NAME ?? "" : "",
                    UserId = ua.USERID ?? "",
                    Name = CombineName(ua.FNAME, ua.LNAME),
                    Org = ua.ORG ?? "",
                    SiteFacility = FormatSiteFacility(ua.SITE_CODE, ua.FACT_CODE),
                    CreatedBy = ua.CREATED_BY ?? "",
                    CreatedDateTime = ua.CREATED_DATETIME,
                    UpdatedBy = ua.UPDATED_BY ?? "",
                    UpdatedDateTime = ua.UPDATED_DATETIME
                })
                .OrderBy(ua => ua.ApplicationTitle)
                .ThenBy(ua => ua.RoleName)
                .ThenBy(ua => ua.UserId)
                .ToListAsync();

            return authUsers;
        }

        private static string CombineName(string? firstName, string? lastName)
        {
            var name = "";
            if (!string.IsNullOrEmpty(firstName))
                name += firstName;
            if (!string.IsNullOrEmpty(lastName))
            {
                if (!string.IsNullOrEmpty(name))
                    name += " ";
                name += lastName;
            }
            return name;
        }

        private static string FormatSiteFacility(string? siteCode, string? factCode)
        {
            if (string.IsNullOrEmpty(siteCode) && string.IsNullOrEmpty(factCode))
                return "";

            var result = siteCode ?? "";
            if (!string.IsNullOrEmpty(factCode))
            {
                result += $" [{factCode}]";
            }
            return result;
        }
    }
}