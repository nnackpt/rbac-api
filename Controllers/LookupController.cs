using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _service;

        public LookupController(ILookupService service)
        {
            _service = service;
        }

        [HttpGet("roles/{appCode}")]
        public async Task<IActionResult> GetRoles(string appCode)
        {
            return Ok(await _service.GetRolesByAppAsync(appCode));
        }

        [HttpGet("functions/{appCode}")]
        public async Task<IActionResult> GetFunctions(string appCode)
        {
            return Ok(await _service.GetFunctionsByAppAsync(appCode));
        }
    }
}