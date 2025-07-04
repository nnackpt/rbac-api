using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CmAppFunctionsController : ControllerBase
    {
        private readonly IAppFunctionsService _service;

        public CmAppFunctionsController(IAppFunctionsService service)
        {
            _service = service;
        }

        // Get all Applications function
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // Get App Functions by Id
        [HttpGet("{funcCode}")]
        public async Task<IActionResult> GetById(string funcCode)
        {
            var result = await _service.GetByIdAsync(funcCode);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Create App Functions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CM_APPS_FUNCTIONS func)
        {
            var createdBy = User.Identity?.Name;
            if (!string.IsNullOrEmpty(createdBy) && createdBy.Contains("\\"))
            {
                createdBy = createdBy.Split('\\')[1];
            }
            if (string.IsNullOrEmpty(createdBy))
            {
                createdBy = "anonymous";
            }
            func.CREATED_BY = createdBy;
            func.CREATED_DATETIME = DateTimeOffset.UtcNow;
            func.UPDATED_BY = null;
            func.UPDATED_DATETIME = null;
            var result = await _service.CreateAsync(func);
            return CreatedAtAction(nameof(GetById), new { funcCode = result.FUNC_CODE }, result);
        }

        // Update by id
        [HttpPut("{funcCode}")]
        public async Task<IActionResult> Update(string funcCode, [FromBody] CM_APPS_FUNCTIONS updated)
        {
            var updatedBy = User.Identity?.Name;
            if (!string.IsNullOrEmpty(updatedBy) && updatedBy.Contains("\\"))
            {
                updatedBy = updatedBy.Split('\\')[1];
            }
            if (string.IsNullOrEmpty(updatedBy))
            {
                updatedBy = "anonymous";
            }
            updated.UPDATED_BY = updatedBy;
            updated.UPDATED_DATETIME = DateTimeOffset.UtcNow;
            var result = await _service.UpdateAsync(funcCode, updated);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Delete by id
        [HttpDelete("{funcCode}")]
        public async Task<IActionResult> Delete(string funcCode)
        {
            var deleted = await _service.DeleteAsync(funcCode);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}