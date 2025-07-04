using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;

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

        // GET: api/UserAuthorize/{authCode}
        [HttpGet("{authCode}")]
        public async Task<ActionResult<CM_USERS_AUTHORIZE>> GetUserAuthorizeById(string authCode)
        {
            var userAuthorize = await _usersAuthorizeService.GetByIdAsync(authCode);

            if (userAuthorize == null)
            {
                return NotFound();
            }
            return Ok(userAuthorize);
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
            if (string.IsNullOrEmpty(createdBy))
            {
                createdBy = "anonymous";
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
            var userAuthorize = await _usersAuthorizeService.GetByIdAsync(authCode);
            if (userAuthorize == null)
            {
                return NotFound();
            }

            // APP_CODE และ USERID แก้ไขไม่ได้
            // userAuthorize.APP_CODE = userAuthorize.APP_CODE;
            // userAuthorize.USERID = userAuthorize.USERID;
            if (!string.IsNullOrEmpty(request.ROLE_CODE))
                userAuthorize.ROLE_CODE = request.ROLE_CODE;
            if (!string.IsNullOrEmpty(request.FNAME))
                userAuthorize.FNAME = request.FNAME;
            if (!string.IsNullOrEmpty(request.LNAME))
                userAuthorize.LNAME = request.LNAME;
            if (!string.IsNullOrEmpty(request.ORG))
                userAuthorize.ORG = request.ORG;
            if (request.ACTIVE.HasValue)
                userAuthorize.ACTIVE = request.ACTIVE;
            // ไม่รองรับการแก้ไข Facilities ในการ update เดี่ยวนี้ (ถ้าต้องการแจ้งเพิ่ม)

            var updatedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(updatedBy))
            {
                updatedBy = "anonymous";
            }
            userAuthorize.UPDATED_BY = updatedBy;
            userAuthorize.UPDATED_DATETIME = DateTime.UtcNow;

            await _usersAuthorizeService.UpdateAsync(userAuthorize);
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