using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Services
{
    public class RbacService : IRbacService
    {
        private readonly ApplicationDbContext _context;

        public RbacService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all RBAC
        public async Task<IEnumerable<CM_RBAC>> GetAllAsync()
        {
            return await _context.RBAC
                .Include(r => r.CM_APPLICATIONS)
                .Include(r => r.CM_APPS_ROLES)
                .Include(r => r.CM_APPS_FUNCTIONS)
                .ToListAsync();
        }

        // Get RBAC by code
        public async Task<CM_RBAC?> GetByCodeAsync(string rbacCode)
        {
            return await _context.RBAC
                .Include(r => r.CM_APPLICATIONS)
                .Include(r => r.CM_APPS_ROLES)
                .Include(r => r.CM_APPS_FUNCTIONS)
                .FirstOrDefaultAsync(r => r.RBAC_CODE == rbacCode);
        }

        // Create new RBAC
        public async Task<CM_RBAC> CreateAsync(RbacRequest req)
        {
            var now = DateTime.UtcNow;
            var appCodeSuffix = req.APP_CODE.Replace("APP_", "");

            var items = req.FUNC_CODES.Select((code, index) => new CM_RBAC
            {
                RBAC_CODE = $"{appCodeSuffix}_RBAC{index + 1:00}",
                ROLE_CODE = req.ROLE_CODE,
                FUNC_CODE = code,
                APP_CODE = req.APP_CODE,
                ACTIVE = true,
                CREATED_BY = req.CREATED_BY,
                CREATED_DATETIME = now,
                UPDATED_BY = req.CREATED_BY,
                UPDATED_DATETIME = now
            }).ToList();

            _context.RBAC.AddRange(items);
            await _context.SaveChangesAsync();
            return items.First();
        }

        // Update RBAC
        public async Task<CM_RBAC?> UpdateAsync(string rbacCode, RbacUpdateRequest req)
        {
            var existing = await _context.RBAC
                .Where(r => r.RBAC_CODE == rbacCode)
                .ToListAsync();

            _context.RBAC.RemoveRange(existing);

            var now = DateTime.UtcNow;
            var appCodeSuffix = req.APP_CODE.Replace("APP_", "");
            
            var newItems = req.FUNC_CODES.Select((code, index) => new CM_RBAC
            {
                RBAC_CODE = $"{appCodeSuffix}_RBAC{index + 1:00}",
                ROLE_CODE = req.ROLE_CODE,
                FUNC_CODE = code,
                APP_CODE = req.APP_CODE,
                ACTIVE = true,
                CREATED_BY = req.UPDATED_BY,
                CREATED_DATETIME = now,
                UPDATED_BY = req.UPDATED_BY,
                UPDATED_DATETIME = now
            });

            await _context.RBAC.AddRangeAsync(newItems);
            await _context.SaveChangesAsync();
            
            return newItems.FirstOrDefault();
        }

        // Delete RBAC
        public async Task<IEnumerable<CM_RBAC?>> DeleteAsync(string rbacCode)
        {
            var rbacs = await _context.RBAC
                .Where(r => r.RBAC_CODE == rbacCode)
                .ToListAsync();

            if (rbacs == null || rbacs.Count == 0)
            {
                return null;
            }

            _context.RBAC.RemoveRange(rbacs);
            await _context.SaveChangesAsync();

            return rbacs;
        }
    }
}