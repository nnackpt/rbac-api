using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CmAppRolesController : ControllerBase
    {
        private readonly IAppRolesService _service;

        public CmAppRolesController(IAppRolesService service)
        {
            _service = service;
        }

        // Get All App Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CM_APPS_ROLES>>> GetAll()
        {
            return Ok(await _service.GetAllRolesAsync());
        }

        // Get App Role by Code
        [HttpGet("{code}")]
        public async Task<ActionResult<CM_APPS_ROLES>> GetByCode(string code)
        {
            var role = await _service.GetRoleByCodeAsync(code);
            if (role == null) return NotFound();
            return Ok(role);
        }

        // Create new App Role
        [HttpPost]
        public async Task<ActionResult<CM_APPS_ROLES>> Create(CM_APPS_ROLES role)
        {
            var created = await _service.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetByCode), new { code = created.ROLE_CODE }, created);
        }

        // Update App Role by Code
        [HttpPut("{code}")]
        public async Task<ActionResult<CM_APPS_ROLES>> Update(string code, CM_APPS_ROLES updated)
        {
            var result = await _service.UpdateRoleAsync(code, updated);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Delete App role by Code
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var success = await _service.DeleteRoleAsync(code);
            return success ? NoContent() : NotFound();
        }
    }
}