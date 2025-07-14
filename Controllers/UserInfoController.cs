using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBACapi.Controllers
{
    //[Authorize]
    // [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        [HttpGet("current")]
        public IActionResult GetCurrentUserInfo()
        {
            var userName = UserHelper.GetCurrentUsername(User.Identity);

            // Retrieve user information from the claims
            // var fullName = User.Identity?.Name;
            // string? userName = null;
            // if (!string.IsNullOrEmpty(fullName))
            // {
            //     var parts = fullName.Split('\\');
            //     userName = parts.Length > 1 ? parts[1] : parts[0];
            // }

            // if (string.IsNullOrEmpty(userName))
            // {
            //     return Unauthorized();
            // }

            // Return user information
            return Ok(new
            {
                UserName = userName
            });
        }
    }
}