using Microsoft.AspNetCore.Mvc;
using RBACapi.Models.Dtos;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthUsersController : ControllerBase
    {
        private readonly IAuthUsersService _authUsersService;
        private readonly ILogger<AuthUsersController> _logger;

        public AuthUsersController(IAuthUsersService authUsersService, ILogger<AuthUsersController> logger)
        {
            _authUsersService = authUsersService;
            _logger = logger;
        }

        /// <summary>
        /// Get all authorized users with application and role information
        /// </summary>
        /// <returns>List of authorized users</returns>
        [HttpGet]
        public async Task<ActionResult<List<AuthUsersResponseDto>>> GetAllAuthUsers()
        {
            try
            {
                _logger.LogInformation("Getting all authorized users");
                
                var authUsers = await _authUsersService.GetAllAuthUsersAsync();
                
                _logger.LogInformation($"Retrieved {authUsers.Count} authorized users");
                
                return Ok(authUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting authorized users");
                return StatusCode(500, new { message = "An error occurred while retrieving authorized users", error = ex.Message });
            }
        }
    }
}