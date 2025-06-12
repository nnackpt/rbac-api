using Microsoft.AspNetCore.Mvc;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMAppRolesController : ControllerBase
    {
        private readonly IAppRolesService _service;

        public CMAppRolesController(IAppRolesService service)
        {
            _service = service;
        }

        // Get All App Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CM_APPS_ROLES>>> GetAll()
        {
            return Ok(await _service.GetAllRolesAsync());
        }
    }
}