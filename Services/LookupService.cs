using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _context;

        public LookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CM_APPS_ROLES>> GetRolesByAppAsync(string appCode)
        {
            return await _context.AppRoles.Where(r => r.APP_CODE == appCode).ToListAsync();
        }

        public async Task<IEnumerable<CM_APPS_FUNCTIONS>> GetFunctionsByAppAsync(string appCode)
        {
            return await _context.AppFunctions.Where(f => f.APP_CODE == appCode).ToListAsync();
        }
    }
}