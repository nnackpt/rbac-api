using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    // [Authorize]
    // [ApiController]
    // [Route("api/auth")]
    // public class AuthController : ControllerBase
    // {
    //     private readonly IUsersAuthorizeService _usersAuthorizationService;
    //
    //     public AuthController(IUsersAuthorizeService usersAuthorizeService)
    //     {
    //         _usersAuthorizationService = usersAuthorizeService;
    //     }
    //
    //     [HttpPost("login")]
    //     public async Task<ActionResult<LoginResponseDto>> Login()
    //     {
    //         var windowsUsername = User.Identity?.Name;
    //         if (string.IsNullOrEmpty(windowsUsername))
    //         {
    //             return Unauthorized("Windows identity cloud not be determined.");
    //         }
    //
    //         var userId = windowsUsername.Contains('\\') ? windowsUsername.Split('\\').Last() : windowsUsername;
    //
    //         var authorizations = await _usersAuthorizationService.GetByUserIdAsync(userId);
    //
    //         if (authorizations == null || !authorizations.Any())
    //         {
    //             return StatusCode(StatusCodes.Status403Forbidden, new { Message = $"User '{userId}' is not authorized to access this application." });
    //         }
    //
    //         var response = new LoginResponseDto
    //         {
    //             Message = "Login successful. User is authorized.",
    //             UserId = userId,
    //             Authorizations = authorizations.Select(a => new UserAuthorizationDto
    //             {
    //                 AUTH_CODE = a.AUTH_CODE,
    //                 ROLE_CODE = a.ROLE_CODE,
    //                 APP_CODE = a.APP_CODE,
    //                 SITE_CODE = a.SITE_CODE
    //             })
    //         };
    //
    //         return Ok(response);
    //     }
    // }
}