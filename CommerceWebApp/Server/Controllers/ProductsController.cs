using CommerceWebApp.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ILogger<ProductsController> logger
        )
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string json = await System.IO.File.ReadAllTextAsync("Data/products.json");
            IEnumerable<Product> products = JsonConvert.DeserializeObject<IEnumerable<Product>>(json)!; // we deserialize it in order to map to our version of `Product`
            return StatusCode(200, JsonConvert.SerializeObject(products));
        }
    }
}