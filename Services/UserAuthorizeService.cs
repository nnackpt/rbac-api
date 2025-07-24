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
        public async Task<IEnumerable<CM_USERS_AUTHORIZE>> GetAllAsync()
        {
            return await _context.UsersAuthorize.ToListAsync();
        }

        // Get user authorize entry by AUTH_CODE
        public async Task<CM_USERS_AUTHORIZE?> GetByIdAsync(string authCode)
        {
            return await _context.UsersAuthorize
                .FirstOrDefaultAsync(x => x.AUTH_CODE == authCode);
        }

        // Get all active authorization for a specific user ID
        public async Task<IEnumerable<CM_USERS_AUTHORIZE>> GetByUserIdAsync(string userId)
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

        // Update Async
        public async Task UpdateAsync(string authCode, UsersAuthorizeUpdateRequestDto request, string updatedBy)
        {
            var now = DateTime.UtcNow;

            var existingAuthorizationsForAuthCode = await _context.UsersAuthorize
                .AsNoTracking()
                .Where(ua => ua.AUTH_CODE == authCode)
                .ToListAsync();

            if (!existingAuthorizationsForAuthCode.Any())
                throw new InvalidOperationException($"No existing authorization found for AUTH_CODE: '{authCode}'.");

            var baseUserAuthorization = existingAuthorizationsForAuthCode.First();

            if (request.Facilities != null)
            {
                var currentFacilitiesInDb = existingAuthorizationsForAuthCode.Select(ea => new FacilitySelectionDto
                {
                    SITE_CODE = ea.SITE_CODE!,
                    DOMAIN_CODE = ea.DOMAIN_CODE!,
                    FACT_CODE = ea.FACT_CODE!
                }).ToList();

                var requestedFacilities = request.Facilities.ToList();

                var facilitiesToRemove = existingAuthorizationsForAuthCode
                    .Where(ea => !requestedFacilities.Any(rf =>
                        rf.SITE_CODE == ea.SITE_CODE &&
                        rf.DOMAIN_CODE == ea.DOMAIN_CODE &&
                        rf.FACT_CODE == ea.FACT_CODE))
                    .ToList();

                var facilitiesToAdd = requestedFacilities
                    .Where(rf => !currentFacilitiesInDb.Any(cf =>
                        cf.SITE_CODE == rf.SITE_CODE &&
                        cf.DOMAIN_CODE == rf.DOMAIN_CODE &&
                        cf.FACT_CODE == rf.FACT_CODE))
                    .ToList();

                // Remove
                foreach (var entityToRemove in facilitiesToRemove)
                {
                    var toDelete = new CM_USERS_AUTHORIZE
                    {
                        AUTH_CODE = entityToRemove.AUTH_CODE,
                        APP_CODE = entityToRemove.APP_CODE,
                        ROLE_CODE = entityToRemove.ROLE_CODE,
                        USERID = entityToRemove.USERID,
                        SITE_CODE = entityToRemove.SITE_CODE,
                        DOMAIN_CODE = entityToRemove.DOMAIN_CODE,
                        FACT_CODE = entityToRemove.FACT_CODE
                    };

                    _context.Entry(toDelete).State = EntityState.Deleted;
                }

                // Add
                foreach (var facilityDto in facilitiesToAdd)
                {
                    var newAuth = new CM_USERS_AUTHORIZE
                    {
                        AUTH_CODE = authCode,
                        APP_CODE = baseUserAuthorization.APP_CODE,
                        ROLE_CODE = request.ROLE_CODE ?? baseUserAuthorization.ROLE_CODE,
                        USERID = baseUserAuthorization.USERID,
                        SITE_CODE = facilityDto.SITE_CODE,
                        DOMAIN_CODE = facilityDto.DOMAIN_CODE,
                        FACT_CODE = facilityDto.FACT_CODE,
                        FNAME = request.FNAME ?? baseUserAuthorization.FNAME,
                        LNAME = request.LNAME ?? baseUserAuthorization.LNAME,
                        ORG = request.ORG ?? baseUserAuthorization.ORG,
                        ACTIVE = request.ACTIVE ?? baseUserAuthorization.ACTIVE,
                        CREATED_BY = baseUserAuthorization.CREATED_BY,
                        CREATED_DATETIME = baseUserAuthorization.CREATED_DATETIME,
                        UPDATED_BY = updatedBy,
                        UPDATED_DATETIME = now
                    };

                    _context.UsersAuthorize.Add(newAuth);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string authCode)
        {
            var entities = await _context.UsersAuthorize
                .Where(x => x.AUTH_CODE == authCode)
                .ToListAsync();

            if (entities.Any())
            {
                _context.UsersAuthorize.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FacilitySelectionDto>> GetUserFacilitiesByAuthCodeAsync(string authCode)
        {
            var facilities = await _context.UsersAuthorize
                .Where(u => u.AUTH_CODE == authCode)
                .Select(u => new FacilitySelectionDto
                {
                    SITE_CODE = u.SITE_CODE!,
                    DOMAIN_CODE = u.DOMAIN_CODE!,
                    FACT_CODE = u.FACT_CODE!
                })
                .Distinct()
                .ToListAsync();

            return facilities;
        }

        public async Task<IEnumerable<CM_USERS_AUTHORIZE>> GetByAuthCodeForFacilitiesAsync(string authCode)
        {
            return await _context.UsersAuthorize
                .Where(u => u.AUTH_CODE == authCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<FacilitySelectionDto>> GetUserFacilitiesByUserIdAsync(string userId)
        {
            var facilities = await _context.UsersAuthorize
                .Where(u => u.USERID == userId)
                .Select(u => new FacilitySelectionDto
                {
                    SITE_CODE = u.SITE_CODE!,
                    DOMAIN_CODE = u.DOMAIN_CODE!,
                    FACT_CODE = u.FACT_CODE!
                })
                .Distinct()
                .ToListAsync();

            return facilities;
        }
    }
}