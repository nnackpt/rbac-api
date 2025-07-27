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
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UsersAuthorizeService(ApplicationDbContext context, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _context = context;
            _dbContextFactory = dbContextFactory;
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

        // UpdateAsync
        public async Task UpdateAsync(string authCode, UsersAuthorizeUpdateRequestDto request, string updatedBy)
        {
            var now = DateTime.UtcNow;

            var baseUserAuthorization = await _context.UsersAuthorize
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.AUTH_CODE == authCode);

            if (baseUserAuthorization == null)
                throw new InvalidOperationException($"No existing authorization found for AUTH_CODE: '{authCode}'.");

            var allExistingFacilitiesForUserAppRole = await _context.UsersAuthorize
                .AsNoTracking()
                .Where(ua => ua.USERID == baseUserAuthorization.USERID && 
                            ua.APP_CODE == baseUserAuthorization.APP_CODE && 
                            ua.ROLE_CODE == baseUserAuthorization.ROLE_CODE)
                .ToListAsync();

            if (request.Facilities != null)
            {
                var currentFacilitiesInDb = allExistingFacilitiesForUserAppRole.Select(ea => new
                {
                    ea.SITE_CODE,
                    ea.DOMAIN_CODE,
                    ea.FACT_CODE,
                    ea.AUTH_CODE
                }).ToList();

                var requestedFacilities = request.Facilities.Select(rf => new
                {
                    rf.SITE_CODE,
                    rf.DOMAIN_CODE,
                    rf.FACT_CODE
                }).ToList();

                // Find facilities to remove (exist in DB but not in request)
                var facilitiesToRemove = currentFacilitiesInDb
                    .Where(current => !requestedFacilities.Any(requested =>
                        requested.SITE_CODE == current.SITE_CODE &&
                        requested.DOMAIN_CODE == current.DOMAIN_CODE &&
                        requested.FACT_CODE == current.FACT_CODE))
                    .ToList();

                // Find facilities to add (exist in request but not in DB)
                var facilitiesToAdd = requestedFacilities
                    .Where(requested => !currentFacilitiesInDb.Any(current =>
                        current.SITE_CODE == requested.SITE_CODE &&
                        current.DOMAIN_CODE == requested.DOMAIN_CODE &&
                        current.FACT_CODE == requested.FACT_CODE))
                    .ToList();

                // Find facilities to update (exist in both request and DB)
                var facilitiesToUpdate = currentFacilitiesInDb
                    .Where(current => requestedFacilities.Any(requested =>
                        requested.SITE_CODE == current.SITE_CODE &&
                        requested.DOMAIN_CODE == current.DOMAIN_CODE &&
                        requested.FACT_CODE == current.FACT_CODE))
                    .ToList();

                // Remove facilities that are no longer in the request
                if (facilitiesToRemove.Any())
                {
                    var authCodesToDelete = facilitiesToRemove.Select(f => f.AUTH_CODE).ToList();
                    var entitiesToDelete = await _context.UsersAuthorize
                        .Where(ua => authCodesToDelete.Contains(ua.AUTH_CODE))
                        .ToListAsync();

                    _context.UsersAuthorize.RemoveRange(entitiesToDelete);
                }

                // Update existing facilities (only update other fields, don't recreate)
                if (facilitiesToUpdate.Any())
                {
                    var authCodesToUpdate = facilitiesToUpdate.Select(f => f.AUTH_CODE).ToList();
                    var entitiesToUpdate = await _context.UsersAuthorize
                        .Where(ua => authCodesToUpdate.Contains(ua.AUTH_CODE))
                        .ToListAsync();

                    foreach (var entity in entitiesToUpdate)
                    {
                        entity.ROLE_CODE = request.ROLE_CODE ?? entity.ROLE_CODE;
                        entity.FNAME = request.FNAME ?? entity.FNAME;
                        entity.LNAME = request.LNAME ?? entity.LNAME;
                        entity.ORG = request.ORG ?? entity.ORG;
                        entity.ACTIVE = request.ACTIVE ?? entity.ACTIVE;
                        entity.UPDATED_BY = updatedBy;
                        entity.UPDATED_DATETIME = now;

                        _context.Entry(entity).State = EntityState.Modified;
                    }
                }

                // Add only NEW facilities with new AUTH_CODEs
                if (facilitiesToAdd.Any())
                {
                    var appCodePrefix = baseUserAuthorization.APP_CODE.StartsWith("APP_")
                        ? baseUserAuthorization.APP_CODE.Substring(4)
                        : baseUserAuthorization.APP_CODE;

                    // Get all AUTH_CODEs with same pattern to find the highest number
                    var authCodes = await _context.UsersAuthorize
                        .Where(ua => ua.APP_CODE == baseUserAuthorization.APP_CODE &&
                                    ua.AUTH_CODE.StartsWith($"{appCodePrefix}_USER_"))
                        .Select(ua => ua.AUTH_CODE)
                        .ToListAsync();

                    // Find the highest existing USER number
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

                    // Create new authorizations only for BRAND NEW facilities
                    foreach (var facilityToAdd in facilitiesToAdd)
                    {
                        var originalFacility = request.Facilities.First(rf =>
                            rf.SITE_CODE == facilityToAdd.SITE_CODE &&
                            rf.DOMAIN_CODE == facilityToAdd.DOMAIN_CODE &&
                            rf.FACT_CODE == facilityToAdd.FACT_CODE);

                        string newAuthCode = $"{appCodePrefix}_USER_{nextUserNumber:000}";

                        var newAuth = new CM_USERS_AUTHORIZE
                        {
                            AUTH_CODE = newAuthCode,
                            APP_CODE = baseUserAuthorization.APP_CODE,
                            ROLE_CODE = request.ROLE_CODE ?? baseUserAuthorization.ROLE_CODE,
                            USERID = baseUserAuthorization.USERID,
                            SITE_CODE = originalFacility.SITE_CODE,
                            DOMAIN_CODE = originalFacility.DOMAIN_CODE,
                            FACT_CODE = originalFacility.FACT_CODE,
                            FNAME = request.FNAME ?? baseUserAuthorization.FNAME,
                            LNAME = request.LNAME ?? baseUserAuthorization.LNAME,
                            ORG = request.ORG ?? baseUserAuthorization.ORG,
                            ACTIVE = request.ACTIVE ?? baseUserAuthorization.ACTIVE ?? true,
                            CREATED_BY = updatedBy,
                            CREATED_DATETIME = now,
                            UPDATED_BY = updatedBy,
                            UPDATED_DATETIME = now
                        };

                        _context.UsersAuthorize.Add(newAuth);
                        nextUserNumber++;
                    }
                }
            }
            else
            {
                // If no facilities provided, just update the existing record fields for this authCode only
                var existingRecord = await _context.UsersAuthorize
                    .FirstOrDefaultAsync(ua => ua.AUTH_CODE == authCode);

                if (existingRecord != null)
                {
                    existingRecord.ROLE_CODE = request.ROLE_CODE ?? existingRecord.ROLE_CODE;
                    existingRecord.FNAME = request.FNAME ?? existingRecord.FNAME;
                    existingRecord.LNAME = request.LNAME ?? existingRecord.LNAME;
                    existingRecord.ORG = request.ORG ?? existingRecord.ORG;
                    existingRecord.ACTIVE = request.ACTIVE ?? existingRecord.ACTIVE;
                    existingRecord.UPDATED_BY = updatedBy;
                    existingRecord.UPDATED_DATETIME = now;

                    _context.Entry(existingRecord).State = EntityState.Modified;
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

        public async Task DeleteByUserIdAppCodeRoleCodeAsync(string userId, string appCode, string roleCode)
        {
            var entities = await _context.UsersAuthorize
                .Where(x => x.USERID == userId && x.APP_CODE == appCode && x.ROLE_CODE == roleCode)
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

        public async Task<IEnumerable<FacilitySelectionDto>> GetAllAvailableFacilitiesAsync()
        {
            var facilities = await _context.UsersAuthorize
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

        public async Task<IEnumerable<FacilitySelectionDto>> GetUserFacilitiesByUserIdAppCodeRoleCodeAsync(string userId, string appCode, string roleCode)
        {
            var facilities = await _context.UsersAuthorize
                .Where(u => u.USERID == userId && u.APP_CODE == appCode && u.ROLE_CODE == roleCode)
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