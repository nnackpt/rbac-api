// using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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

        // Get Assigned Functions
        public async Task<IEnumerable<string>> GetAssignedFunctionsAsync(string appCode, string roleCode)

        {
            var assignedFunctions = await _context.RBAC
                .Where(r => r.APP_CODE == appCode && r.ROLE_CODE == roleCode && r.ACTIVE == true)
                .Select(r => r.FUNC_CODE)
                .Distinct()
                .ToListAsync();

            return assignedFunctions;
        }
        // Create new RBAC
        public async Task<CM_RBAC> CreateAsync(RbacRequest req)
        {
            var now = DateTime.UtcNow;
            var appCodeSuffix = req.APP_CODE.Replace("APP_", "");

            // Delete ole Role and App
            var existingRbacs = await _context.RBAC
                .Where(r => r.ROLE_CODE == req.ROLE_CODE && r.APP_CODE == req.APP_CODE)
                .ToListAsync();

            if (existingRbacs.Any())
            {
                _context.RBAC.RemoveRange(existingRbacs);
            }

            // Create new RBAC
            var existingCodes = await _context.RBAC
                .Where(r => r.RBAC_CODE.StartsWith($"{appCodeSuffix}_RBAC"))
                .Select(r => r.RBAC_CODE)
                .ToListAsync();

            int maxIndex = 0;
            foreach (var code in existingCodes)
            {
                var suffix = code.Replace($"{appCodeSuffix}_RBAC", "");
                if (int.TryParse(suffix, out int index))
                {
                    maxIndex = Math.Max(maxIndex, index);
                }
            }

            var items = req.FUNC_CODES.Select((funcCode, i) => new CM_RBAC
            {
                RBAC_CODE = $"{appCodeSuffix}_RBAC{maxIndex + i + 1:00}",
                ROLE_CODE = req.ROLE_CODE,
                FUNC_CODE = funcCode,
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
            var existingRbacs = await _context.RBAC
                .Where(r => r.APP_CODE == req.APP_CODE && r.ROLE_CODE == req.ROLE_CODE)
                .ToListAsync();

            var existingFuncCodes = existingRbacs.Select(r => r.FUNC_CODE).ToHashSet();
            var requestedFuncCodes = req.FUNC_CODES.ToHashSet();

            var funcsToRemove = existingRbacs
                .Where(r => !requestedFuncCodes.Contains(r.FUNC_CODE))
                .ToList();

            if (funcsToRemove.Any())
            {
                _context.RBAC.RemoveRange(funcsToRemove);
            }

            var funcCodesToAdd = requestedFuncCodes
                .Where(c => !existingFuncCodes.Contains(c))
                .ToList();

            var now = DateTime.UtcNow;
            var appCodeSuffix = req.APP_CODE.Replace("APP_", "");

            // Find next available sequential RBAC_CODE
            int maxExistingSuffix = 0;

            var rbacCodesForSuffix = await _context.RBAC
                .Where(r => r.RBAC_CODE.StartsWith($"{appCodeSuffix}_RBAC"))
                .Select(r => r.RBAC_CODE)
                .ToListAsync();

            if (rbacCodesForSuffix.Any())
            {
                var regex = new Regex($@"^{Regex.Escape(appCodeSuffix)}_RBAC(\d+)$");

                maxExistingSuffix = rbacCodesForSuffix
                    .Select(code =>
                    {
                        var match = regex.Match(code);
                        return match.Success && int.TryParse(match.Groups[1].Value, out int suffix) ? suffix : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();
            }

            int nextSuffix = maxExistingSuffix + 1;

            var newItems = funcCodesToAdd.Select(code =>
            {
                var rbacCodeForNewItem = $"{appCodeSuffix}_RBAC{nextSuffix:00}";
                nextSuffix++;

                return new CM_RBAC
                {
                    RBAC_CODE = rbacCodeForNewItem,
                    ROLE_CODE = req.ROLE_CODE,
                    FUNC_CODE = code,
                    APP_CODE = req.APP_CODE,
                    ACTIVE = true,
                    CREATED_BY = req.UPDATED_BY,
                    CREATED_DATETIME = now,
                    UPDATED_BY = req.UPDATED_BY,
                    UPDATED_DATETIME = now
                };
            }).ToList();

            if (newItems.Any())
            {
                await _context.RBAC.AddRangeAsync(newItems);
            }

            await _context.SaveChangesAsync();

            return await _context.RBAC
                .Include(r => r.CM_APPLICATIONS)
                .Include(r => r.CM_APPS_ROLES)
                .Include(r => r.CM_APPS_FUNCTIONS)
                .FirstOrDefaultAsync(r => r.APP_CODE == req.APP_CODE && r.ROLE_CODE == req.ROLE_CODE);
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