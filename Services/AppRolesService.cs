using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;
using System.Linq;
using System.Text.RegularExpressions;

namespace RBACapi.Services
{
    public class AppRolesService : IAppRolesService
    {
        private readonly ApplicationDbContext _context;

        public AppRolesService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get All
        public async Task<IEnumerable<CM_APPS_ROLES>> GetAllRolesAsync()
        {
            return await _context.AppRoles.Include(r => r.CM_APPLICATIONS).ToListAsync();
            //return await _context.AppRoles.ToListAsync();
        }

        // Get By Code
        public async Task<CM_APPS_ROLES?> GetRoleByCodeAsync(string code)
        {
            return await _context.AppRoles.Include(r => r.CM_APPLICATIONS).FirstOrDefaultAsync(r => r.ROLE_CODE == code);
        }

        // Create new 
        public async Task<CM_APPS_ROLES> CreateRoleAsync(CM_APPS_ROLES role)
        {
            if (string.IsNullOrEmpty(role.APP_CODE))
            {
                throw new ArgumentException("APP_CODE can't be null or empty.");
            }

            string appCodePrefix = role.APP_CODE;
            if (appCodePrefix.StartsWith("APP_"))
            {
                appCodePrefix = appCodePrefix.Substring(4);
            }

            int maxExistingSuffix = 0;
            var existingRoleCodes = await _context.AppRoles
                .Where(r => r.APP_CODE == role.APP_CODE && r.ROLE_CODE.StartsWith($"{appCodePrefix}_R"))
                .Select(r => r.ROLE_CODE)
                .ToListAsync();

            if (existingRoleCodes.Any())
            {
                var regex = new Regex($@"^{Regex.Escape(appCodePrefix)}_R(\d+)$");

                maxExistingSuffix = existingRoleCodes
                    .Select(code =>
                    {
                        var match = regex.Match(code);
                        return match.Success && int.TryParse(match.Groups[1].Value, out int suffix) ? suffix : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();
            }

            int nextSuffix = maxExistingSuffix + 1;
            role.ROLE_CODE = $"{appCodePrefix}_R{nextSuffix:00}";
            
            _context.AppRoles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        // Update by code
        public async Task<CM_APPS_ROLES?> UpdateRoleAsync(string code, CM_APPS_ROLES updatedRole)
        {
            var role = await _context.AppRoles.FindAsync(code);
            if (role == null) return null;

            role.NAME = updatedRole.NAME;
            role.DESC = updatedRole.DESC;
            role.HOME_URL = updatedRole.HOME_URL;
            role.ACTIVE = updatedRole.ACTIVE;
            role.UPDATED_BY = updatedRole.UPDATED_BY;
            role.UPDATED_DATETIME = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return role;
        }

        // Delete by code
        public async Task<bool> DeleteRoleAsync(string code)
        {
            var role = await _context.AppRoles.FindAsync(code);
            if (role == null) return false;

            _context.AppRoles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
