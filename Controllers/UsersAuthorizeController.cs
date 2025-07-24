// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;
using RBACapi.Utils;

namespace RBACapi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/CmUserAuthorize")]
    public class CmUsersAuthorizeController : ControllerBase
    {
        private readonly IUsersAuthorizeService _usersAuthorizeService;

        public CmUsersAuthorizeController(IUsersAuthorizeService usersAuthorizeService)
        {
            _usersAuthorizeService = usersAuthorizeService;
        }

        // GET: api/UserAuthorize
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CM_USERS_AUTHORIZE>>> GetAllUsersAuthorize()
        {
            var usersAuthorize = await _usersAuthorizeService.GetAllAsync();
            return Ok(usersAuthorize);
        }

        // GET: api/CmUserAuthorize/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CM_USERS_AUTHORIZE>>> GetUserAuthorizeByUserId(string userId)
        {
            var userAuthorizations = await _usersAuthorizeService.GetByUserIdAsync(userId);
            return Ok(userAuthorizations);
        }

        // GET: api/CmUserAuthorize/user/{userId}/facilities
        [HttpGet("user/{userId}/facilities")]
        public async Task<ActionResult<IEnumerable<FacilitySelectionDto>>> GetUserFacilitiesByUserId(string userId)
        {
            var facilities = await _usersAuthorizeService.GetUserFacilitiesByUserIdAsync(userId);
            if (facilities == null || !facilities.Any())
            {
                return NotFound();
            }
            return Ok(facilities);
        }

        // GET: api/UserAuthorize/{authCode}
        [HttpGet("{authCode}")]
        public async Task<ActionResult<IEnumerable<CM_USERS_AUTHORIZE>>> GetUserAuthorizeById(string authCode)
        {
            var userAuthorize = await _usersAuthorizeService.GetByIdAsync(authCode);

            if (userAuthorize == null)
            {
                return NotFound();
            }
            return Ok(userAuthorize);
        }

        // GET: api/CmUserAuthorize/{authCode}/facilities
        [HttpGet("{authCode}/facilities")] 
        public async Task<ActionResult<IEnumerable<FacilitySelectionDto>>> GetUserFacilities(string authCode)
        {
            var facilities = await _usersAuthorizeService.GetUserFacilitiesByAuthCodeAsync(authCode);
            if (facilities == null || !facilities.Any())
            {
                return NotFound();
            }
            return Ok(facilities);
        }

        // POST: api/UserAuthorize
        [HttpPost]
        // [Authorize]
        public async Task<ActionResult<IEnumerable<UsersAuthorizeCreateResponseDto>>> CreateUsersAuthorize(UsersAuthorizeCreateRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdBy = User.Identity?.Name;
            if (!string.IsNullOrEmpty(createdBy) && createdBy.Contains("\\"))
            {
                createdBy = createdBy.Split('\\')[1];
            }
            if (string.IsNullOrEmpty(createdBy))
            {
                createdBy = "anonymous";
                // createdBy = UserHelper.GetCurrentUsername(User.Identity);
            }

            var createdAuthorizations = await _usersAuthorizeService.CreateAsync(request, createdBy);

            var responseDtos = createdAuthorizations.Select(a => new UsersAuthorizeCreateResponseDto {
                AUTH_CODE = a.AUTH_CODE,
                APP_CODE = a.APP_CODE,
                ROLE_CODE = a.ROLE_CODE,
                USERID = a.USERID,
                SITE_CODE = a.SITE_CODE,
                DOMAIN_CODE = a.DOMAIN_CODE,
                FACT_CODE = a.FACT_CODE,
                ACTIVE = a.ACTIVE
            }).ToList();

            return Ok(responseDtos);
        }

        // PUT: api/UserAuthorize/{authCode}
        [HttpPut("{authCode}")]
        public async Task<IActionResult> UpdateUserAuthorize(string authCode, UsersAuthorizeUpdateRequestDto request)
        {
            var existingUserAuthorizations = await _usersAuthorizeService.GetByAuthCodeForFacilitiesAsync(authCode);
            if (!existingUserAuthorizations.Any())
            {
                return NotFound();
            }

            var updatedBy = UserHelper.GetCurrentUsername(User.Identity);

            await _usersAuthorizeService.UpdateAsync(authCode, request, updatedBy);
            return NoContent();
        }

        // DELETE: api/UserAuthorize/{authCode}
        [HttpDelete("{authCode}")]
        public async Task<IActionResult> DeleteUserAuthorize(string authCode)
        {
            var userAuthorize = await _usersAuthorizeService.GetByIdAsync(authCode);
            if (userAuthorize == null)
            {
                return NotFound();
            }
            await _usersAuthorizeService.DeleteAsync(authCode);
            return NoContent();
        }
    }
}