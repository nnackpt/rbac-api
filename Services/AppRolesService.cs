using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

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
