using CommerceWebApp.Shared;
using CommerceWebApp.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CommerceWebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsService productsService;

        public ProductsController(
            ProductsService productsService
        ) {
            this.productsService = productsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            IEnumerable<Product> products = await Task.Run(() => {
                return this.productsService.Products;
            });
            
            return StatusCode(200, JsonConvert.SerializeObject(products));
        }
    }
}