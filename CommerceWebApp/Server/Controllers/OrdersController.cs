using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        [HttpPost("")]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            
            return StatusCode(200, 1);
        }
    }
}