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
                _logger.LogInformation($"Generating user review form for Application: {applicationName ?? "ALL"}, Role: {roleName ?? "ALL"}");

                // Clean up parameters
                applicationName = string.IsNullOrWhiteSpace(applicationName) ? null : applicationName.Trim();
                roleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim();

                var excelBytes = await _userReviewFormService.GenerateUserReviewFormAsync(applicationName, roleName);

                if (excelBytes == null || excelBytes.Length == 0)
                {
                    _logger.LogWarning("Generated Excel file is empty");
                    return BadRequest(new { message = "Generated file is empty" });
                }

                // Generate dynamic filename based on parameters
                var fileNameParts = new List<string> { "Application_User_Review" };
                if (!string.IsNullOrEmpty(applicationName))
                {
                    fileNameParts.Add(applicationName.Replace(" ", "_"));
                }
                if (!string.IsNullOrEmpty(roleName))
                {
                    fileNameParts.Add(roleName.Replace(" ", "_"));
                }
                fileNameParts.Add(DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                var fileName = string.Join("_", fileNameParts) + ".xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                _logger.LogInformation($"Generated user review form '{fileName}' with {excelBytes.Length} bytes");

                // Add proper headers for file download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                return File(excelBytes, contentType, fileName);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Invalid parameters provided");
                return BadRequest(new { message = "Invalid parameters provided", error = argEx.Message });
            }
            catch (InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, "No data found for the specified criteria");
                return NotFound(new { message = "No data found for the specified criteria", error = opEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating user review form");
                return StatusCode(500, new
                {
                    message = "An error occurred while generating user review form",
                    error = ex.Message,
                    details = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
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