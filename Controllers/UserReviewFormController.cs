using Microsoft.AspNetCore.Mvc;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserReviewFormController : ControllerBase
    {
        private readonly IUserReviewFormService _userReviewFormService;
        private readonly ILogger<UserReviewFormController> _logger;

        public UserReviewFormController(IUserReviewFormService userReviewFormService, ILogger<UserReviewFormController> logger)
        {
            _userReviewFormService = userReviewFormService;
            _logger = logger;
        }

        /// <summary>
        /// Download user review form as XLSX file
        /// </summary>
        /// <param name="applicationName">Application name for the review</param>
        /// <param name="roleName">Role name for the review</param>
        /// <returns>XLSX file for user review</returns>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadUserReviewForm([FromQuery] string? applicationName = null, [FromQuery] string? roleName = null)
        {
            try
            {
                _logger.LogInformation($"Generating user review form for Application: {applicationName}, Role: {roleName}");

                var excelBytes = await _userReviewFormService.GenerateUserReviewFormAsync(applicationName, roleName);

                var fileName = $"Application_User_Review_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                _logger.LogInformation($"Generated user review form with {excelBytes.Length} bytes");

                return File(excelBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating user review form");
                return StatusCode(500, new { message = "An error occurred while generating user review form", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available applications and roles for form generation
        /// </summary>
        /// <returns>List of applications and roles</returns>
        [HttpGet("options")]
        public async Task<IActionResult> GetFormOptions()
        {
            try
            {
                _logger.LogInformation("Getting form options");

                var options = await _userReviewFormService.GetFormOptionsAsync();

                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting form options");
                return StatusCode(500, new { message = "An error occurred while getting form options", error = ex.Message });
            }
        }
    }
}