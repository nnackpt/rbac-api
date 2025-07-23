using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;

namespace RBACapi.Services
{
    public class UsersAuthorizeService : IUsersAuthorizeService
    {
        private readonly ApplicationDbContext _context;

        public UsersAuthorizeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all users authorize entries
        public async Task<List<CM_USERS_AUTHORIZE>> GetAllAsync()
        {
            return await _context.UsersAuthorize.ToListAsync();
        }

        // Get user authorize entry by AUTH_CODE
        public async Task<CM_USERS_AUTHORIZE?> GetByIdAsync(string authCode)
        {
            return await _context.UsersAuthorize.FindAsync(authCode);
        }

        // Get all active authorization for a specific user ID
        public async Task<List<CM_USERS_AUTHORIZE>> GetByUserIdAsync(string userId)
        {
            return await _context.UsersAuthorize
                .Where(u => u.USERID == userId && u.ACTIVE == true).ToListAsync();
        }

        // Create new user authorization
        public async Task<IEnumerable<CM_USERS_AUTHORIZE>> CreateAsync(UsersAuthorizeCreateRequestDto request, string createdBy)
        {
            var appCodePrefix = request.APP_CODE.StartsWith("APP_") ? request.APP_CODE.Substring(4) : request.APP_CODE;

            // ดึง AUTH_CODE ที่ตรงเงื่อนไขทั้งหมดมาในหน่วยความจำ
            var authCodes = await _context.UsersAuthorize
                .Where(ua => ua.APP_CODE == request.APP_CODE && ua.AUTH_CODE.StartsWith($"{appCodePrefix}_USER_"))
                .Select(ua => ua.AUTH_CODE)
                .ToListAsync();

            // หาเลข USER ที่มากที่สุด
            int highestExistingUserNumber = 0;
            foreach (var code in authCodes)
            {
                var parts = code.Split('_');
                if (parts.Length >= 4 && parts[2] == "USER")
                {
                    if (int.TryParse(parts[3], out int userNumber))
                    {
                        if (userNumber > highestExistingUserNumber)
                            highestExistingUserNumber = userNumber;
                    }
                }
            }

            int nextUserNumber = highestExistingUserNumber + 1;
            var createdAuthorizations = new List<CM_USERS_AUTHORIZE>();
            var now = DateTime.UtcNow;

            // For each facility, create a new AUTH_CODE with incremented USER number
            foreach (var facility in request.Facilities)
            {
                string userAuthCode = $"{appCodePrefix}_USER_{nextUserNumber:000}";
                var newAuth = new CM_USERS_AUTHORIZE
                {
                    AUTH_CODE = userAuthCode,
                    APP_CODE = request.APP_CODE,
                    ROLE_CODE = request.ROLE_CODE,
                    USERID = request.USERID,
                    SITE_CODE = facility.SITE_CODE,
                    DOMAIN_CODE = facility.DOMAIN_CODE,
                    FACT_CODE = facility.FACT_CODE,
                    FNAME = request.FNAME,
                    LNAME = request.LNAME,
                    ORG = request.ORG,
                    ACTIVE = request.ACTIVE,
                    CREATED_BY = createdBy,
                    CREATED_DATETIME = now,
                    // UPDATED_BY = createdBy,
                    // UPDATED_DATETIME = now
                };
                _context.UsersAuthorize.Add(newAuth);
                createdAuthorizations.Add(newAuth);
                nextUserNumber++; // increment for next facility
            }

            await _context.SaveChangesAsync();
            return createdAuthorizations;
        }

        public async Task UpdateAsync(string authCode, UsersAuthorizeUpdateRequestDto request, string updatedBy)
        {
            var existingAuthorizations = await _context.UsersAuthorize
                .Where(ua => ua.AUTH_CODE == authCode)
                .ToListAsync();

            if (!existingAuthorizations.Any())
            {
                return;
            }

            var now = DateTime.UtcNow;

            foreach (var auth in existingAuthorizations)
            {
                if (!string.IsNullOrEmpty(request.ROLE_CODE))
                    auth.ROLE_CODE = request.ROLE_CODE;
                if (!string.IsNullOrEmpty(request.FNAME))
                    auth.FNAME = request.FNAME;
                if (!string.IsNullOrEmpty(request.LNAME))
                    auth.LNAME = request.LNAME;
                if (!string.IsNullOrEmpty(request.ORG))
                    auth.ORG = request.ORG;
                if (request.ACTIVE.HasValue)
                    auth.ACTIVE = request.ACTIVE;

                auth.UPDATED_BY = updatedBy;
                auth.UPDATED_DATETIME = now;
            }

            if (request.Facilities != null && request.Facilities.Any())
            {
                var existingFacilities = existingAuthorizations.Select(ea => new FacilitySelectionDto
                {
                    SITE_CODE = ea.SITE_CODE,
                    DOMAIN_CODE = ea.DOMAIN_CODE,
                    FACT_CODE = ea.FACT_CODE
                }).ToList();

                var newFacilities = request.Facilities.ToList();

                // Facilities to add
                var facilitiesToAdd = newFacilities
                    .Where(nf => !existingFacilities.Any(ef =>
                        ef.SITE_CODE == nf.SITE_CODE &&
                        ef.DOMAIN_CODE == nf.DOMAIN_CODE &&
                        ef.FACT_CODE == nf.FACT_CODE))
                    .ToList();

                // Facilities to remove
                var facilitiesToRemove = existingFacilities
                    .Where(ef => !newFacilities.Any(nf =>
                        nf.SITE_CODE == ef.SITE_CODE &&
                        nf.DOMAIN_CODE == ef.DOMAIN_CODE &&
                        nf.FACT_CODE == ef.FACT_CODE))
                    .ToList();

                // Add new facilities                
                foreach (var facilityDto in facilitiesToAdd)
                {
                    var baseAuth = existingAuthorizations.First();

                    var newAuth = new CM_USERS_AUTHORIZE
                    {
                        AUTH_CODE = baseAuth.AUTH_CODE, 
                        APP_CODE = baseAuth.APP_CODE,
                        ROLE_CODE = request.ROLE_CODE ?? baseAuth.ROLE_CODE, 
                        USERID = baseAuth.USERID,
                        SITE_CODE = facilityDto.SITE_CODE,
                        DOMAIN_CODE = facilityDto.DOMAIN_CODE,
                        FACT_CODE = facilityDto.FACT_CODE,
                        FNAME = request.FNAME ?? baseAuth.FNAME,
                        LNAME = request.LNAME ?? baseAuth.LNAME,
                        ORG = request.ORG ?? baseAuth.ORG,
                        ACTIVE = request.ACTIVE ?? baseAuth.ACTIVE,
                        CREATED_BY = baseAuth.CREATED_BY,
                        CREATED_DATETIME = baseAuth.CREATED_DATETIME,
                        UPDATED_BY = updatedBy,
                        UPDATED_DATETIME = now
                    };
                    _context.UsersAuthorize.Add(newAuth);
                }

                // Remove old facilities
                foreach (var facilityDto in facilitiesToRemove)
                {
                    var entityToRemove = existingAuthorizations.FirstOrDefault(ea =>
                        ea.SITE_CODE == facilityDto.SITE_CODE &&
                        ea.DOMAIN_CODE == facilityDto.DOMAIN_CODE &&
                        ea.FACT_CODE == facilityDto.FACT_CODE);

                    if (entityToRemove != null)
                    {
                        _context.UsersAuthorize.Remove(entityToRemove);
                    }
                }
            }

            // Save all Changes
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string authCode)
        {
            var entity = await _context.UsersAuthorize.FindAsync(authCode);
            if (entity != null)
            {
                _context.UsersAuthorize.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<FacilitySelectionDto>> GetUserFacilitiesByAuthCodeAsync(string authCode)
        {
            var facilities = await _context.UsersAuthorize
                .Where(u => u.AUTH_CODE == authCode)
                .Select(u => new FacilitySelectionDto
                {
                    SITE_CODE = u.SITE_CODE,
                    DOMAIN_CODE = u.DOMAIN_CODE,
                    FACT_CODE = u.FACT_CODE
                })
                .Distinct()
                .ToListAsync();

            return facilities;
        }

        public async Task<List<CM_USERS_AUTHORIZE>> GetByAuthCodeForFacilitiesAsync(string authCode)
        {
            return await _context.UsersAuthorize
                .Where(u => u.AUTH_CODE == authCode)
                .ToListAsync();
        }
    }
}