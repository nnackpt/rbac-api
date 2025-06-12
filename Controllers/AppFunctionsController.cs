using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CmAppFunctionsController : ControllerBase
    {
        private readonly AppFunctionsService _service;

        public CmAppFunctionsController(AppFunctionsService service)
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
            var result = await _service.CreateAsync(func);
            return CreatedAtAction(nameof(GetById), new { funcCode = result.FUNC_CODE }, result);
        }

        // Update by id
        [HttpPut("{funcCode}")]
        public async Task<IActionResult> Update(string funcCode, [FromBody] CM_APPS_FUNCTIONS updated)
        {
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