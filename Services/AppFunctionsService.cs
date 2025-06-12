using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;

namespace RBACapi.Services
{
    public class AppFunctionsService
    {
        private readonly ApplicationDbContext _context;

        public AppFunctionsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all
        public async Task<List<CM_APPS_FUNCTIONS>> GetAllAsync()
        {
            return await _context.AppFunctions.ToListAsync();
            //return await _context.AppFunctions.Include(r => r.CM_APPLICATIONS).ToListAsync();
        }

        // Get by id
        public async Task<CM_APPS_FUNCTIONS?> GetByIdAsync(string funcCode)
        {
            return await _context.AppFunctions.FindAsync(funcCode);
        }

        // Create new
        public async Task<CM_APPS_FUNCTIONS> CreateAsync(CM_APPS_FUNCTIONS func)
        {
            _context.AppFunctions.Add(func);
            await _context.SaveChangesAsync();
            return func;
        }

        // Update by id
        public async Task<CM_APPS_FUNCTIONS?> UpdateAsync(string funcCode, CM_APPS_FUNCTIONS updated)
        {
            var func = await _context.AppFunctions.FindAsync(funcCode);
            if (func == null) return null;

            func.NAME = updated.NAME;
            func.DESC = updated.DESC;
            func.FUNC_URL = updated.FUNC_URL;
            func.ACTIVE = updated.ACTIVE;
            func.UPDATED_BY = updated.UPDATED_BY;
            func.UPDATED_DATETIME = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return func;
        }

        // Delete by id
        public async Task<bool> DeleteAsync(string funcCode)
        {
            var func = await _context.AppFunctions.FindAsync(funcCode);
            if (func == null) return false;

            _context.AppFunctions.Remove(func);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}