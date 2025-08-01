using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;
using RBACapi.Utils;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CmApplicationsController : ControllerBase
    {
        private readonly IApplicationsService _service;

        public CmApplicationsController(IApplicationsService service)
        {
            _service = service;
        }

        // Get All Applications
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _service.GetAllApplicationsAsync();
            return Ok(apps);
        }

        // Get Applications by Code
        [HttpGet("{code}")]
        public async Task<IActionResult> GetById(string code)
        {
            var app = await _service.GetApplicationByCodeAsync(code);
            if (app == null) return NotFound();
            return Ok(app);
        }

        // Create new Applications
        [HttpPost]
        public async Task<IActionResult> Create(CM_APPLICATIONS application)
        {
            // var createdBy = User.Identity?.Name;
            // if (!string.IsNullOrEmpty(createdBy) && createdBy.Contains("\\"))
            // {
            //     createdBy = createdBy.Split('\\')[1];
            // }
            // if (string.IsNullOrEmpty(createdBy))
            // {
            //     createdBy = "anonymous";
            // }
            // application.CREATED_BY = createdBy;
            application.CREATED_BY = UserHelper.GetCurrentUsername(User.Identity);
            application.CREATED_DATETIME = DateTimeOffset.UtcNow;
            application.UPDATED_BY = null;
            application.UPDATED_DATETIME = null;
            await _service.CreateApplicationAsync(application);
            return CreatedAtAction(nameof(GetById), new { code = application.APP_CODE }, application);
        }

        // Update Applications by Code
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, CM_APPLICATIONS application)
        {
            if (code != application.APP_CODE) return BadRequest();
            // var updatedBy = User.Identity?.Name;
            // if (!string.IsNullOrEmpty(updatedBy) && updatedBy.Contains("\\"))
            // {
            //     updatedBy = updatedBy.Split('\\')[1];
            // }
            // if (string.IsNullOrEmpty(updatedBy))
            // {
            //     updatedBy = "anonymous";
            // }
            // application.UPDATED_BY = updatedBy;
            application.UPDATED_BY = UserHelper.GetCurrentUsername(User.Identity);
            application.UPDATED_DATETIME = DateTimeOffset.UtcNow;
            var updated = await _service.UpdateApplicationAsync(application);
            return updated ? NoContent() : NotFound();
        }

        // Delete Applications by Code
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var deleted = await _service.DeleteApplicationAsync(code);
            return deleted ? NoContent() : NotFound();
        }
    }
}