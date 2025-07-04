using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RbacController : ControllerBase
    {
        private readonly IRbacService _service;

        public RbacController(IRbacService service)
        {
            _service = service;
        }

        // Get All RBAC
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CM_RBAC>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // Get RBAC by Code
        [HttpGet("{rbacCode}")]
        public async Task<ActionResult<CM_RBAC>> GetByCode(string rbacCode)
        {
            var rbac = await _service.GetByCodeAsync(rbacCode);
            if (rbac == null) return NotFound();
            return Ok(rbac);
        }

        [HttpPost]
        public async Task<ActionResult<CM_RBAC>> Create(RbacRequest req)
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
            req.CREATED_BY = createdBy;
            var created = await _service.CreateAsync(req);
            return Ok(created);
        }

        [HttpPut("{rbacCode}")]
        public async Task<IActionResult> Update(string rbacCode, RbacUpdateRequest req)
        {
            var updatedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(updatedBy))
            {
                updatedBy = "anonymous";
            }
            req.UPDATED_BY = updatedBy;
            var updated = await _service.UpdateAsync(rbacCode, req);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{rbacCode}")]
        public async Task<IActionResult> Delete(string rbacCode)
        {
            var deleted = await _service.DeleteAsync(rbacCode);
            if (deleted == null) return NotFound();
            return Ok(deleted);
        }
    }
}