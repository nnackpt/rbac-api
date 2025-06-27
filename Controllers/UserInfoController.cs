using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RBACapi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        [HttpGet("current")]
        public IActionResult GetCurrentUserInfo()
        {
            // Retrieve user information from the claims
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            // Return user information
            return Ok(new
            {
                UserName = userName
            });
        }
    }
}